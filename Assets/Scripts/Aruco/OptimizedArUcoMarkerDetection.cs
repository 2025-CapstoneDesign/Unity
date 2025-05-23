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

    private void InitCameraParameters()
    {
        // HoloLens 2의 실제 카메라 파라미터 적용
        camMatrix = new Mat(3, 3, CvType.CV_64FC1);
        
        // HoloLens 2 PV 카메라의 실제 내부 파라미터 (1280x720 해상도 기준)
        double[] cameraMatrix = new double[] {
            1037.3, 0.0, 637.4,     // fx, 0, cx
            0.0, 1037.9, 359.5,     // 0, fy, cy
            0.0, 0.0, 1.0           // 0, 0, 1
        };
        
        camMatrix.put(0, 0, cameraMatrix);

        // HoloLens 2 PV 카메라의 실제 왜곡 계수 (double 타입 사용)
        double[] distCoeffsArray = new double[] { 0.2709, -0.9735, 0.0021, -0.0015, 0.8155 };
        distCoeffs = new MatOfDouble();
        distCoeffs.fromArray(distCoeffsArray);

        scaledCamMatrix = new Mat(3, 3, CvType.CV_64FC1);
        scaledDistCoeffs = new MatOfDouble();
        scaledDistCoeffs.fromArray((double[])distCoeffsArray.Clone());
    }

    private void InitArucoDetector()
    {
        var dictionary = Objdetect.getPredefinedDictionary(Objdetect.DICT_6X6_250);
        DetectorParameters detectorParams = new DetectorParameters();
        
        // 마커 검출을 위한 기본 파라미터 최적화
        detectorParams.set_adaptiveThreshWinSizeMin(3);
        detectorParams.set_adaptiveThreshWinSizeMax(23);
        detectorParams.set_adaptiveThreshWinSizeStep(10);
        detectorParams.set_adaptiveThreshConstant(7);
        
        // 마커 검출 정확도 향상을 위한 파라미터
        detectorParams.set_minMarkerPerimeterRate(0.03f);
        detectorParams.set_maxMarkerPerimeterRate(4.0f);
        detectorParams.set_polygonalApproxAccuracyRate(0.02f);
        detectorParams.set_minCornerDistanceRate(0.05f);
        detectorParams.set_minDistanceToBorder(3);
        
        // 코너 검출 개선
        detectorParams.set_cornerRefinementMethod(Objdetect.CORNER_REFINE_SUBPIX);
        detectorParams.set_cornerRefinementWinSize(5);
        detectorParams.set_cornerRefinementMaxIterations(30);
        detectorParams.set_cornerRefinementMinAccuracy(0.1f);
        
        // 원근 변환 파라미터 최적화
        detectorParams.set_perspectiveRemovePixelPerCell(4);
        detectorParams.set_perspectiveRemoveIgnoredMarginPerCell(0.13f);
        
        // ArUco3 개선된 검출 알고리즘 활성화
        detectorParams.set_useAruco3Detection(true);
        detectorParams.set_minSideLengthCanonicalImg(32);
        detectorParams.set_minMarkerLengthRatioOriginalImg(0.0f);
        
        detector = new ArucoDetector(dictionary, detectorParams);
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
        using (Mat gray = new Mat())
        using (Mat blurred = new Mat())
        using (Mat enhanced = new Mat())
        {
            // 그레이스케일 변환
            Imgproc.cvtColor(input, gray, Imgproc.COLOR_RGB2GRAY);
            
            // 양방향 필터로 노이즈 제거하면서 엣지 보존
            double sigmaColor = 50.0;
            double sigmaSpace = 5.0;
            Imgproc.bilateralFilter(gray, blurred, 7, sigmaColor, sigmaSpace);
            
            // CLAHE(Contrast Limited Adaptive Histogram Equalization) 적용
            Imgproc.equalizeHist(blurred, enhanced);
            
            // 적응형 이진화 적용 - 파라미터를 double로 명시
            double maxValue = 255.0;
            double adaptiveThresholdC = 2.0;
            Imgproc.adaptiveThreshold(
                enhanced,
                enhanced,
                maxValue,
                Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C,
                Imgproc.THRESH_BINARY,
                11,
                adaptiveThresholdC
            );
            
            if (enableDownScaling && Math.Abs(scale - 1.0) > 0.00001)
            {
                RecalculateCameraMatrix(scale);
                Calib3d.undistort(enhanced, output, scaledCamMatrix, scaledDistCoeffs);
            }
            else
            {
                Calib3d.undistort(enhanced, output, camMatrix, distCoeffs);
            }
        }
    }

    private void ProcessDetectedMarker(int index, Matrix4x4 camToWorld, float scale)
    {
        using (Mat cornerMat = corners[index].reshape(2, 4))
        using (MatOfPoint2f points = new MatOfPoint2f(cornerMat))
        using (Mat rvec = new Mat())
        using (Mat tvec = new Mat())
        {
            // 이전 프레임의 회전/이동 벡터를 초기 추정치로 사용
            int markerId = (int)ids.get(index, 0)[0];
            Mat initialRvec = null;
            Mat initialTvec = null;

            if (markerMap.ContainsKey(markerId))
            {
                // 이전 프레임의 변환 행렬을 Rodrigues 형식으로 변환
                Matrix4x4 prevTransform = Matrix4x4.TRS(markerMap[markerId].position, markerMap[markerId].rotation, Vector3.one);
                using (Mat prevRotMat = new Mat(3, 3, CvType.CV_64FC1))
                {
                    initialRvec = new Mat();
                    initialTvec = new Mat(3, 1, CvType.CV_64FC1);

                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                            prevRotMat.put(row, col, prevTransform[row, col]);
                        initialTvec.put(row, 0, prevTransform[row, 3]);
                    }

                    Calib3d.Rodrigues(prevRotMat, initialRvec);
                }
            }

            // solvePnP 함수 호출 시 더 정확한 설정 사용
            bool useExtrinsicGuess = initialRvec != null && initialTvec != null;
            Mat currentCamMatrix = enableDownScaling && Math.Abs(scale - 1f) > 0.00001f ? scaledCamMatrix : camMatrix;
            MatOfDouble currentDistCoeffs = enableDownScaling && Math.Abs(scale - 1f) > 0.00001f ? scaledDistCoeffs : distCoeffs;
            
            // 초기 포즈 추정
            Calib3d.solvePnP(CreateObjPoints(), points, currentCamMatrix, currentDistCoeffs, 
                            rvec, tvec, useExtrinsicGuess, Calib3d.SOLVEPNP_IPPE);

            // 결과 정제
            using (Mat objPoints = CreateObjPoints())
            {
                Calib3d.solvePnPRefineLM(objPoints, points, currentCamMatrix, currentDistCoeffs, rvec, tvec);
            }

            initialRvec?.Dispose();
            initialTvec?.Dispose();

            SaveMarkerData(markerId, rvec, tvec, camToWorld);
        }
    }

    private Matrix4x4 GetTransformMatrix(Mat rvec, Mat tvec, int markerId)
    {
        using (Mat rotMat = new Mat())
        {
            Calib3d.Rodrigues(rvec, rotMat);

            // 회전 행렬을 쿼터니언으로 변환
            Quaternion rawQuat = MatrixToQuaternion(rotMat);

            // 이전 프레임의 회전값과 부호 검사
            if (markerMap.TryGetValue(markerId, out MarkerData prevMarker))
            {
                if (Quaternion.Dot(rawQuat, prevMarker.rotation) < 0f)
                {
                    // 부호를 반대로 뒤집어 같은 회전 쪽으로 맞춰준다
                    rawQuat = new Quaternion(-rawQuat.x, -rawQuat.y, -rawQuat.z, -rawQuat.w);
                }
            }

            // Unity 좌표계로 변환
            Matrix4x4 m = Matrix4x4.TRS(
                new Vector3(
                    (float)tvec.get(0, 0)[0],
                    (float)tvec.get(1, 0)[0],
                    (float)tvec.get(2, 0)[0]
                ),
                rawQuat,
                Vector3.one
            );

            // OpenCV에서 Unity 좌표계로 변환
            Matrix4x4 cvToUnity = Matrix4x4.identity;
            cvToUnity[0, 0] = 1;
            cvToUnity[1, 1] = -1;
            cvToUnity[2, 2] = -1;

            return cvToUnity * m;
        }
    }

    private Quaternion MatrixToQuaternion(Mat rotMat)
    {
        // 회전 행렬의 원소 추출
        double m00 = rotMat.get(0, 0)[0];
        double m01 = rotMat.get(0, 1)[0];
        double m02 = rotMat.get(0, 2)[0];
        double m10 = rotMat.get(1, 0)[0];
        double m11 = rotMat.get(1, 1)[0];
        double m12 = rotMat.get(1, 2)[0];
        double m20 = rotMat.get(2, 0)[0];
        double m21 = rotMat.get(2, 1)[0];
        double m22 = rotMat.get(2, 2)[0];

        float tr = (float)(m00 + m11 + m22);
        float qw, qx, qy, qz;

        if (tr > 0)
        {
            float S = Mathf.Sqrt((float)(tr + 1.0)) * 2;
            qw = 0.25f * S;
            qx = (float)(m21 - m12) / S;
            qy = (float)(m02 - m20) / S;
            qz = (float)(m10 - m01) / S;
        }
        else if ((m00 > m11) && (m00 > m22))
        {
            float S = Mathf.Sqrt((float)(1.0 + m00 - m11 - m22)) * 2;
            qw = (float)(m21 - m12) / S;
            qx = 0.25f * S;
            qy = (float)(m01 + m10) / S;
            qz = (float)(m02 + m20) / S;
        }
        else if (m11 > m22)
        {
            float S = Mathf.Sqrt((float)(1.0 + m11 - m00 - m22)) * 2;
            qw = (float)(m02 - m20) / S;
            qx = (float)(m01 + m10) / S;
            qy = 0.25f * S;
            qz = (float)(m12 + m21) / S;
        }
        else
        {
            float S = Mathf.Sqrt((float)(1.0 + m22 - m00 - m11)) * 2;
            qw = (float)(m10 - m01) / S;
            qx = (float)(m02 + m20) / S;
            qy = (float)(m12 + m21) / S;
            qz = 0.25f * S;
        }

        return new Quaternion(qx, qy, qz, qw).normalized;
    }

    private Matrix4x4 StabilizeRotation(Matrix4x4 currentTransform, int markerId)
    {
        if (!markerMap.ContainsKey(markerId))
            return currentTransform;

        MarkerData previousMarker = markerMap[markerId];
        Matrix4x4 previousTransform = Matrix4x4.TRS(previousMarker.position, previousMarker.rotation, Vector3.one);

        // 현재와 이전 방향 벡터 추출
        Vector3 currentUp = currentTransform.GetColumn(1);
        Vector3 currentForward = currentTransform.GetColumn(2);
        Vector3 previousUp = previousTransform.GetColumn(1);
        Vector3 previousForward = previousTransform.GetColumn(2);

        // 방향 벡터 사이의 각도 계산
        float upAngle = Vector3.Angle(currentUp, previousUp);
        float forwardAngle = Vector3.Angle(currentForward, previousForward);

        // 급격한 방향 변화 감지 (160도 이상)
        if (upAngle > 160f || forwardAngle > 160f)
        {
            // 이전 방향을 유지
            return previousTransform;
        }

        // 부드러운 방향 전환 적용
        float smoothFactor = 0.8f;
        Vector3 position = Vector3.Lerp(previousTransform.GetColumn(3), currentTransform.GetColumn(3), smoothFactor);
        Quaternion rotation = Quaternion.Lerp(previousMarker.rotation, Quaternion.LookRotation(currentForward, currentUp), smoothFactor);

        return Matrix4x4.TRS(position, rotation, Vector3.one);
    }

    private void SaveMarkerData(int markerId, Mat rvec, Mat tvec, Matrix4x4 camToWorld)
    {
        Matrix4x4 markerToCamera = GetTransformMatrix(rvec, tvec, markerId);
        Matrix4x4 markerToWorld = camToWorld * markerToCamera;
        
        // 회전 안정화 적용
        markerToWorld = StabilizeRotation(markerToWorld, markerId);

        Vector3 pos = markerToWorld.GetColumn(3);
        Quaternion rot = Quaternion.LookRotation(markerToWorld.GetColumn(2), markerToWorld.GetColumn(1));

        if (markerMap.ContainsKey(markerId))
        {
            MarkerData marker = markerMap[markerId];
            
            // 급격한 변화 감지 및 필터링
            float positionDelta = Vector3.Distance(marker.position, pos);
            float rotationDelta = Quaternion.Angle(marker.rotation, rot);
            
            if (positionDelta > 0.1f || rotationDelta > 30f)
            {
                // 급격한 변화 발생 시 이전 값을 더 많이 반영
                pos = Vector3.Lerp(marker.position, pos, 0.3f);
                rot = Quaternion.Lerp(marker.rotation, rot, 0.3f);
            }
            else
            {
                // 작은 변화는 부드럽게 반영
                pos = Vector3.Lerp(marker.position, pos, 0.8f);
                rot = Quaternion.Lerp(marker.rotation, rot, 0.8f);
            }
            
            marker.position = pos;
            marker.rotation = rot;
        }
        else
        {
            markerMap[markerId] = new MarkerData(pos, rot);
        }
    }

    private void RecalculateCameraMatrix(float scale)
    {
        // 이미지 스케일링에 따른 카메라 매트릭스 정확한 조정
        double fx = camMatrix.get(0, 0)[0] * scale;
        double fy = camMatrix.get(1, 1)[0] * scale;
        double cx = camMatrix.get(0, 2)[0] * scale;
        double cy = camMatrix.get(1, 2)[0] * scale;

        scaledCamMatrix.put(0, 0, new double[] {
            fx, 0, cx,
            0, fy, cy,
            0, 0, 1
        });

        // 왜곡 계수는 스케일링의 영향을 받지 않음
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

    private void ApplyViewAngleStabilization(int markerId, float viewAngle)
    {
        if (!markerMap.ContainsKey(markerId)) return;
        
        MarkerData marker = markerMap[markerId];
        Quaternion rotation = marker.rotation;
        
        // 마커 평면 방향(forward) 계산
        Vector3 markerNormal = rotation * Vector3.forward;
        
        // 마커 평면과 시선 방향 사이의 각도 계산
        float planeViewAngle = Vector3.Angle(markerNormal, -Camera.main.transform.forward);
        
        // 마커가 시선에 거의 수직일 때 (마커가 옆으로 보일 때) 강한 안정화 필요
        if (planeViewAngle > 70f)
        {
            // 마커의 월드 좌표상 높이 안정화
            Vector3 position = marker.position;
            float heightAverage = marker.GetRecentHeightAverage();
            position.y = Mathf.Lerp(position.y, heightAverage, 0.8f);
            marker.position = position;
            
            // 회전 안정화 - 마커가 가파르게 기울어지는 것 방지
            Vector3 eulerAngles = rotation.eulerAngles;
            eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, marker.GetRecentZRotationAverage(), 0.7f);
            marker.rotation = Quaternion.Euler(eulerAngles);
        }
    }
}