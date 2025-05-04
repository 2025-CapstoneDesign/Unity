using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class InfantAirwayManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private InfantAirwayUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private InfantAirwayState currentState;
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
    private Dictionary<InfantAirwayState, float> stageTimeLimit = new Dictionary<InfantAirwayState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수

    void Start()
    {
        cprValidator = new CPRValidator(uiManager);
        breathValidator = new BreathValidator(uiManager);
        currentState = InfantAirwayState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(InfantAirwayState)).Length - 1;
        setState(InfantAirwayState.EnsureSceneSafety);
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
        // 영아기도폐쇄에 맞는 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[InfantAirwayState.EnsureSceneSafety] = 10f;
        stageTimeLimit[InfantAirwayState.WearPPE] = 15f;
        stageTimeLimit[InfantAirwayState.Call119AndRequestAED] = 15f;
        stageTimeLimit[InfantAirwayState.Perform5BackBlows] = 20f;
        stageTimeLimit[InfantAirwayState.Perform5ChestThrusts] = 20f;
        stageTimeLimit[InfantAirwayState.RepeatBackBlowsAndChestThrusts] = 30f;
        stageTimeLimit[InfantAirwayState.IfUnconsciousPlaceSupine] = 15f;
        stageTimeLimit[InfantAirwayState.Perform30ChestCompressions] = 25f;
        stageTimeLimit[InfantAirwayState.OpenAirwayAndCheckForObstruction] = 20f;
        stageTimeLimit[InfantAirwayState.Perform1RescueBreath] = 15f;
        stageTimeLimit[InfantAirwayState.ReopenAirwayAndPerform1RescueBreath] = 15f;
        stageTimeLimit[InfantAirwayState.Perform30To2CPRCycle] = 30f;
        stageTimeLimit[InfantAirwayState.RecordOnMedicalChart] = 20f;
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
        if (currentState != InfantAirwayState.RecordOnMedicalChart)
        {
            // 현재 단계의 경과 시간을 UI에 표시
            float elapsedTime = Time.time - currentStageStartTime;
            float timeLimit = stageTimeLimit.ContainsKey(currentState) ? stageTimeLimit[currentState] : float.MaxValue;
            uiManager.UpdateTimerUI(timerManager, currentState);
        }
    }

    private void HandleSensorData(string type, float value)
    {
        // 센서 데이터 처리 로직
    }

    private void HandleGyroData(float roll, float pitch)
    {
        // 자이로스코프 데이터 처리 로직
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
            AddError($"{AdapterErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
            uiManager.ShowCheckIconFail(this);
            Debug.Log("🟡 음성 평가 : 오답");
        }
        else
        {
            uiManager.ShowCheckIconFail(this);
            Debug.Log("🔴 음성 평가 : 헛소리");
        }
    }

    private void AddError(string errorMessage, int penaltyPoints = 1)
    {
        if (!checkScore.ContainsKey(errorMessage))
        {
            checkScore.Add(errorMessage, penaltyPoints);
            score -= penaltyPoints;
        }
    }

    private IEnumerator Procedure()
    {
        while (currentState != InfantAirwayState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case InfantAirwayState.EnsureSceneSafety:
                    break;

                case InfantAirwayState.WearPPE:
                    break;

                case InfantAirwayState.Call119AndRequestAED:
                    break;

                case InfantAirwayState.Perform5BackBlows:
                    break;

                case InfantAirwayState.Perform5ChestThrusts:
                    break;

                case InfantAirwayState.RepeatBackBlowsAndChestThrusts:
                    break;

                case InfantAirwayState.IfUnconsciousPlaceSupine:
                    break;

                case InfantAirwayState.Perform30ChestCompressions:
                    break;

                case InfantAirwayState.OpenAirwayAndCheckForObstruction:
                    break;

                case InfantAirwayState.Perform1RescueBreath:
                    break;

                case InfantAirwayState.ReopenAirwayAndPerform1RescueBreath:
                    break;

                case InfantAirwayState.Perform30To2CPRCycle:
                    break;
            }

            yield return new WaitForSeconds(1f); // 반응성을 위해 더 짧은 주기로 체크
        }

        uiManager.ShowCompleteMessage();
        yield return new WaitForSeconds(2f);
        StartCoroutine(GenerateTrainingSummary());
    }

    private IEnumerator GenerateTrainingSummary()
    {
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "영아 기도폐쇄 훈련 평가 단계", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 영아 기도폐쇄 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));

        SaveResultToGameManager();
        StoreHistory();
        SceneManager.LoadScene("FeedbackScene"); // 결과 씬 이름으로 이동
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

        GameManager.Instance.protocolName = "영아 기도폐쇄";
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
            protocolName = "영아 기도폐쇄",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
    }

    private void setState(InfantAirwayState nextState)
    {
        // 단계 변경 시 시작 시간 갱신
        currentStageStartTime = Time.time;

        // 이전 단계에서 제한 시간이 있는 경우, 초과 시간에 대해 점수 차감
        if (currentState != nextState && stageTimeLimit.ContainsKey(currentState))
        {
            float elapsedTime = Time.time - currentStageStartTime;
            float timeLimit = stageTimeLimit[currentState];
            
            if (elapsedTime > timeLimit && currentState != InfantAirwayState.RecordOnMedicalChart)
            {
                int exceededSeconds = Mathf.FloorToInt(elapsedTime - timeLimit);
                int penalty = exceededSeconds * TIME_PENALTY_PER_SECOND;
                
                if (penalty > 0)
                {
                    AddError($"{AdapterErrorType.GetLabel(currentState)} 단계 시간 초과 ({exceededSeconds}초)", penalty);
                }
            }
        }

        currentState = nextState;
        VoiceSender.Instance.CurrentStageTag = nextState.ToString();
        Debug.Log($"➡️ 상태 전환: {currentState}");
    }

    private void initPlag()
    {
        voicePassed = false;
        sensorPassed = false;
        handTrackingPassed = false;
        eyeTrackingPassed = false;
        markerPositionFirstPassed = false;
        markerPositionSecondPassed = false;
        markerDistancePassed = false;
        gyroPassed = false;
        flowPassed = false;
        pressurePassed = false;
    }

    private void ResetValidationFlags()
    {
        initPlag();
        
        fiveCycleCount = 0;
        cprValidator.Reset();
        breathValidator.Reset();
    }

    // 상태 설정 무매개변수 메서드 추가
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

    // 이벤트 등록 및 해제
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

    // 예시: 압박 깊이가 부족할 때
    public void OnCompressionDepthError()
    {
        AddError("가슴압박 깊이 부족", 1);
    }

    // 예시: 압박 속도가 불규칙할 때
    public void OnCompressionRateError()
    {
        AddError("압박 속도 불규칙", 1);
    }

    // 예시: 인공호흡이 부족할 때
    public void OnBreathingError()
    {
        AddError("인공호흡 부족", 1);
    }

    // 수동으로 피드백 생성을 호출하고 싶을 때
    public void GenerateFeedbackNow()
    {
        StartCoroutine(GenerateTrainingSummary());
    }
}
