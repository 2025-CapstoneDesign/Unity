using OpenCVForUnity.UnityUtils.Helper;
using HoloLensCameraStream;
using HoloLensWithOpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HLCameraStream2MatHelper), typeof(ImageOptimizationHelper))]
public class OptimizedArUcoMarkerDetection : MonoBehaviour
{
    public static Dictionary<int, MarkerData> markerMap = new Dictionary<int, MarkerData>();

    public float markerLength = 0.05f;
    public bool enableDownScaling = false;
    public bool useFlippedZ = true;
    
    // 마커 위치 안정화를 위한 설정
    [Header("마커 안정화 설정")]
    public bool enableSmoothing = true;
    [Range(1, 30)]
    public int smoothingFrameCount = 10;
    [Range(0.0f, 1.0f)]
    public float positionSmoothFactor = 0.8f;
    [Range(0.0f, 1.0f)]
    public float rotationSmoothFactor = 0.5f;

    private HLCameraStream2MatHelper camHelper;
    private ArucoDetector detector;
    private ImageOptimizationHelper optimizationHelper;

    private Texture2D previewTexture;
    public GameObject previewQuad;
    public bool displayCameraPreview = true;

    private Mat camMatrix;
    private MatOfDouble distCoeffs;
    private Mat scaledCamMatrix;
    private MatOfDouble scaledDistCoeffs;

    private List<Mat> corners = new List<Mat>();
    private Mat ids;
    private List<Mat> rejectedCorners = new List<Mat>();
    private bool isDetecting = false;

    void Start()
    {
        Debug.Log("[NS] : 마커 감지 시도");
        optimizationHelper = GetComponent<ImageOptimizationHelper>();
        camHelper = GetComponent<HLCameraStream2MatHelper>();
        camHelper.Initialize();

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
        camHelper.frameMatAcquired += OnFrameMatAcquired;
#endif

        InitArucoDetector();
        InitCameraParameters();
    }

    private void InitArucoDetector()
    {
        var dictionary = Objdetect.getPredefinedDictionary(Objdetect.DICT_6X6_250);
        DetectorParameters detectorParams = new DetectorParameters();
        detectorParams.set_cornerRefinementMethod(Objdetect.CORNER_REFINE_SUBPIX);
        detectorParams.set_useAruco3Detection(true);
        detector = new ArucoDetector(dictionary, detectorParams);
    }

    private void InitCameraParameters()
    {
        camMatrix = new Mat(3, 3, CvType.CV_64FC1);
        double scaleX = 1280.0 / 640.0;    // 2.0
        double scaleY = 720.0  / 480.0;    // 1.5

        camMatrix.put(0,0, 370.6985 * scaleX, 0, 363.9156 * scaleX);
        camMatrix.put(1,0, 0, 361.9248 * scaleY, 273.5386 * scaleY);
        camMatrix.put(2,0, 0, 0, 1);

        distCoeffs = new MatOfDouble(-0.3952, 2.5100, 0.0587, -0.1033, -11.2717);

        scaledCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
        scaledDistCoeffs = new MatOfDouble(distCoeffs.clone());
    }

    public void OnWebCamTextureToMatHelperInitialized()
    {
        var mat = camHelper.GetMat();
        if (mat != null && previewQuad != null)
        {
            previewTexture = new Texture2D(mat.width(), mat.height(), TextureFormat.RGB24, false);
            previewQuad.GetComponent<Renderer>().material.mainTexture = previewTexture;
            previewQuad.transform.localScale = new Vector3(mat.width() / (float)mat.height(), 1f, 1f);
            previewQuad.SetActive(displayCameraPreview);
        }
    }

    public void OnWebCamTextureToMatHelperDisposed()
    {
        if (previewTexture != null)
            Destroy(previewTexture);
    }

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
    private void OnFrameMatAcquired(Mat frame, Matrix4x4 proj, Matrix4x4 camToWorld, CameraIntrinsics intrinsics)
    {
        Debug.Log("[NS] : 마커 감지 호출");
        if (isDetecting || frame == null || frame.empty()) return;
        isDetecting = true;
        Debug.Log("[NS] : 마커 감지 시작");
        try
        {
            using (Mat frameCopy = frame.clone())
            using (Mat inputMat = enableDownScaling ? optimizationHelper.GetDownScaleMat(frameCopy) : frameCopy.clone())
            using (Mat undistorted = new Mat())
            {
                float scale = (float)inputMat.width() / frameCopy.width();
                UndistortImage(inputMat, undistorted, scale);

                ClearAndDisposePrevious();
                detector.detectMarkers(undistorted, corners, ids, rejectedCorners);

                if (ids != null && ids.total() > 0)
                {
                    for (int i = 0; i < ids.total(); i++)
                        ProcessDetectedMarker(i, camToWorld, scale);
                }

                if (previewTexture != null)
                    OpenCVForUnity.UnityUtils.Utils.matToTexture2D(undistorted, previewTexture);
            }
        }
        catch (Exception e)
        {
        }
        finally
        {
            isDetecting = false;
        }
    }
#endif

    private void UndistortImage(Mat input, Mat output, float scale)
    {
        if (enableDownScaling && Math.Abs(scale - 1f) > 0.00001f)
        {
            RecalculateCameraMatrix(scale);
            Calib3d.undistort(input, output, scaledCamMatrix, scaledDistCoeffs);
        }
        else
        {
            Calib3d.undistort(input, output, camMatrix, distCoeffs);
        }
    }

    private void ProcessDetectedMarker(int index, Matrix4x4 camToWorld, float scale)
    {
        using (Mat cornerMat = corners[index].reshape(2, 4))
        using (MatOfPoint2f points = new MatOfPoint2f(cornerMat))
        using (Mat rvec = new Mat())
        using (Mat tvec = new Mat())
        {
            if (enableDownScaling && Math.Abs(scale - 1f) > 0.00001f)
                Calib3d.solvePnP(CreateObjPoints(), points, scaledCamMatrix, scaledDistCoeffs, rvec, tvec);
            else
                Calib3d.solvePnP(CreateObjPoints(), points, camMatrix, distCoeffs, rvec, tvec);

            SaveMarkerData((int)ids.get(index, 0)[0], rvec, tvec, camToWorld);
        }
    }

    private void SaveMarkerData(int markerId, Mat rvec, Mat tvec, Matrix4x4 camToWorld)
    {
        Matrix4x4 markerToCamera = GetTransformMatrix(rvec, tvec);
        Matrix4x4 markerToWorld = camToWorld * markerToCamera;

        Vector3 pos = markerToWorld.GetColumn(3);
        Quaternion rot = Quaternion.LookRotation(markerToWorld.GetColumn(2), markerToWorld.GetColumn(1));

        // 👉 눈 위치 보정
        Vector3 eyeOffset = new Vector3(0f, 0.03f, -0.05f);
        Vector3 worldOffset = camToWorld.MultiplyVector(eyeOffset);
        pos += worldOffset;

        // 마커 데이터 업데이트 또는 생성
        if (markerMap.ContainsKey(markerId) && enableSmoothing)
        {
            // 기존 마커 데이터에 새 위치/회전 정보 추가하고 평활화 적용
            markerMap[markerId].UpdatePosition(pos, positionSmoothFactor, smoothingFrameCount);
            markerMap[markerId].UpdateRotation(rot, rotationSmoothFactor, smoothingFrameCount);
            
            pos = markerMap[markerId].position;    // 평활화된 위치 사용
            rot = markerMap[markerId].rotation;    // 평활화된 회전 사용
        }
        else
        {
            // 새 마커 생성
            markerMap[markerId] = new MarkerData(pos, rot);
        }

        Debug.Log($"📌 마커 {markerId} 감지됨: 위치={pos}, 회전={rot.eulerAngles}");

        MainThreadDispatcher.Enqueue(() =>
        {
            float distance = Vector3.Distance(Camera.main.transform.position, pos);
            Debug.Log($"📏 카메라와 마커 {markerId} 사이 거리: {distance}m");
        });
    }

    private void RecalculateCameraMatrix(float scale)
    {
        double fx = camMatrix.get(0, 0)[0] * scale;
        double fy = camMatrix.get(1, 1)[0] * scale;
        double cx = camMatrix.get(0, 2)[0] * scale;
        double cy = camMatrix.get(1, 2)[0] * scale;

        scaledCamMatrix.put(0, 0, fx); scaledCamMatrix.put(0, 1, 0); scaledCamMatrix.put(0, 2, cx);
        scaledCamMatrix.put(1, 0, 0); scaledCamMatrix.put(1, 1, fy); scaledCamMatrix.put(1, 2, cy);
        scaledCamMatrix.put(2, 0, 0); scaledCamMatrix.put(2, 1, 0); scaledCamMatrix.put(2, 2, 1);

        scaledDistCoeffs.fromArray(distCoeffs.toArray());
    }

    private void ClearAndDisposePrevious()
    {
        if (ids != null) ids.Dispose();
        ids = new Mat();

        foreach (var c in corners) c?.Dispose();
        corners.Clear();

        foreach (var r in rejectedCorners) r?.Dispose();
        rejectedCorners.Clear();
    }

    private MatOfPoint3f CreateObjPoints()
    {
        return new MatOfPoint3f(
            new Point3(-markerLength / 2, markerLength / 2, 0),
            new Point3(markerLength / 2, markerLength / 2, 0),
            new Point3(markerLength / 2, -markerLength / 2, 0),
            new Point3(-markerLength / 2, -markerLength / 2, 0)
        );
    }

    private Matrix4x4 GetTransformMatrix(Mat rvec, Mat tvec)
    {
        Mat rotMat = new Mat();
        Calib3d.Rodrigues(rvec, rotMat);

        // 변환 행렬 생성
        Matrix4x4 m = Matrix4x4.identity;
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                m[row, col] = (float)rotMat.get(row, col)[0];

        // 위치 벡터 넣기
        m[0, 3] = (float)tvec.get(0, 0)[0];
        m[1, 3] = (float)tvec.get(1, 0)[0];
        m[2, 3] = (float)tvec.get(2, 0)[0];

        rotMat.Dispose();

        // 🧩 좌표계 변환: Y, Z 축 반전
        Matrix4x4 convert = Matrix4x4.Scale(new Vector3(1, -1, -1));
        return convert * m;
    }


    private void OnDestroy()
    {
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
        if (camHelper != null)
            camHelper.frameMatAcquired -= OnFrameMatAcquired;
#endif
        ClearAndDisposePrevious();
        detector?.Dispose();
        camMatrix?.Dispose();
        distCoeffs?.Dispose();
        scaledCamMatrix?.Dispose();
        scaledDistCoeffs?.Dispose();
        camHelper?.Dispose();
    }
}