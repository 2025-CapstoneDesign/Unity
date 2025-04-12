using System.Collections;
using UnityEngine;

public class VoiceSender : MonoBehaviour
{
    public static VoiceSender Instance;

    public int recordDuration = 2;
    private string micDevice;
    private AudioClip clip;

    private Coroutine sendCoroutine;
    private bool isCapturing = false;

    // ✅ 현재 단계 태그 (외부에서 설정해줘야 함)
    public string CurrentStageTag { get; set; } = "UNKNOWN";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCapture();
    }

    public void StartCapture()
    {
        if (isCapturing) return;

        foreach (var device in Microphone.devices)
        {
            Debug.Log("🎤 마이크 디바이스: " + device);
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("❌ 마이크 디바이스 없음");
        }

        Debug.Log("🎤 음성 캡처 시작");
        micDevice = Microphone.devices[0];
        clip = Microphone.Start(micDevice, true, 1, 16000);
        sendCoroutine = StartCoroutine(SendLoop());
        isCapturing = true;
    }

    public void StopCapture()
    {
        if (!isCapturing) return;

        Debug.Log("🛑 음성 캡처 중지");
        Microphone.End(micDevice);
        if (sendCoroutine != null) StopCoroutine(sendCoroutine);
        isCapturing = false;
    }

    IEnumerator SendLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(recordDuration);
            yield return StartCoroutine(SendAudio());
        }
    }

    IEnumerator SendAudio()
    {
        if (clip == null || !WebSocketClient.Instance.IsConnected())
        {
            Debug.LogWarning("🚫 전송 불가: 클립 없음 or 연결 끊김");
            yield break;
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        byte[] bytes = FloatArrayToPCM(samples);

        // ✅ 1. 현재 단계 태그 먼저 전송
        SendVoiceTag(CurrentStageTag);

        // ✅ 2. 음성 데이터 전송
        WebSocketClient.Instance.SendBytes(bytes);

        Debug.Log($"📤 음성과 태그 전송 완료: {CurrentStageTag}");

        yield return null;
    }

    byte[] FloatArrayToPCM(float[] samples)
    {
        byte[] data = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short s = (short)(samples[i] * short.MaxValue);
            byte[] b = System.BitConverter.GetBytes(s);
            data[i * 2] = b[0];
            data[i * 2 + 1] = b[1];
        }
        return data;
    }

    public void SendVoiceTag(string tag)
    {
        if (WebSocketClient.Instance != null && WebSocketClient.Instance.IsConnected())
        {
            string json = $"{{\"type\": \"voice_tag\", \"value\": \"{tag}\"}}";
            WebSocketClient.Instance.SendText(json);
            Debug.Log($"📤 보이스 태그 전송: {tag}");
        }
    }

    void OnDestroy()
    {
        StopCapture();
    }

    void OnDisable()
    {
        StopCapture();
    }
}
