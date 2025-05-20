using UnityEngine;
using System;

public class TrainingEvaluator : MonoBehaviour
{
    public static TrainingEvaluator Instance;

    // 외부에서 점수 수신을 구독할 수 있도록 이벤트 형태로 제공
    public event Action<int> OnServerResultReceived;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // WebSocket에서 메시지 수신시 처리
        VoiceWebSocketClient.Instance.OnMessageReceived += OnServerResponse;
    }

void OnServerResponse(string msg)
{
    try
    {
        ServerMessage result = JsonUtility.FromJson<ServerMessage>(msg);
        
        if (result.type == "result")
        {
            int rawScore = result.score;
            int normalizedScore = NormalizeScore(rawScore);
            Debug.Log($"🧠 서버 응답 수신 - 원 점수: {rawScore}, 정규화: {normalizedScore}");

            // 이벤트(콜백)로 외부에 전달하기 전에 구독된 메소드 목록 확인
            if (OnServerResultReceived != null)
            {
                Delegate[] subscribers = OnServerResultReceived.GetInvocationList();
                foreach(var sub in subscribers)
                {
                    Debug.Log("구독된 메소드: " + sub.Method.Name + " / 대상: " + sub.Target);
                }
            }
            else{
                Debug.Log("구독된 메소드 없음");
            }

            
            // 이벤트 호출
            OnServerResultReceived?.Invoke(normalizedScore);
        }
    }
    catch (Exception e)
    {
        Debug.LogWarning($"⚠️ 서버 응답 처리 중 오류: {e.Message}");
    }
}


    // 점수 정규화 함수 (0 ~ 2 범위로 분류)
    int NormalizeScore(int score)
    {
        if (score <= 10) return 0;
        if (score <= 70) return 1;
        return 2;
    }

    // 서버에서 받는 메시지 구조
    [System.Serializable]
    public class ServerMessage
    {
        public string type;
        public string action;
        public int score;
    }
}
