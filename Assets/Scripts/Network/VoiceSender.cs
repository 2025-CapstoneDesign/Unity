using System.Collections;
using UnityEngine;

public class VoiceSender : MonoBehaviour
{
    public static VoiceSender Instance;

    public float recordDuration; // 1초에서 0.1초로 변경
    private string micDevice;
    private AudioClip clip;
    private int lastReadPosition = 0;

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
        // ✅ 강제로 캡처 실행 (테스트용)
        isCapturing = false; // ★ 여기를 false로 해서 아래 코드가 실행되게 만듦

        if (isCapturing) return;

        foreach (var device in Microphone.devices)
        {
            Debug.Log("🎤 마이크 디바이스: " + device);
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("❌ 마이크 디바이스 없음 - 테스트용 더미 마이크 사용");
            micDevice = null; // 마이크 없을 때도 진행 (주의!)
        }
        else
        {
            micDevice = Microphone.devices[0];
        }

        Debug.Log("🎤 음성 캡처 시작");
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
        if (clip == null || !VoiceWebSocketClient.Instance.IsConnected())
        {
            Debug.LogWarning("🚫 전송 불가: 클립 없음 or 연결 끊김");
            yield break;
        }

        // 현재 마이크 위치 가져오기
        int currentPosition = Microphone.GetPosition(micDevice);
        
        // 새 데이터가 있는 경우에만 처리
        if (currentPosition != lastReadPosition)
        {
            int sampleCount;
            float[] samples;
            
            // 링 버퍼에서 위치 계산
            if (currentPosition < lastReadPosition)
            {
                // 버퍼 끝에서 처음으로 순환된 경우
                sampleCount = (clip.samples - lastReadPosition) + currentPosition;
                samples = new float[sampleCount * clip.channels];
                
                // 두 부분으로 나누어 데이터 가져오기
                float[] firstPart = new float[(clip.samples - lastReadPosition) * clip.channels];
                float[] secondPart = new float[currentPosition * clip.channels];
                
                clip.GetData(firstPart, lastReadPosition);
                clip.GetData(secondPart, 0);
                
                // 두 부분 합치기
                System.Array.Copy(firstPart, 0, samples, 0, firstPart.Length);
                System.Array.Copy(secondPart, 0, samples, firstPart.Length, secondPart.Length);
            }
            else
            {
                // 일반적인 경우 - 새 데이터만 가져오기
                sampleCount = currentPosition - lastReadPosition;
                samples = new float[sampleCount * clip.channels];
                clip.GetData(samples, lastReadPosition);
            }
            
            byte[] bytes = FloatArrayToPCM(samples);

            // ✅ 1. 현재 단계 태그 먼저 전송
            SendVoiceTag(CurrentStageTag);

            // ✅ 2. 음성 데이터 전송
            VoiceWebSocketClient.Instance.SendBytes(bytes);

            Debug.Log($"📤 음성과 태그 전송 완료: {CurrentStageTag}, 샘플 수: {sampleCount}");
            
            // 마지막으로 읽은 위치 업데이트
            lastReadPosition = currentPosition;
        }
        else
        {
            Debug.Log("새로운 오디오 데이터가 없습니다.");
        }

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
        if (VoiceWebSocketClient.Instance != null && VoiceWebSocketClient.Instance.IsConnected())
        {
            string json = $"{{\"type\": \"voice_tag\", \"value\": \"{tag}\"}}";
            VoiceWebSocketClient.Instance.SendText(json);
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