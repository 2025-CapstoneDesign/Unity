using UnityEngine;

public class TrainingEvaluator : MonoBehaviour
{
    public static TrainingEvaluator Instance;

    private bool isWaiting = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        WebSocketClient.Instance.OnMessageReceived += OnServerResponse;
    }

    public void SubmitAction()
    {
        if (isWaiting)
        {
            Debug.Log("⏳ 결과 대기 중...");
            return;
        }

        isWaiting = true;
        Debug.Log("📤 행동 제출 요청");

        // 평가 메시지 전송
        var msg = new ServerMessage { type = "submit", action = "cpr_done" };
        string json = JsonUtility.ToJson(msg);
        WebSocketClient.Instance.SendText(json);
    }

    void OnServerResponse(string msg)
    {
        ServerMessage result = JsonUtility.FromJson<ServerMessage>(msg);

        if (result.type == "result")
        {
            isWaiting = false;
            Evaluate(result.score);
        }
    }

    void Evaluate(int score)
    {
        Debug.Log($"🧠 점수 수신: {score}");

        VoiceSender.Instance.StopCapture(); // 녹음 종료

        if (score >= 80)
        {
            Debug.Log("✅ 통과! 다음 단계로 이동");
            LessonManager.Instance.GoToNextStep();
        }
        else
        {
            Debug.Log("❌ 실패. 다시 시도하세요");
        }
    }

    [System.Serializable]
    public class ServerMessage
    {
        public string type;
        public string action;
        public int score;
    }
}
