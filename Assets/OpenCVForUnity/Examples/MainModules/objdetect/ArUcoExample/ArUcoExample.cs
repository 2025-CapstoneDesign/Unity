using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenCVForUnityExample
{
    [RequireComponent(typeof(MultiSource2MatHelper))]
    public class ArUcoExample : MonoBehaviour
    {
        /// 마커 종류.
        public MarkerType markerType = MarkerType.CanonicalMarker;

        // 화면에서 보이는 UI 요소들 (Dropdown, Toggle 등)을 제거
        // public Dropdown markerTypeDropdown;
        // public Dropdown dictionaryIdDropdown;
        // public Toggle useStoredCameraParametersToggle;
        // public Toggle showRejectedCornersToggle;
        // public Toggle refineMarkerDetectionToggle;
        // public Toggle enableLowPassFilterToggle;

        /// 마커 길이 - 인스펙터에서 조정 가능
        public float markerLength = 0.1f;

        /// ARHelper
        public ARHelper arHelper;

        /// 텍스처.
        Texture2D texture;

        /// 다중 소스에서 매트릭스를 가져오는 헬퍼.
        MultiSource2MatHelper multiSource2MatHelper;

        /// RGB 매트.
        Mat rgbMat;

        /// 왜곡이 보정된 RGB 매트.
        Mat undistortedRgbMat;

        /// 카메라 매트릭스.
        Mat camMatrix;

        /// 왜곡 계수.
        MatOfDouble distCoeffs;

        /// AR을 위한 변환 매트릭스.
        Matrix4x4 ARM;

        /// FPS 모니터.
        FpsMonitor fpsMonitor;

        // CanonicalMarker용.
        Mat ids;
        List<Mat> corners;
        List<Mat> rejectedCorners;
        Mat rotMat;
        Dictionary dictionary;
        Mat recoveredIdxs;
        ArucoDetector arucoDetector;

        // Use this for initialization
        void Start()
        {
            // True일 경우, 네이티브 OpenCV의 에러 로그가 Unity 에디터 콘솔에 표시됩니다.
            Utils.setDebugMode(true);

            fpsMonitor = GetComponent<FpsMonitor>();

            multiSource2MatHelper = gameObject.GetComponent<MultiSource2MatHelper>();
            multiSource2MatHelper.outputColorFormat = Source2MatHelperColorFormat.RGBA;
            multiSource2MatHelper.Initialize();
        }

        public void OnSourceToMatHelperInitialized()
        {
            Debug.Log("OnSourceToMatHelperInitialized");

            Mat rgbaMat = multiSource2MatHelper.GetMat();

            texture = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(rgbaMat, texture);

            // Set the Texture2D as the main texture of the Renderer component attached to the game object
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            // Adjust the scale of the game object to match the dimensions of the texture
            gameObject.transform.localScale = new Vector3(rgbaMat.cols(), rgbaMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            // Adjust the orthographic size of the main Camera to fit the aspect ratio of the image
            float width = rgbaMat.width();
            float height = rgbaMat.height();
            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", rgbaMat.width().ToString());
                fpsMonitor.Add("height", rgbaMat.height().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }

            // 카메라 파라미터 설정.
            double fx;
            double fy;
            double cx;
            double cy;

            string loadDirectoryPath = Path.Combine(Application.persistentDataPath, "ArUcoCameraCalibrationExample");
            string calibratonDirectoryName = "camera_parameters" + width + "x" + height;
            string loadCalibratonFileDirectoryPath = Path.Combine(loadDirectoryPath, calibratonDirectoryName);
            string loadPath = Path.Combine(loadCalibratonFileDirectoryPath, calibratonDirectoryName + ".xml");
            if (File.Exists(loadPath))
            {
                CameraParameters param;
                XmlSerializer serializer = new XmlSerializer(typeof(CameraParameters));
                using (var stream = new FileStream(loadPath, FileMode.Open))
                {
                    param = (CameraParameters)serializer.Deserialize(stream);
                }

                camMatrix = param.GetCameraMatrix();
                distCoeffs = new MatOfDouble(param.GetDistortionCoefficients());

                fx = param.camera_matrix[0];
                fy = param.camera_matrix[4];
                cx = param.camera_matrix[2];
                cy = param.camera_matrix[5];

                Debug.Log("Loaded CameraParameters from a stored XML file.");
                Debug.Log("loadPath: " + loadPath);
            }
            else
            {
                int max_d = (int)Mathf.Max(width, height);
                fx = max_d;
                fy = max_d;
                cx = width / 2.0f;
                cy = height / 2.0f;

                camMatrix = new Mat(3, 3, CvType.CV_64FC1);
                camMatrix.put(0, 0, fx);
                camMatrix.put(0, 1, 0);
                camMatrix.put(0, 2, cx);
                camMatrix.put(1, 0, 0);
                camMatrix.put(1, 1, fy);
                camMatrix.put(1, 2, cy);
                camMatrix.put(2, 0, 0);
                camMatrix.put(2, 1, 0);
                camMatrix.put(2, 2, 1.0f);

                distCoeffs = new MatOfDouble(0, 0, 0, 0);

                Debug.Log("Created a dummy CameraParameters.");
            }

            rgbMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
            undistortedRgbMat = new Mat();
            ids = new Mat();
            corners = new List<Mat>();
            rejectedCorners = new List<Mat>();
            rotMat = new Mat(3, 3, CvType.CV_64FC1);
            dictionary = Objdetect.getPredefinedDictionary((int)ArUcoDictionary.DICT_6X6_250);
            recoveredIdxs = new Mat();

            DetectorParameters detectorParams = new DetectorParameters();
            detectorParams.set_minDistanceToBorder(3);
            detectorParams.set_useAruco3Detection(true);
            detectorParams.set_cornerRefinementMethod(Objdetect.CORNER_REFINE_SUBPIX);
            arucoDetector = new ArucoDetector(dictionary, detectorParams);

            arHelper.SetCamMatrix(camMatrix);
            arHelper.SetDistCoeffs(distCoeffs);
            arHelper.Initialize(Screen.width, Screen.height, rgbMat.width(), rgbMat.height());
        }

        // Update is called once per frame
        void Update()
        {
            if (multiSource2MatHelper.IsPlaying() && multiSource2MatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = multiSource2MatHelper.GetMat();

                Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);

                switch (markerType)
                {
                    case MarkerType.CanonicalMarker:
                        Calib3d.undistort(rgbMat, undistortedRgbMat, camMatrix, distCoeffs);
                        arucoDetector.detectMarkers(undistortedRgbMat, corners, ids, rejectedCorners);

                        if (corners.Count == ids.total() || ids.total() == 0)
                            Objdetect.drawDetectedMarkers(undistortedRgbMat, corners, ids, new Scalar(0, 255, 0));

                        if (ids.total() > 0)
                            EstimatePoseCanonicalMarker(undistortedRgbMat);
                        break;
                }

                Imgproc.cvtColor(undistortedRgbMat, rgbaMat, Imgproc.COLOR_RGB2RGBA);

                Utils.matToTexture2D(rgbaMat, texture);
            }
        }

        private void EstimatePoseCanonicalMarker(Mat rgbMat)
        {
            using (MatOfPoint3f objectPoints = new MatOfPoint3f(
                new Point3(-markerLength / 2f, markerLength / 2f, 0),
                new Point3(markerLength / 2f, markerLength / 2f, 0),
                new Point3(markerLength / 2f, -markerLength / 2f, 0),
                new Point3(-markerLength / 2f, -markerLength / 2f, 0)
                ))
            {
                for (int i = 0; i < corners.Count; i++)
                {
                    using (Mat rvec = new Mat(1, 1, CvType.CV_64FC3))
                    using (Mat tvec = new Mat(1, 1, CvType.CV_64FC3))
                    using (Mat corner_4x1 = corners[i].reshape(2, 4)) // 1*4*CV_32FC2 => 4*1*CV_32FC2
                    using (MatOfPoint2f imagePoints = new MatOfPoint2f(corner_4x1))
                    {
                        // 각 마커에 대한 자세(회전, 이동 벡터) 계산
                        Calib3d.solvePnP(objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);

                        // 콘솔에 로그 출력 (회전 벡터와 이동 벡터)
                        // rvec, tvec는 마커가 아닌 "카메라 좌표계"에서 마커가 어디에 있는지 나타내는 값입니다.
                        Debug.Log($"[Marker {i}] rvec (rotation): {rvec.dump()}");
                        Debug.Log($"[Marker {i}] tvec (translation): {tvec.dump()}");

                        // 마커 좌표(코너)도 확인하고 싶다면 corner_4x1.dump()로 출력 가능
                        Debug.Log($"[Marker {i}] corners: {corner_4x1.dump()}");

                        // AR 표시(마커 축) 그리기
                        Calib3d.drawFrameAxes(rgbMat, camMatrix, distCoeffs, rvec, tvec, markerLength * 0.5f);
                    }
                }
            }
        }

        void OnDestroy()
        {
            multiSource2MatHelper.Dispose();
            Utils.setDebugMode(false);
        }

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("OpenCVForUnityExample");
        }

        public void OnPlayButtonClick()
        {
            multiSource2MatHelper.Play();
        }

        public void OnPauseButtonClick()
        {
            multiSource2MatHelper.Pause();
        }

        public void OnStopButtonClick()
        {
            multiSource2MatHelper.Stop();
        }

        public void OnChangeCameraButtonClick()
        {
            multiSource2MatHelper.requestedIsFrontFacing = !multiSource2MatHelper.requestedIsFrontFacing;
        }

        public enum MarkerType
        {
            CanonicalMarker
        }

        public enum ArUcoDictionary
        {
            DICT_6X6_250 = Objdetect.DICT_6X6_250
        }
    }
}
