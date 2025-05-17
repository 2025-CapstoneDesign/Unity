using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    private PythonCommunicator pythonCommunicator;
    private bool isCameraInitialized = false;

    void Start()
    {
        pythonCommunicator = FindObjectOfType<PythonCommunicator>();

        // ✅ Play 모드에서도 카메라 권한 요청
        StartCoroutine(RequestCameraPermission());
    }

    IEnumerator RequestCameraPermission()
    {
        Debug.Log("🔒 Requesting camera permission...");

        // Play 모드에서도 권한 요청
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("✅ Camera permission granted!");
            StartCoroutine(InitializeCamera());
        }
        else
        {
            Debug.LogError("❌ Camera permission denied! Enable it in system settings.");
        }
    }

    IEnumerator InitializeCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log($"🔍 Checking available cameras. Found: {devices.Length}");

        if (devices.Length > 0)
        {
            Debug.Log($"📸 Using camera: {devices[0].name}");
            // 640x480에서 1280x720으로 해상도 변경
            webCamTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);
            webCamTexture.Play();

            // ✅ 카메라가 실제로 실행될 때까지 기다림 (최대 5초)
            float timeout = 5.0f;
            while (!webCamTexture.isPlaying && timeout > 0)
            {
                Debug.Log($"⏳ Waiting for camera to start... {timeout} seconds remaining");
                yield return new WaitForSeconds(1.0f);
                timeout -= 1.0f;
            }

            if (webCamTexture.isPlaying)
            {
                Debug.Log("✅ Camera started successfully!");
                isCameraInitialized = true;
                StartCoroutine(SendFramesToPython());
            }
            else
            {
                Debug.LogError("❌ Camera failed to start! Retrying...");
                yield return new WaitForSeconds(2.0f);
                StartCoroutine(InitializeCamera()); // 다시 시도
            }
        }
        else
        {
            Debug.LogError("❌ No camera found. Retrying...");
            yield return new WaitForSeconds(2.0f);
            StartCoroutine(InitializeCamera()); // 다시 시도
        }

        yield return null;
    }

    IEnumerator SendFramesToPython()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);  // 1초마다 실행

            if (!isCameraInitialized || webCamTexture == null || !webCamTexture.isPlaying)
            {
                Debug.LogError("❌ Camera not playing. Retrying initialization...");
                StartCoroutine(InitializeCamera());
                yield break;  // 현재 루프 종료 후 다시 실행
            }

            Debug.Log("📡 Capturing frame...");

            Texture2D frame = new Texture2D(webCamTexture.width, webCamTexture.height);
            frame.SetPixels(webCamTexture.GetPixels());
            frame.Apply();

            byte[] imageBytes = frame.EncodeToJPG(50); // 50% 압축하여 전송
            Debug.Log($"📤 Sending {imageBytes.Length} bytes to Python...");

            pythonCommunicator.SendFrameToPython(imageBytes);
        }
    }
}
