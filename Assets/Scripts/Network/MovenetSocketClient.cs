using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class PoseRecognitionResult
{
    public string type;             // 메시지 종류 (현재는 "pose_result")
    public bool cpr;                // 성인/유아 CPR 자세 적합 여부
    public bool infant_airway;      // 기도 확보 자세 적합 여부
    public bool infant_compression; // 유아 흉부 압박 적합 여부
    public bool vacuum_pump;        // 흡인기 사용 자세 적합 여부
}

public class MovenetSocketClient : MonoBehaviour
{
    public static MovenetSocketClient Instance;

    public Action<PoseRecognitionResult> OnPoseResultReceived;

    private WebSocket websocket;
    private bool isConnecting = false;
    private bool isManuallyClosed = false;
    private float reconnectInterval = 5f;
    private float reconnectTimer = 0f;

    [Header("WebSocket 설정")]
    [Tooltip("ws:// 형식의 WebSocket 주소를 입력하세요")]
    public string serverUrl = "ws://localhost:10051"; // ✅ 인스펙터에서 수정 가능

    // 가장 최근에 받은 포즈 인식 결과를 저장
    private PoseRecognitionResult latestPoseResult;

    void Awake() => Instance = this;

    async void Start()
    {
        Debug.Log("START MovenetSocketClient");
        await Connect();
    }

    async Task Connect()
    {
        if (isConnecting || websocket?.State == WebSocketState.Open) return;

        isConnecting = true;
        isManuallyClosed = false;

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ MovenetWebSocket 연결됨");
            isConnecting = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            try
            {
                PoseRecognitionResult result = JsonConvert.DeserializeObject<PoseRecognitionResult>(msg);
                if (result != null && result.type == "pose_result")
                {
                    latestPoseResult = result;
                    OnPoseResultReceived?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("❌ MovenetWebSocket 메시지 파싱 오류: " + ex.Message);
            }
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("❌ MovenetWebSocket 에러: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.LogWarning("🔌 MovenetWebSocket 끊김, 재연결 대기 중...");
            if (!isManuallyClosed)
            {
                isConnecting = false;
                reconnectTimer = reconnectInterval;
            }
        };

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("⚠️ MovenetWebSocket 연결 실패: " + ex.Message);
            isConnecting = false;
            reconnectTimer = reconnectInterval;
        }
    }

    void Update()
    {
        websocket?.DispatchMessageQueue();

        if (!isConnecting && (websocket == null || websocket.State != WebSocketState.Open))
        {
            reconnectTimer -= Time.deltaTime;
            if (reconnectTimer <= 0f)
            {
                reconnectTimer = reconnectInterval;
                _ = Connect(); // 재연결 시도
            }
        }
    }

    public async void SendBytes(byte[] data)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Send(data);
        }
        else
        {
            Debug.LogWarning("📤 MovenetWebSocket 연결되지 않음, 전송 실패");
        }
    }

    public async void SendText(string msg)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(msg);
        }
        else
        {
            Debug.LogWarning("📤 MovenetWebSocket 연결되지 않음, 전송 실패");
        }
    }

    private void OnApplicationQuit()
    {
        isManuallyClosed = true;
        websocket?.Close();
    }

    public bool IsConnected()
    {
        return websocket != null && websocket.State == WebSocketState.Open;
    }

    // Manager 클래스들을 위한 포즈 인식 결과 접근 메소드들
    public bool IsCPRPoseCorrect()
    {
        return latestPoseResult != null && latestPoseResult.cpr;
    }

    public bool IsInfantAirwayPoseCorrect()
    {
        return latestPoseResult != null && latestPoseResult.infant_airway;
    }

    public bool IsInfantCompressionPoseCorrect()
    {
        return latestPoseResult != null && latestPoseResult.infant_compression;
    }

    public bool IsVacuumPumpPoseCorrect()
    {
        return latestPoseResult != null && latestPoseResult.vacuum_pump;
    }

    // 모든 포즈 인식 결과를 한꺼번에 가져오기
    public PoseRecognitionResult GetLatestPoseResult()
    {
        return latestPoseResult;
    }
}
