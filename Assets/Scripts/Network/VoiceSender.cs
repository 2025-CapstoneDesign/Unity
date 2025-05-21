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
        if (clip == null || !VoiceWebSocketClient.Instance.IsConnected() || !Microphone.IsRecording(micDevice))
        {
            Debug.LogWarning("🚫 전송 불가: 클립 없음 or 연결 끊김 or 마이크 녹음 중지");
            yield break;
        }

        // 현재 마이크 위치 가져오기
        int currentPosition = Microphone.GetPosition(micDevice);
        
        // 마이크 위치가 유효하지 않은 경우
        if (currentPosition < 0)
        {
            Debug.LogWarning("🚫 유효하지 않은 마이크 위치");
            lastReadPosition = 0;
            yield break;
        }

        // 새 데이터가 있는 경우에만 처리
        if (currentPosition != lastReadPosition)
        {
            int sampleCount;
            float[] samples;
            
            try {
                // 링 버퍼에서 위치 계산
                if (currentPosition < lastReadPosition)
                {
                    // 안전 방법: 전체 클립 데이터를 가져온 후 필요한 부분만 추출
                    float[] allSamples = new float[clip.samples * clip.channels];
                    if (!clip.GetData(allSamples, 0)) {
                        Debug.LogError("GetData 실패 - 전체 데이터");
                        yield break;
                    }
                    
                    // 필요한 샘플 수 계산
                    sampleCount = (clip.samples - lastReadPosition) + currentPosition;
                    samples = new float[sampleCount * clip.channels];
                    
                    // 첫 부분 복사 (lastReadPosition부터 끝까지)
                    int firstPartLength = (clip.samples - lastReadPosition) * clip.channels;
                    System.Array.Copy(allSamples, lastReadPosition * clip.channels, 
                                     samples, 0, firstPartLength);
                    
                    // 두번째 부분 복사 (시작부터 currentPosition까지)
                    System.Array.Copy(allSamples, 0, 
                                     samples, firstPartLength, 
                                     currentPosition * clip.channels);
                }
                else
                {
                    // 일반적인 경우 - 새 데이터만 가져오기
                    sampleCount = currentPosition - lastReadPosition;
                    samples = new float[sampleCount * clip.channels];
                    
                    // 전체 버퍼를 가져온 후 필요한 부분만 추출
                    float[] allSamples = new float[clip.samples * clip.channels];
                    if (!clip.GetData(allSamples, 0)) {
                        Debug.LogError("GetData 실패 - 전체 데이터");
                        yield break;
                    }
                    
                    System.Array.Copy(allSamples, lastReadPosition * clip.channels,
                                     samples, 0, sampleCount * clip.channels);
                }
                
                // 데이터가 유효한지 검사
                if (samples.Length > 0)
                {
                    byte[] bytes = FloatArrayToPCM(samples);

                    // ✅ 1. 현재 단계 태그 먼저 전송
                    SendVoiceTag(CurrentStageTag);

                    // ✅ 2. 음성 데이터 전송
                    VoiceWebSocketClient.Instance.SendBytes(bytes);

                    Debug.Log($"📤 음성과 태그 전송 완료: {CurrentStageTag}, 샘플 수: {sampleCount}");
                }
                else
                {
                    Debug.LogWarning("유효한 샘플 데이터가 없습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"오디오 데이터 처리 오류: {e.Message}\n{e.StackTrace}");
            }
            
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