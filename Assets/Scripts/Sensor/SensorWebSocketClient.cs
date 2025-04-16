using UnityEngine;
using NativeWebSocket;
using System.Text;
using Newtonsoft.Json.Linq;
using System;

public class SensorWebSocketClient : MonoBehaviour
{
    [Header("WebSocket 설정")]
    [Tooltip("ws:// 형식의 WebSocket 주소를 입력하세요")]
    public string serverAddress = "ws://localhost:10049";

    private WebSocket websocket;
    private bool isConnecting = false;
    private float reconnectInterval = 5f;
    private float reconnectTimer = 0f;

    async void Start()
    {
        await Connect();
    }

    async System.Threading.Tasks.Task Connect()
    {
        if (isConnecting || (websocket != null && websocket.State == WebSocketState.Open))
            return;

        isConnecting = true;
        Debug.Log("🌐 SensorWebSocket 연결 시도 중...");

        websocket = new WebSocket(serverAddress);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ SensorWebSocket 연결됨");
            isConnecting = false;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("❌ SensorWebSocket 에러: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.LogWarning("🔌 SensorWebSocket 닫힘");
            isConnecting = false;
            reconnectTimer = reconnectInterval;
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            // Debug.Log("📩 SensorWebSocket 수신 메시지: " + message);

            try
            {
                JArray sensorArray = JArray.Parse(message);

                foreach (var sensor in sensorArray)
                {
                    string type = sensor["type"]?.ToString();

                    switch (type)
                    {
                        case "자이로 센서":
                            float roll = sensor["roll"]?.ToObject<float>() ?? 0f;
                            float pitch = sensor["pitch"]?.ToObject<float>() ?? 0f;
                            SensorEvents.OnGyroDataReceived?.Invoke(roll, pitch);
                            break;

                        case "유량 센서":
                        case "압력 센서":
                            float value = sensor["value"]?.ToObject<float>() ?? 0f;
                            SensorEvents.OnSensorDataReceived?.Invoke(type, value);
                            break;

                        default:
                            Debug.LogWarning("⚠️ 알 수 없는 센서 타입: " + type);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("⚠️ SensorWebSocket JSON 파싱 실패: " + ex.Message);
            }
        };

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("⚠️ SensorWebSocket 연결 실패: " + ex.Message);
            isConnecting = false;
            reconnectTimer = reconnectInterval;
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        if (!isConnecting && (websocket == null || websocket.State != WebSocketState.Open))
        {
            reconnectTimer -= Time.deltaTime;
            if (reconnectTimer <= 0f)
            {
                _ = Connect(); // 재연결 시도
            }
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
