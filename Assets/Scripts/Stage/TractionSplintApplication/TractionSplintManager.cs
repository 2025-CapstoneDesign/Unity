using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class TractionSplintManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private TractionSplintUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private TractionSplintState currentState;
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
    private Dictionary<TractionSplintState, float> stageTimeLimit = new Dictionary<TractionSplintState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수

    private bool hasPlayedVoice = false;

    void Start()
    {
        cprValidator = new CPRValidator(uiManager, "Adult");
        breathValidator = new BreathValidator(uiManager, "Adult");
        currentState = TractionSplintState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(TractionSplintState)).Length - 1;
        setState(TractionSplintState.EnsureSceneSafety);
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
        // 견인 부목 적용에 맞는 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[TractionSplintState.EnsureSceneSafety] = 10f;
        stageTimeLimit[TractionSplintState.WearPPE] = 15f;
        stageTimeLimit[TractionSplintState.ExposeAndSupportFracture] = 20f;
        stageTimeLimit[TractionSplintState.AssessDistalPulseMotorSensation] = 20f;
        stageTimeLimit[TractionSplintState.ApplyManualTractionAndDelegate] = 25f;
        stageTimeLimit[TractionSplintState.MeasureSplintLength] = 15f;
        stageTimeLimit[TractionSplintState.ApplyTractionSplint] = 30f;
        stageTimeLimit[TractionSplintState.ApplyIschialStrap] = 20f;
        stageTimeLimit[TractionSplintState.ApplyAnkleHitch] = 20f;
        stageTimeLimit[TractionSplintState.ConnectAndTightenAnkleTraction] = 25f;
        stageTimeLimit[TractionSplintState.ApplySupportStraps] = 20f;
        stageTimeLimit[TractionSplintState.ReassessDistalPMS] = 15f;
        stageTimeLimit[TractionSplintState.StateLogRollTransferToSpineBoard] = 30f;
        stageTimeLimit[TractionSplintState.RecordOnMedicalChart] = 20f;
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
            AddError($"{TractionSplintToErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
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
        while (currentState != TractionSplintState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case TractionSplintState.EnsureSceneSafety:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.WearPPE);
                    break;

                case TractionSplintState.WearPPE:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    wearPassed = true;
                    if (!wearPassed || !voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ExposeAndSupportFracture);
                    // 다음 단계에 필요한 마커 검증 시작
                    markerPositionValidator.BeginValidation(1, 11, new Vector3(0.1f, 0.1f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    break;

                case TractionSplintState.ExposeAndSupportFracture:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.AssessDistalPulseMotorSensation);
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case TractionSplintState.AssessDistalPulseMotorSensation:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!handTrackingPassed) {
                        break;
                    }
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ApplyManualTractionAndDelegate);
                    handValidator.BeginVerification(2, new Vector3(0.1f, 0.1f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case TractionSplintState.ApplyManualTractionAndDelegate:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!handTrackingPassed) {
                        voicePassed = false;
                        break;
                    }
                    if (!voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.MeasureSplintLength);
                    markerDistanceValidator.BeginValidation(2, 4, 0.2f, 0.3f, setMarkerDistancePassed);
                    break;

                case TractionSplintState.MeasureSplintLength:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerDistancePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ApplyTractionSplint);
                    markerPositionValidator.BeginValidation(1, 12, new Vector3(0.2f, 0.15f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    break;

                case TractionSplintState.ApplyTractionSplint:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ApplyIschialStrap);
                    markerPositionValidator.BeginValidation(1, 13, new Vector3(0.3f, 0.2f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    break;

                case TractionSplintState.ApplyIschialStrap:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ApplyAnkleHitch);
                    markerPositionValidator.BeginValidation(1, 14, new Vector3(-0.1f, -0.15f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    break;

                case TractionSplintState.ApplyAnkleHitch:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ConnectAndTightenAnkleTraction);
                    markerPositionValidator.BeginValidation(1, 15, new Vector3(-0.2f, -0.1f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    break;

                case TractionSplintState.ConnectAndTightenAnkleTraction:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ApplySupportStraps);
                    markerPositionValidator.BeginValidation(1, 16, new Vector3(0.05f, 0.05f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    handValidator.BeginVerification(3, new Vector3(0f, 0.1f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case TractionSplintState.ApplySupportStraps:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!markerPositionFirstPassed || !handTrackingPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.ReassessDistalPMS);
                    handValidator.BeginVerification(4, new Vector3(0f, -0.1f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case TractionSplintState.ReassessDistalPMS:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!handTrackingPassed) {
                        break;
                    }
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.StateLogRollTransferToSpineBoard);
                    break;

                case TractionSplintState.StateLogRollTransferToSpineBoard:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(TractionSplintState.RecordOnMedicalChart);
                    break;
            }

            yield return new WaitForSeconds(1f); // 반응성을 위해 더 짧은 주기로 체크
        }

        uiManager.ShowCompleteMessage();
        yield return StartCoroutine(GenerateTrainingSummary());
        SaveResultToGameManager();
        StoreHistory();
        SceneManager.LoadScene("FeedbackScene");
    }

    private void PlayVoiceForStage(TractionSplintState state)
    {
        int stageNumber = (int)state;
        string path = $"SceneStage/Splint/Splint{stageNumber + 1}";

        if (AudioManager.Instance != null)
        {
            Debug.Log($"🔊 견인부목 적용 단계 {stageNumber} 오디오 재생: {path}");
            AudioManager.Instance.PlayVoice(path);
        }
        else
        {
            Debug.LogWarning("❗ AudioManager.Instance is NULL! 음성을 재생할 수 없습니다.");
        }
    }

    private IEnumerator GenerateTrainingSummary()
    {
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "견인 부목 적용 훈련 평가 단계", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 견인 부목 적용 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }

    private void setState(TractionSplintState nextState)
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

        GameManager.Instance.protocolName = "견인 부목 적용";
        GameManager.Instance.duration = durationString;
        GameManager.Instance.score = score;
        GameManager.Instance.feedback = feedback;
    }

    void StoreHistory()
    {
        // 오늘 날짜를 yyyy-MM-dd 형식으로
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        // 경과 시간을 분과 초로 변환
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

        TrainingResult newResult = new TrainingResult
        {
            protocol_name = "견인 부목 적용",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
    }
}
