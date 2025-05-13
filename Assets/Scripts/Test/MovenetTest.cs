using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovenetTest : MonoBehaviour
{
    [Header("UI 요소 (선택사항)")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI cprStatusText;
    public TextMeshProUGUI infantAirwayStatusText;
    public TextMeshProUGUI infantCompressionStatusText;
    public TextMeshProUGUI vacuumPumpStatusText;
    public TextMeshProUGUI lastUpdateTimeText;

    [Header("테스트 도구 (선택사항)")]
    public Button simulateButton;
    public Toggle cprToggle;
    public Toggle infantAirwayToggle;
    public Toggle infantCompressionToggle;
    public Toggle vacuumPumpToggle;

    [Header("디버그 설정")]
    public bool useDebugLog = true;  // 항상 콘솔에 로그 출력
    public KeyCode simulateKeyCode = KeyCode.Space;  // 스페이스바로 시뮬레이션 실행
    public bool autoSimulateInEditor = false;  // 에디터에서 자동 시뮬레이션 실행
    public float autoSimulateInterval = 3f;    // 자동 시뮬레이션 간격 (초)
    
    // 마지막 업데이트 시간 추적
    private float lastUpdateTime = 0f;
    private string lastReceivedJson = "";
    private bool hasReceivedData = false;
    private float autoSimulateTimer = 0f;

    // MovenetSocketClient 인스턴스 (시작할 때 생성)
    private MovenetSocketClient socketClient;

    void Start()
    {
        // MovenetSocketClient가 씬에 없는 경우 동적으로 생성
        socketClient = MovenetSocketClient.Instance;
        if (socketClient == null)
        {
            Debug.Log("MovenetTest: MovenetSocketClient를 동적으로 생성합니다.");
            GameObject clientObj = new GameObject("MovenetSocketClient");
            socketClient = clientObj.AddComponent<MovenetSocketClient>();
            MovenetSocketClient.Instance = socketClient;
        }

        // MovenetSocketClient 이벤트에 구독
        socketClient.OnPoseResultReceived += OnPoseResultReceived;
        
        // 시뮬레이션 버튼 설정 (UI가 있는 경우)
        if (simulateButton != null)
        {
            simulateButton.onClick.AddListener(SimulatePoseResult);
        }

        LogMessage("MovenetTest: 포즈 인식 이벤트 구독 완료");
        UpdateStatusText("웹소켓 연결 대기 중...");

        // 초기 UI 업데이트
        UpdateAllStatusTexts();
    }

    void Update()
    {
        // 키 입력으로 시뮬레이션 실행
        if (Input.GetKeyDown(simulateKeyCode))
        {
            SimulatePoseResult();
        }

        // 자동 시뮬레이션 (에디터에서만 동작)
        if (autoSimulateInEditor && Application.isEditor)
        {
            autoSimulateTimer -= Time.deltaTime;
            if (autoSimulateTimer <= 0f)
            {
                autoSimulateTimer = autoSimulateInterval;
                SimulatePoseResult();
            }
        }

        // 연결 상태 업데이트 및 디버그
        if (socketClient != null)
        {
            bool isConnected = socketClient.IsConnected();
            
            // 연결 상태가 변경되었을 때만 로그 출력
            if (isConnected && (statusText == null || statusText.text.Contains("연결 대기") || statusText.text.Contains("연결 끊김")))
            {
                LogMessage("웹소켓 연결됨");
                UpdateStatusText("웹소켓 연결됨");
            }
            else if (!isConnected && (statusText == null || !statusText.text.Contains("연결 대기") && !statusText.text.Contains("연결 끊김")))
            {
                LogMessage("웹소켓 연결 끊김");
                UpdateStatusText("웹소켓 연결 끊김");
            }

            // 데이터 수신 타임아웃 체크 (5초 이상 데이터가 없으면 경고)
            if (hasReceivedData && Time.time - lastUpdateTime > 5f)
            {
                if (statusText == null || !statusText.text.Contains("데이터 수신 없음"))
                {
                    LogMessage("웹소켓 연결됨 (5초 이상 데이터 수신 없음)");
                    UpdateStatusText("웹소켓 연결됨 (5초 이상 데이터 수신 없음)");
                }
            }
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (socketClient != null)
        {
            socketClient.OnPoseResultReceived -= OnPoseResultReceived;
        }
    }

    // MovenetSocketClient에서 포즈 데이터가 도착할 때 호출되는 이벤트 핸들러
    private void OnPoseResultReceived(PoseRecognitionResult result)
    {
        hasReceivedData = true;
        lastUpdateTime = Time.time;
        lastReceivedJson = JsonUtility.ToJson(result, true); // pretty print
        
        LogMessage($"[포즈 인식 데이터 수신]" +
                 $"\n타입: {result.type}" + 
                 $"\nCPR: {(result.cpr ? "✓ 올바름" : "✗ 틀림")}" +
                 $"\n유아 기도 확보: {(result.infant_airway ? "✓ 올바름" : "✗ 틀림")}" +
                 $"\n유아 흉부 압박: {(result.infant_compression ? "✓ 올바름" : "✗ 틀림")}" +
                 $"\n흡인기 사용: {(result.vacuum_pump ? "✓ 올바름" : "✗ 틀림")}" +
                 $"\n\n원본 JSON: {lastReceivedJson}");
        
        // UI 텍스트 업데이트 (UI가 있는 경우만)
        UpdateStatusText("포즈 데이터 수신됨 (" + System.DateTime.Now.ToString("HH:mm:ss") + ")");
        UpdatePoseStatusTexts(result);
    }

    // 테스트용 포즈 인식 결과 시뮬레이션
    public void SimulatePoseResult()
    {
        PoseRecognitionResult simulatedResult = new PoseRecognitionResult
        {
            type = "pose_result",
            cpr = cprToggle != null ? cprToggle.isOn : Random.value > 0.5f,
            infant_airway = infantAirwayToggle != null ? infantAirwayToggle.isOn : Random.value > 0.5f,
            infant_compression = infantCompressionToggle != null ? infantCompressionToggle.isOn : Random.value > 0.5f,
            vacuum_pump = vacuumPumpToggle != null ? vacuumPumpToggle.isOn : Random.value > 0.5f
        };

        LogMessage("시뮬레이션된 포즈 데이터 생성");
        
        // 포즈 결과 시뮬레이션
        if (socketClient != null && socketClient.OnPoseResultReceived != null)
        {
            socketClient.OnPoseResultReceived.Invoke(simulatedResult);
        }
        else
        {
            // MovenetSocketClient가 없는 경우 직접 처리
            OnPoseResultReceived(simulatedResult);
        }
    }

    // 모든 상태 텍스트 한 번에 업데이트 (UI가 있는 경우만)
    private void UpdateAllStatusTexts()
    {
        PoseRecognitionResult currentPose = socketClient?.GetLatestPoseResult();
        if (currentPose != null)
        {
            UpdatePoseStatusTexts(currentPose);
        }
        else
        {
            // 기본 상태 표시
            if (cprStatusText != null) cprStatusText.text = "CPR 자세: 데이터 없음";
            if (infantAirwayStatusText != null) infantAirwayStatusText.text = "유아 기도 확보: 데이터 없음";
            if (infantCompressionStatusText != null) infantCompressionStatusText.text = "유아 흉부 압박: 데이터 없음";
            if (vacuumPumpStatusText != null) vacuumPumpStatusText.text = "흡인기 사용: 데이터 없음";
            if (lastUpdateTimeText != null) lastUpdateTimeText.text = "마지막 업데이트: 없음";
        }
    }

    // 포즈 상태 UI 업데이트 (UI가 있는 경우만)
    private void UpdatePoseStatusTexts(PoseRecognitionResult pose)
    {
        if (cprStatusText != null)
            cprStatusText.text = $"CPR 자세: {(pose.cpr ? "✓ 올바름" : "✗ 틀림")}";
        
        if (infantAirwayStatusText != null)
            infantAirwayStatusText.text = $"유아 기도 확보: {(pose.infant_airway ? "✓ 올바름" : "✗ 틀림")}";
        
        if (infantCompressionStatusText != null)
            infantCompressionStatusText.text = $"유아 흉부 압박: {(pose.infant_compression ? "✓ 올바름" : "✗ 틀림")}";
        
        if (vacuumPumpStatusText != null)
            vacuumPumpStatusText.text = $"흡인기 사용: {(pose.vacuum_pump ? "✓ 올바름" : "✗ 틀림")}";
        
        if (lastUpdateTimeText != null)
            lastUpdateTimeText.text = $"마지막 업데이트: {System.DateTime.Now.ToString("HH:mm:ss")}\n{lastReceivedJson}";
    }

    // 상태 텍스트 업데이트 (UI가 있는 경우만)
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    // 항상 콘솔에 로그 출력
    private void LogMessage(string message)
    {
        if (useDebugLog)
        {
            Debug.Log($"[MovenetTest] {message}");
        }
    }
}
