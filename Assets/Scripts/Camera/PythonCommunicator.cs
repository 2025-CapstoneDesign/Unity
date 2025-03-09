using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;

public class PythonCommunicator : MonoBehaviour
{
    private string serverIP = "192.168.1.182";  // Python 서버 실행 중인 PC의 IP
    private int serverPort = 5000;
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;
    private bool isTryingToReconnect = false;

    void Start()
    {
        Debug.Log("🔄 Python server connection initializing...");
        ConnectToServer();
    }

    void ConnectToServer()
    {
        if (isConnected || isTryingToReconnect) return;  // 중복 연결 방지
        isTryingToReconnect = true;

        new Thread(() =>
        {
            while (!isConnected)
            {
                try
                {
                    client = new TcpClient(serverIP, serverPort);
                    stream = client.GetStream();
                    isConnected = true;
                    isTryingToReconnect = false;
                    Debug.Log("✅ Connected to Python Server!");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Connection failed, retrying... ({e.Message})");
                    Thread.Sleep(2000);  // 2초 후 재시도
                }
            }
        }).Start();
    }

    public void SendFrameToPython(byte[] imageData)
    {
        if (!isConnected) return;

        try
        {
            Debug.Log($"📤 Sending {imageData.Length} bytes...");
            stream.Write(imageData, 0, imageData.Length);
            Debug.Log("✅ Frame sent!");

            // Python 서버에서 응답 받기
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Debug.Log($"📥 Received marker data: {jsonData}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Connection lost: {e.Message}");
            isConnected = false;
            ConnectToServer();  // 연결 끊기면 재연결 시도
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("🔌 Closing connection...");
        stream?.Close();
        client?.Close();
        isConnected = false;
    }
}
