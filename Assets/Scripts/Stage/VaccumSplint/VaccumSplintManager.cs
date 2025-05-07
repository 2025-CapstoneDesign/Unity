using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class VaccumSplintManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private VaccumSplintUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private VacuumSplintState currentState;
    private bool externalInput = false;
    private int totalSteps;
    private bool wearPassed = false;
    private bool voicePassed = false;
    private bool sensorPassed = false;
    private bool handTrackingPassed = false;
    private bool eyeTrackingPassed = false;
    private bool markerPositionFirstPassed = false;
    private bool markerPositionSecondPassed = false;
    private bool markerDistancePassed = false;
    private bool gyroPassed = false;
    private bool flowPassed = false;
    private bool pressurePassed = false;

    private int fiveCycleCount = 0;

    public EyeTrackingValidate eyeTrackingValidator;
    public HandTrackingValidate handValidator;
    public MarkerPositionValidate markerPositionValidator;
    public MarkerDistanceValidate markerDistanceValidator;
    public MoveValidation moveValidator;

    private CPRValidator cprValidator;
    private BreathValidator breathValidator;

    private int score = 100;
    private Dictionary<string, int> checkScore = new Dictionary<string, int>();
    String feedback = "wait";

    // 각 단계별 제한 시간 (초)
    private Dictionary<VacuumSplintState, float> stageTimeLimit = new Dictionary<VacuumSplintState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수

    void Start()
    {
        cprValidator = new CPRValidator(uiManager);
        breathValidator = new BreathValidator(uiManager);
        currentState = VacuumSplintState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(VacuumSplintState)).Length - 1;
        setState(VacuumSplintState.EnsureSceneSafety);
        ResetValidationFlags();
        StartCoroutine(InitSequence());
        score = 100;
        uiManager.InitializeUI();

        // 각 단계별 제한 시간 설정 (초 단위)
        InitializeTimeLimits();
        currentStageStartTime = Time.time;
    }

    private void InitializeTimeLimits()
    {
        // 진공부목 적용에 맞는 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[VacuumSplintState.EnsureSceneSafety] = 10f;
        stageTimeLimit[VacuumSplintState.WearPPE] = 15f;
        stageTimeLimit[VacuumSplintState.ExposeAndSupportFracture] = 20f;
        stageTimeLimit[VacuumSplintState.AssessDistalPMS] = 15f;
        stageTimeLimit[VacuumSplintState.MeasureSplintSize] = 20f;
        stageTimeLimit[VacuumSplintState.ApplySplintToInjury] = 25f;
        stageTimeLimit[VacuumSplintState.AttachVacuumPumpAndEvacuateAir] = 25f;
        stageTimeLimit[VacuumSplintState.ReSecureSplintStraps] = 20f;
        stageTimeLimit[VacuumSplintState.SecureArmToBody] = 20f;
        stageTimeLimit[VacuumSplintState.ReassessDistalPMS] = 15f;
        stageTimeLimit[VacuumSplintState.RecordOnMedicalChart] = 30f;
    }

    private IEnumerator InitSequence()
    {
        yield return new WaitUntil(() => TrainingEvaluator.Instance != null);
        TrainingEvaluator.Instance.OnServerResultReceived -= OnServerResultReceivedHandler;
        TrainingEvaluator.Instance.OnServerResultReceived += OnServerResultReceivedHandler;

        yield return new WaitForSeconds(2f);
        timerManager.StartTimer(300f);
        StartCoroutine(Procedure());
    }

    void Update()
    {
        uiManager.UpdateTimerUI(timerManager, currentState);
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("💾 저장 테스트 시작!");
            StoreHistory(); // 👈 저장 함수 바로 호출
        }
    }

    private void HandleSensorData(string type, float value)
    {
        switch (currentState)
        {
            default:
                break;
        }
    }

    private void HandleGyroData(float roll, float pitch)
    {
        Debug.Log($"📐 자이로 수신 - Roll: {roll}, Pitch: {pitch}");

        switch (currentState)
        {
            case VacuumSplintState.ExposeAndSupportFracture:
                if (!gyroPassed && Mathf.Abs(pitch) > 30f)
                {
                    Debug.Log("🌀 골절 부위 지지 확인 성공!");
                    setGyroPassed();
                }
                break;
            default:
                break;  
        }
    }

    private void OnServerResultReceivedHandler(int score)
    {
        if (score >= 2)
        {
            voicePassed = true;
            Debug.Log("🟢 음성 평가 : 통과");
        }
        else if (score == 1)
        {
            // 단계 + 음성 오답 형태로 기록
            AddError($"{VacuumSplintToErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
            uiManager.ShowCheckIconFail(this);
            Debug.Log("🟡 음성 평가 : 오답");
        }
        else
        {
            uiManager.ShowCheckIconFail(this);
            Debug.Log("🔴 음성 평가 : 헛소리");
        }
    }

    private IEnumerator Procedure()
    {
        while (currentState != VacuumSplintState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress((float)(int)currentState / totalSteps);

            switch (currentState)
            {
                case VacuumSplintState.EnsureSceneSafety:
                    break;
                    
                case VacuumSplintState.WearPPE:
                    break;
                    
                case VacuumSplintState.ExposeAndSupportFracture:
                    break;
                    
                case VacuumSplintState.AssessDistalPMS:
                    break;
                    
                case VacuumSplintState.MeasureSplintSize:
                    break;
                    
                case VacuumSplintState.ApplySplintToInjury:
                    break;
                    
                case VacuumSplintState.AttachVacuumPumpAndEvacuateAir:
                    break;
                    
                case VacuumSplintState.ReSecureSplintStraps:
                    break;
                    
                case VacuumSplintState.SecureArmToBody:
                    break;
                    
                case VacuumSplintState.ReassessDistalPMS:
                    break;
            }

            yield return new WaitForSeconds(1f); // 반응성을 위해 더 짧은 주기로 체크
        }

        uiManager.ShowCompleteMessage();
        // 피드백 생성
        yield return StartCoroutine(GenerateTrainingSummary());

        SaveResultToGameManager();
        StoreHistory();
        SceneManager.LoadScene("FeedbackScene"); // 결과 씬 이름으로 이동
    }

    private IEnumerator GenerateTrainingSummary()
    {
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "진공부목 적용 훈련", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 진공부목 적용 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }

    private void setState(VacuumSplintState nextState)
    {
        // 이전 단계의 시간 초과 여부 확인 및 점수 차감
        if (stageTimeLimit.ContainsKey(currentState))
        {
            float elapsedTime = Time.time - currentStageStartTime;
            float timeLimit = stageTimeLimit[currentState];

            if (elapsedTime > timeLimit)
            {
                // 초과한 초 단위로 점수 차감 (1초당 1점)
                int penaltySeconds = Mathf.FloorToInt(elapsedTime - timeLimit);
                if (penaltySeconds > 0)
                {
                    int penalty = penaltySeconds * TIME_PENALTY_PER_SECOND;

                    // 에러 기록
                    string errorKey = $"{currentState} 단계 시간 초과";
                    AddError(errorKey, penalty);

                    Debug.Log($"⏰ 시간 초과 패널티: -{penalty}점 (현재 점수: {score})");
                }
            }
        }

        // 새 단계로 상태 변경 및 시작 시간 기록
        currentStageStartTime = Time.time;
        currentState = nextState;
        VoiceSender.Instance.CurrentStageTag = nextState.ToString();
        Debug.Log($"➡️ 상태 전환: {currentState}");
    }

    private void initPlag()
    {
        wearPassed = false;
        voicePassed = false;
        sensorPassed = false;
        handTrackingPassed = false;
        eyeTrackingPassed = false;
        gyroPassed = false;
        flowPassed = false;
        pressurePassed = false;
        markerPositionFirstPassed = false;
        markerPositionSecondPassed = false;
        markerDistancePassed = false;
    }

    private void ResetValidationFlags()
    {
        initPlag();

        fiveCycleCount = 0;
        cprValidator.Reset();
        breathValidator.Reset();
    }

    public void setVoicePassed()
    {
        voicePassed = true;
    }

    public void setSensorPassed()
    {
        sensorPassed = true;
    }

    public void setHandTrackingPassed()
    {
        handTrackingPassed = true;
    }

    public void setEyeTrackingPassed()
    {
        eyeTrackingPassed = true;
    }

    public void setMarkerPostionFristPassed()
    {
        markerPositionFirstPassed = true;
    }

    public void setMarkerPositionSecondPassed()
    {
        markerPositionSecondPassed = true;
    }

    public void setGyroPassed()
    {
        gyroPassed = true;
    }

    public void setFlowPassed()
    {
        flowPassed = true;
    }

    public void setPressurePassed()
    {
        pressurePassed = true;
    }

    public void setMarkerDistancePassed()
    {
        markerDistancePassed = true;
    }

    void OnEnable()
    {
        SensorEvents.OnSensorDataReceived += HandleSensorData;
        SensorEvents.OnGyroDataReceived += HandleGyroData;
    }

    void OnDisable()
    {
        SensorEvents.OnSensorDataReceived -= HandleSensorData;
        SensorEvents.OnGyroDataReceived -= HandleGyroData;
    }

    private void AddError(string errorType, int penaltyPoints = 1)
    {
        if (checkScore.ContainsKey(errorType))
        {
            checkScore[errorType] += penaltyPoints;
        }
        else
        {
            checkScore[errorType] = penaltyPoints;
        }

        // 점수 차감
        score -= penaltyPoints;
        score = Mathf.Max(0, score); // 최소 0점

        Debug.Log($"❌ 오류: {errorType} (-{penaltyPoints}점, 현재 점수: {score})");
    }

    // 예시: 압박 깊이가 부족할 때
    public void OnCompressionDepthError()
    {
        AddError("가슴압박 깊이 부족");
    }

    // 예시: 압박 속도가 불규칙할 때
    public void OnCompressionRateError()
    {
        AddError("압박 속도 불규칙");
    }

    // 예시: 인공호흡이 부족할 때
    public void OnBreathingError()
    {
        AddError("인공호흡 부족");
    }

    // 수동으로 피드백 생성을 호출하고 싶을 때
    public void GenerateFeedbackNow()
    {
        StartCoroutine(GenerateTrainingSummary());
    }

    void SaveResultToGameManager()
    {
        // 오늘 날짜를 yyyy-MM-dd 형식으로
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        // 경과 시간(초)을 분으로 변환하고 소수점 없이 정수로 표현
        float elapsedSeconds = timerManager.elapsedTime;
        int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
        int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);

        string durationString = "";

        if (minutes >= 1)
        {
            durationString = $"{minutes}분 {seconds}초";
        }
        else
        {
            durationString = $"{seconds}초";
        }

        GameManager.Instance.protocolName = "진공부목 적용";
        GameManager.Instance.duration = durationString;
        GameManager.Instance.score = score;
        GameManager.Instance.feedback = feedback;
    }

    void StoreHistory()
    {
        // 오늘 날짜를 yyyy-MM-dd 형식으로
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        // 경과 시간(초)을 분으로 변환하고 소수점 없이 정수로 표현
        int minutes = Mathf.FloorToInt(timerManager.elapsedTime / 60f);
        string durationString = minutes + "분";

        TrainingResult newResult = new TrainingResult
        {
            protocolName = "진공부목 적용",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
    }
}
