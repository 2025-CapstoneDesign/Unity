using System;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.ImgprocModule;
using HoloLensCameraStream;
using HoloLensWithOpenCVForUnity.UnityUtils.Helper;

[RequireComponent(typeof(HLCameraStream2MatHelper))]
public class HoloLensCameraCalibration : MonoBehaviour
{
    public int boardWidth = 9;
    public int boardHeight = 6;
    public float squareSize = 0.025f; // 2.5cm per square

    private HLCameraStream2MatHelper hlHelper;
    private Mat camMatrix;
    private MatOfDouble distCoeffs;

    private List<Mat> capturedImages = new List<Mat>();
    private List<Mat> imagePoints = new List<Mat>();

    private Mat latestFrame;
    private bool readyToCalibrate = false;
    private float lastCaptureTime = 0f;
    private float startDelay = 5f;
    private bool hasStartedCapturing = false;

    void Start()
    {
        hlHelper = GetComponent<HLCameraStream2MatHelper>();
        hlHelper.Initialize();
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
        hlHelper.frameMatAcquired += OnFrameMatAcquired;
#endif
        Debug.Log("[CAL] HoloLens Camera Calibration Start");
    }

#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
    private void OnFrameMatAcquired(Mat frame, Matrix4x4 proj, Matrix4x4 camToWorld, CameraIntrinsics intrinsics)
    {
        if (frame == null || frame.empty()) return;
        if (latestFrame != null) latestFrame.Dispose();
        latestFrame = frame.clone();
    }
#endif

    void Update()
    {
        if (Time.time < startDelay)
        {
            return;
        }

        if (!hasStartedCapturing)
        {
            Debug.Log("[CAL] Starting frame capture after delay");
            hasStartedCapturing = true;
        }

        if (Time.time - lastCaptureTime >= 1f && latestFrame != null && imagePoints.Count < 10)
        {
            lastCaptureTime = Time.time;
            TryCaptureFrame(latestFrame);
        }

        if (!readyToCalibrate && imagePoints.Count >= 10)
        {
            readyToCalibrate = true;
            Calibrate();
        }
    }

    private void TryCaptureFrame(Mat frame)
    {
        Mat gray = new Mat();
        Imgproc.cvtColor(frame, gray, Imgproc.COLOR_RGBA2GRAY);

        Size patternSize = new Size(boardWidth, boardHeight);
        MatOfPoint2f corners = new MatOfPoint2f();
        bool found = Calib3d.findChessboardCorners(gray, patternSize, corners);

        if (found)
        {
            Imgproc.cornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1),
                new TermCriteria(TermCriteria.EPS + TermCriteria.COUNT, 30, 0.1));

            imagePoints.Add(corners);
            capturedImages.Add(gray.clone());

            Debug.Log($"[CAL] Frame Captured: {imagePoints.Count}/10");
        }
        else
        {
            Debug.Log("[CAL] ❌ Chessboard not found in frame");
        }

        gray.Dispose();
    }

    void Calibrate()
    {
        Debug.Log("[CAL] ==== Starting Calibration ====");

        Size patternSize = new Size(boardWidth, boardHeight);
        List<Mat> objectPoints = new List<Mat>();
        MatOfPoint3f obj = new MatOfPoint3f();

        for (int i = 0; i < boardHeight; i++)
            for (int j = 0; j < boardWidth; j++)
                obj.push_back(new MatOfPoint3f(new Point3(j * squareSize, i * squareSize, 0f)));

        for (int i = 0; i < imagePoints.Count; i++)
            objectPoints.Add(obj);

        camMatrix = new Mat(3, 3, CvType.CV_64FC1);
        distCoeffs = new MatOfDouble();
        List<Mat> rvecs = new List<Mat>();
        List<Mat> tvecs = new List<Mat>();

        double error = Calib3d.calibrateCamera(objectPoints, imagePoints,
            capturedImages[0].size(), camMatrix, distCoeffs, rvecs, tvecs);

        Debug.Log("[CAL] ==== Calibration Done ====");
        Debug.Log("[CAL] Reprojection Error: " + error);
        Debug.Log("[CAL] camMatrix:\n" + camMatrix.dump());
        Debug.Log("[CAL] distCoeffs:\n" + distCoeffs.dump());
    }

    void OnDestroy()
    {
#if WINDOWS_UWP && !DISABLE_HOLOLENSCAMSTREAM_API
        hlHelper.frameMatAcquired -= OnFrameMatAcquired;
#endif
        if (latestFrame != null) latestFrame.Dispose();
    }
}
