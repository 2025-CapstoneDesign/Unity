using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class SuctionManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private SuctionUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private SuctionState currentState;
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
    private bool turnPassed = false;

    private int fiveCycleCount = 0;

    public EyeTrackingValidate eyeTrackingValidator;
    public HandTrackingValidate handValidator;
    public MarkerPositionValidate markerPositionValidator;
    public MarkerDistanceValidate markerDistanceValidator;
    public MoveValidation moveValidator;
    public TurnValidation turnValidator;

    private CPRValidator cprValidator;
    private BreathValidator breathValidator;

    private int score = 100;
    private Dictionary<string, int> checkScore = new Dictionary<string, int>();
    String feedback = "wait";

    // 각 단계별 제한 시간 (초)
    private Dictionary<SuctionState, float> stageTimeLimit = new Dictionary<SuctionState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수

    private bool hasPlayedVoice = false;

    void Start()
    {
        cprValidator = new CPRValidator(uiManager, "Adult");
        breathValidator = new BreathValidator(uiManager, "Adult");
        currentState = SuctionState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(SuctionState)).Length - 1;
        setState(SuctionState.EnsureSceneSafety);
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
        // 흡인 및 산소공급 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[SuctionState.EnsureSceneSafety] = 10f;
        stageTimeLimit[SuctionState.WearPPE] = 15f;
        stageTimeLimit[SuctionState.CheckEquipmentAndSupplies] = 15f;
        stageTimeLimit[SuctionState.TurnOnSuctionDevice] = 10f;
        stageTimeLimit[SuctionState.CheckSuctionPressure] = 15f;
        stageTimeLimit[SuctionState.TestSuctionWithSaline] = 15f;
        stageTimeLimit[SuctionState.PerformOralSuction] = 20f;
        stageTimeLimit[SuctionState.FlushSuctionTipWithSaline] = 15f;
        stageTimeLimit[SuctionState.TurnOffSuctionDevice] = 10f;
        stageTimeLimit[SuctionState.AssembleOxygenTankAndRegulator] = 20f;
        stageTimeLimit[SuctionState.OpenOxygenTankValve] = 10f;
        stageTimeLimit[SuctionState.CheckForLeaksAndStateNoLeaks] = 15f;
        stageTimeLimit[SuctionState.CheckOxygenGaugeAndStateRemainingPressure] = 15f;
        stageTimeLimit[SuctionState.ConnectNonRebreatherMask] = 15f;
        stageTimeLimit[SuctionState.SetOxygenFlowRate] = 15f;
        stageTimeLimit[SuctionState.FillReservoirBagAndApplyMask] = 20f;
        stageTimeLimit[SuctionState.MonitorPatientRespiration] = 20f;
        stageTimeLimit[SuctionState.RemoveMaskUponInstruction] = 10f;
        stageTimeLimit[SuctionState.TurnOffFlowMeterAndTank] = 15f;
        stageTimeLimit[SuctionState.RecordOnMedicalChart] = 30f;
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
            StoreHistory();
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
            AddError($"{SuctionToErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
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
        while (currentState != SuctionState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case SuctionState.EnsureSceneSafety:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.WearPPE);
                    break;

                case SuctionState.WearPPE:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.CheckEquipmentAndSupplies);
                    break;

                case SuctionState.CheckEquipmentAndSupplies:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.TurnOnSuctionDevice);
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case SuctionState.TurnOnSuctionDevice:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!handTrackingPassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.CheckSuctionPressure);
                    break;

                case SuctionState.CheckSuctionPressure:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.PerformOralSuction);
                    break;

                case SuctionState.PerformOralSuction:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.FlushSuctionTipWithSaline);
                    break;

                case SuctionState.FlushSuctionTipWithSaline:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.TurnOffSuctionDevice);
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case SuctionState.TurnOffSuctionDevice:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!handTrackingPassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.AssembleOxygenTankAndRegulator);
                    break;

                case SuctionState.AssembleOxygenTankAndRegulator:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.OpenOxygenTankValve);
                    turnValidator.BeginValidation(
                    markerId: 1,       // 1번 마커 검증
                    targetAngle: 90f,  // 90도 회전 목표
                    tolerance: 5f,     // 오차 ±5도 허용
                    stayTime: 1.0f,    // 1초간 유지해야 함
                    onSuccess: setTurnPassed  // 성공 시 호출될 콜백
                    );
                    break;

                case SuctionState.OpenOxygenTankValve:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!turnPassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.CheckOxygenGaugeAndStateRemainingPressure);
                    break;

                case SuctionState.CheckOxygenGaugeAndStateRemainingPressure:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.ConnectNonRebreatherMask);
                    break;

                case SuctionState.ConnectNonRebreatherMask:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.SetOxygenFlowRate);
                    break;

                case SuctionState.SetOxygenFlowRate:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.FillReservoirBagAndApplyMask);
                    break;

                case SuctionState.FillReservoirBagAndApplyMask:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.RemoveMaskUponInstruction);
                    break;

                case SuctionState.RemoveMaskUponInstruction:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.TurnOffFlowMeterAndTank);
                    break;

                case SuctionState.TurnOffFlowMeterAndTank:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SuctionState.RecordOnMedicalChart);
                    break;

                case SuctionState.RecordOnMedicalChart:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    break;
            }

            yield return new WaitForSeconds(1f); // 반응성을 위해 더 짧은 주기로 체크
        }

        uiManager.ShowCompleteMessage();
        // (1) 피드백 먼저 생성한다
        yield return StartCoroutine(GenerateTrainingSummary());

        SaveResultToGameManager();
        StoreHistory();
        SceneManager.LoadScene("FeedbackScene"); // 결과 씬 이름으로 이동
    }

    private IEnumerator GenerateTrainingSummary()
    {
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "흡인 및 산소공급 훈련 평가 단계", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 흡인 및 산소공급 훈련 요약:\n{feedback}");
        }));
    }

    private void PlayVoiceForStage(SuctionState state)
    {
        int stageNumber = (int)state;
        string path = $"SceneStage/Oxygenation/Oxygenation{stageNumber + 1}";

        if (AudioManager.Instance != null)
        {
            Debug.Log($"🔊 흡인 및 산소공급 단계 {stageNumber} 오디오 재생: {path}");
            AudioManager.Instance.PlayVoice(path);
        }
        else
        {
            Debug.LogWarning("❗ AudioManager.Instance is NULL! 음성을 재생할 수 없습니다.");
        }
    }

    private void setState(SuctionState nextState)
    {
        // 이전 단계의 시간 초과 여부 확인 및 점수 차감
        if (stageTimeLimit.ContainsKey(currentState))
        {
            float elapsedTime = Time.time - currentStageStartTime;
            float timeLimit = stageTimeLimit[currentState];

            if (elapsedTime > timeLimit)
            {
                // 3초마다 1점씩 차감하도록 수정
                int penaltySeconds = Mathf.FloorToInt(elapsedTime - timeLimit);
                if (penaltySeconds > 0)
                {
                    // 3초마다 1점 차감 (기존: 1초당 1점)
                    int penalty = Mathf.FloorToInt(penaltySeconds / 3f);
                    if (penalty > 0) // 최소 3초 이상 초과했을 때만 패널티 적용
                    {
                        string errorKey = $"{currentState} 단계 시간 초과";
                        AddError(errorKey, penalty);
                        Debug.Log($"⏰ 시간 초과 패널티: -{penalty}점 (현재 점수: {score})");
                    }
                }
            }
        }

        // 새 단계로 상태 변경 및 시작 시간 기록
        currentStageStartTime = Time.time;
        currentState = nextState;
        VoiceSender.Instance.CurrentStageTag = nextState.ToVoiceTag();
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
        hasPlayedVoice = false;
        turnPassed = false;
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

    public void setTurnPassed()
    {
        turnPassed = true;
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

        GameManager.Instance.protocolName = "흡인 및 산소공급";
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
            protocolName = "흡인 및 산소공급",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
    }
}
