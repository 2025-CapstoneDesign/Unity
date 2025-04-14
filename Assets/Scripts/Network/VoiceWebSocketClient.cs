using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance;

    public Action<string> OnMessageReceived;

    private WebSocket websocket;
    private bool isConnecting = false;
    private bool isManuallyClosed = false;
    private float reconnectInterval = 5f;
    private float reconnectTimer = 0f;

    [Header("WebSocket 설정")]
    [Tooltip("ws:// 형식의 WebSocket 주소를 입력하세요")]
    public string serverUrl = "ws://localhost:10050"; // ✅ 인스펙터에서 수정 가능

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
    }

    public async void SendBytes(byte[] data)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Send(data);
        }
        else
        {
            Debug.LogWarning("📤 VoiceWebSocket 연결되지 않음, 전송 실패");
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
