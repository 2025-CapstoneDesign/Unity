using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// WebSocket 클라이언트 - 보이스 데이터 전송용
/// 
/// Python 코드의 websockets.connect() 기능을 C#으로 구현한 클래스입니다.
/// Python 코드와 동일하게 동작하도록 설계되었습니다:
/// - 서버에 연결 (ws://192.168.1.129:10050)
/// - JSON 형식의 메시지 전송
/// - 바이너리 오디오 데이터 전송
/// - 자동 재연결 처리
/// </summary>
public class VoiceWebSocketClient : MonoBehaviour
{
    public static VoiceWebSocketClient Instance;

    public Action<string> OnMessageReceived;

    private WebSocket websocket;
    private bool isConnecting = false;
    private bool isManuallyClosed = false;
    private float reconnectInterval = 5f;
    private float reconnectTimer = 0f;    [Header("WebSocket 설정")]
    [Tooltip("ws:// 형식의 WebSocket 주소를 입력하세요")]
    public string serverUrl = "ws://192.168.1.129:10050"; // Python 코드의 URI와 동일하게 설정

    void Awake() => Instance = this;

    async void Start()
    {
        Debug.Log("START VoiceWebSocketClient");
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
            Debug.Log("✅ VoiceWebSocket 연결됨");
            isConnecting = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = Encoding.UTF8.GetString(bytes);
            OnMessageReceived?.Invoke(msg);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("❌ VoiceWebSocket 에러: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.LogWarning("🔌 VoiceWebSocket 끊김, 재연결 대기 중...");
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
            Debug.LogError("⚠️ VoiceWebSocket 연결 실패: " + ex.Message);
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
    }    /// <summary>
    /// 바이너리 데이터를 전송합니다. Python websockets 라이브러리의 binary 전송과 호환됩니다.
    /// </summary>
    /// <param name="data">전송할 바이너리 데이터</param>
    public async void SendBytes(byte[] data)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            try {
                await websocket.Send(data);
                // 디버그 로그는 너무 자주 출력되므로 생략 (실제 작동 중에는 필요 없음)
            }
            catch (Exception ex)
            {
                Debug.LogError($"📤 바이너리 데이터 전송 중 오류: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("📤 VoiceWebSocket 연결되지 않음, 전송 실패");
        }
    }    /// <summary>
    /// 텍스트 데이터를 전송합니다. Python의 json.dumps()로 전송하는 것과 호환됩니다.
    /// </summary>
    /// <param name="msg">전송할 텍스트 메시지 (일반적으로 JSON 형식)</param>
    public async void SendText(string msg)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            try {
                await websocket.SendText(msg);
            }
            catch (Exception ex)
            {
                Debug.LogError($"📤 텍스트 데이터 전송 중 오류: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("📤 VoiceWebSocket 연결되지 않음, 전송 실패");
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
}
