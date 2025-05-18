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
    
    // 각 포즈 타입별 에러 메시지
    public string cpr_errors;
    public string infant_airway_errors;
    public string infant_compression_errors;
    public string vacuum_pump_errors;
    
    // 이전 버전과의 호환성을 위한 속성
    [JsonIgnore]
    public string cpr_error_msg => cpr_errors ?? string.Empty;
    
    [JsonIgnore]
    public string infant_airway_error_msg => infant_airway_errors ?? string.Empty;
    
    [JsonIgnore]
    public string infant_compression_error_msg => infant_compression_errors ?? string.Empty;
    
    [JsonIgnore]
    public string vacuum_pump_error_msg => vacuum_pump_errors ?? string.Empty;
    
    // 해당 포즈 타입에 따른 에러 메시지 반환
    public string GetErrorMessage(string poseType)
    {
        switch (poseType)
        {
            case "cpr":
                return !cpr && !string.IsNullOrEmpty(cpr_error_msg) 
                    ? cpr_error_msg : "CPR 자세가 올바르지 않습니다.";
            case "infant_airway":
                return !infant_airway && !string.IsNullOrEmpty(infant_airway_error_msg) 
                    ? infant_airway_error_msg : "기도 확보 자세가 올바르지 않습니다.";
            case "infant_compression":
                return !infant_compression && !string.IsNullOrEmpty(infant_compression_error_msg) 
                    ? infant_compression_error_msg : "유아 흉부 압박 자세가 올바르지 않습니다.";
            case "vacuum_pump":
                return !vacuum_pump && !string.IsNullOrEmpty(vacuum_pump_error_msg) 
                    ? vacuum_pump_error_msg : "흡인기 사용 자세가 올바르지 않습니다.";
            default:
                return "자세를 다시 취해주세요.";
        }
    }
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
            // 연결 확인용 메시지 전송
            SendText("{\"type\":\"ping\"}");
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }
            
            try
            {
                // 먼저 JSON 형식인지 확인
                if (!msg.StartsWith("{") || !msg.EndsWith("}"))
                {
                    Debug.LogWarning($"⚠️ 잘못된 JSON 형식: {msg}");
                    return;
                }
                
                PoseRecognitionResult result = JsonConvert.DeserializeObject<PoseRecognitionResult>(msg);
                
                if (result != null && result.type == "pose_result")
                {
                    latestPoseResult = result;
                    OnPoseResultReceived?.Invoke(result);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 메시지 파싱 오류: {ex.Message}");
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
        // websocket 객체가 존재하는지 확인 후 큐 처리
        if (websocket != null) 
        {
            try {
                websocket.DispatchMessageQueue();
            }
            catch (Exception ex) {
                Debug.LogError($"❌ 메시지 큐 처리 중 오류: {ex.Message}");
            }
        }

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
