using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;

public class OptimizedArucoFollower : MonoBehaviour
{
    public GameObject markerPrefab;
    private GameObject markerInstance;

    private MultiSource2MatHelper camHelper;
    private Mat rgbMat, undistortedMat, ids, rotMat;
    private List<Mat> corners;
    private Dictionary dictionary;
    private ArucoDetector detector;
    private Mat camMatrix;
    private MatOfDouble distCoeffs;

    public float markerLength = 0.1f;

    void Start()
    {
        camHelper = gameObject.AddComponent<MultiSource2MatHelper>();
        camHelper.outputColorFormat = Source2MatHelperColorFormat.RGBA;
        camHelper.Initialize();
    }

    public void OnSourceToMatHelperInitialized()
    {
        Mat rgbaMat = camHelper.GetMat();
        rgbMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
        undistortedMat = new Mat();
        ids = new Mat();
        corners = new List<Mat>();
        rotMat = new Mat();

        dictionary = Objdetect.getPredefinedDictionary(Objdetect.DICT_6X6_250);
        DetectorParameters detectorParams = new DetectorParameters();
        detectorParams.set_minDistanceToBorder(3);
        detectorParams.set_useAruco3Detection(true);
        detectorParams.set_cornerRefinementMethod(Objdetect.CORNER_REFINE_SUBPIX);
        detectorParams.set_minSideLengthCanonicalImg(16);
        detectorParams.set_errorCorrectionRate(0.8);

        detector = new ArucoDetector(dictionary, detectorParams);

        float width = rgbaMat.width();
        float height = rgbaMat.height();
        int max_d = (int)Mathf.Max(width, height);
        camMatrix = new Mat(3, 3, CvType.CV_64FC1);
        camMatrix.put(0, 0, max_d); camMatrix.put(0, 1, 0); camMatrix.put(0, 2, width / 2);
        camMatrix.put(1, 0, 0);      camMatrix.put(1, 1, max_d); camMatrix.put(1, 2, height / 2);
        camMatrix.put(2, 0, 0);      camMatrix.put(2, 1, 0);      camMatrix.put(2, 2, 1);

        distCoeffs = new MatOfDouble(0, 0, 0, 0, 0);
    }

    void Update()
    {
        if (camHelper.IsPlaying() && camHelper.DidUpdateThisFrame())
        {
            Mat rgbaMat = camHelper.GetMat();
            Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);

            Calib3d.undistort(rgbMat, undistortedMat, camMatrix, distCoeffs);

            corners = new List<Mat>();
            ids = new Mat();

            detector.detectMarkers(undistortedMat, corners, ids);

            if (ids.total() > 0)
            {
                Debug.Log($"Detected Marker IDs: {ids.dump()}");
                EstimatePoseAndPlace(corners[0]);
            }
        }
    }

    private void EstimatePoseAndPlace(Mat markerCorners)
    {
        using (Mat rvec = new Mat())
        using (Mat tvec = new Mat())
        using (Mat corner4x1 = markerCorners.reshape(2, 4))
        using (MatOfPoint2f imagePoints = new MatOfPoint2f(corner4x1))
        using (MatOfPoint3f objectPoints = new MatOfPoint3f(
            new Point3(-markerLength / 2, markerLength / 2, 0),
            new Point3(markerLength / 2, markerLength / 2, 0),
            new Point3(markerLength / 2, -markerLength / 2, 0),
            new Point3(-markerLength / 2, -markerLength / 2, 0)))
        {
            Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);
            Calib3d.Rodrigues(rvec, rotMat);

            Vector3 localPos = new Vector3(
                (float)tvec.get(0, 0)[0],
                -(float)tvec.get(1, 0)[0],
                -(float)tvec.get(2, 0)[0]
            );

            Quaternion rot = MatrixToQuaternion(rotMat);

            if (markerInstance == null)
                markerInstance = Instantiate(markerPrefab);

            markerInstance.transform.position = Camera.main.transform.position + Camera.main.transform.rotation * localPos;
            markerInstance.transform.rotation = Camera.main.transform.rotation * rot;

            // Debugging axis
            Calib3d.drawFrameAxes(undistortedMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);
        }
    }

    private Quaternion MatrixToQuaternion(Mat R)
    {
        double m00 = R.get(0, 0)[0], m01 = R.get(0, 1)[0], m02 = R.get(0, 2)[0];
        double m10 = R.get(1, 0)[0], m11 = R.get(1, 1)[0], m12 = R.get(1, 2)[0];
        double m20 = R.get(2, 0)[0], m21 = R.get(2, 1)[0], m22 = R.get(2, 2)[0];

        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt((float)(1.0 + m00 + m11 + m22)) / 2f;
        q.x = (float)((m21 - m12) / (4.0 * q.w));
        q.y = (float)((m02 - m20) / (4.0 * q.w));
        q.z = (float)((m10 - m01) / (4.0 * q.w));
        return q;
    }

    private void OnDestroy()
    {
        camHelper.Dispose();
    }
}
