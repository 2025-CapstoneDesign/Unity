using System.Collections;
using UnityEngine;

public class VoiceSender : MonoBehaviour
{
    public static VoiceSender Instance;

    public int recordDuration = 10; // 버퍼는 10초로 넉넉하게
    private string micDevice;
    private AudioClip clip;

    private Coroutine sendCoroutine;
    private bool isCapturing = false;
    private int lastSamplePosition = 0;

    void Awake()
    {
        Instance = this;
    }

    public void StartCapture()
    {
        Debug.Log("🎤 StartCapture() 호출됨");

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("❌ 마이크 장치가 없습니다!");
            return;
        }

        micDevice = Microphone.devices[0];
        Debug.Log("🎙️ 사용 중인 마이크: " + micDevice);

        clip = Microphone.Start(micDevice, true, recordDuration, 16000);
        lastSamplePosition = 0;

        if (clip == null)
        {
            Debug.LogError("❌ Microphone.Start() 실패 - AudioClip이 null입니다.");
        }
        else
        {
            Debug.Log("✅ 마이크 녹음 시작됨!");
        }

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
            yield return new WaitForSeconds(1f); // ✅ 1초마다 실행
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

        int currentPosition = Microphone.GetPosition(micDevice);
        int sampleCount = currentPosition - lastSamplePosition;

        if (sampleCount < 0) // 루프되었을 경우
        {
            sampleCount = clip.samples - lastSamplePosition + currentPosition;
        }

        if (sampleCount == 0)
        {
            Debug.Log("⏳ 새로 녹음된 샘플 없음, 전송 생략");
            yield break;
        }

        float[] samples = new float[sampleCount * clip.channels];
        clip.GetData(samples, lastSamplePosition);

        Debug.Log($"📤 전송 샘플 수: {samples.Length}, 시간: {(float)samples.Length / clip.channels / 16000f:0.00}초");

        byte[] bytes = FloatArrayToPCM(samples);
        WebSocketClient.Instance.SendBytes(bytes);

        lastSamplePosition = currentPosition;
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
}
