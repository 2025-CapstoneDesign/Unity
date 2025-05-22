using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.SceneManagement;

public class InfantCPRManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private InfantCPRUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private InfantCPRState currentState;
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
    private Dictionary<InfantCPRState, float> stageTimeLimit = new Dictionary<InfantCPRState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수
    
    private bool hasPlayedVoice = false;

    void Start()
    {
        cprValidator = new CPRValidator(uiManager, "Infant");
        breathValidator = new BreathValidator(uiManager, "Infant");
        currentState = InfantCPRState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(InfantCPRState)).Length - 1;
        setState(InfantCPRState.EnsureSceneSafety);
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
        // 영유아 심폐소생술에 맞는 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[InfantCPRState.EnsureSceneSafety] = 10f;
        stageTimeLimit[InfantCPRState.WearPPE] = 15f;
        stageTimeLimit[InfantCPRState.CheckConsciousness] = 10f;
        stageTimeLimit[InfantCPRState.Call119AndRequestAED] = 15f;
        stageTimeLimit[InfantCPRState.CheckBreathingAndPulse] = 15f;
        stageTimeLimit[InfantCPRState.Perform30ChestCompressions] = 30f;
        stageTimeLimit[InfantCPRState.OpenAirway] = 15f;
        stageTimeLimit[InfantCPRState.Perform2RescueBreathsWithPocketMask] = 15f;
        stageTimeLimit[InfantCPRState.Perform5CyclesOf30To2CPR] = 150f;
        stageTimeLimit[InfantCPRState.RecordOnMedicalChart] = 30f;
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
        Debug.Log($"📊 센서 데이터 수신 - 타입: {type}, 값: {value}");
        
        switch (currentState)
        {
            case InfantCPRState.Perform30ChestCompressions:
                if (type == "압력 센서" && !pressurePassed)
                {
                    if(value < cprValidator.GetMinPressure()){
                        AddError("영유아 가슴압박 압력 부족");
                    }
                    bool complete = cprValidator.TryAddCompression(value);
                    Debug.Log($"압력 센서 : {cprValidator.compressionTimestamps.Count} 횟수 입니다.");
                    if (complete)
                    {
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();
                        cprValidator.Reset();
                    }
                }
                break;

            case InfantCPRState.Perform2RescueBreathsWithPocketMask:
                if (type == "유량 센서" && !flowPassed)
                {
                    if(value < breathValidator.getBreathFlow()){
                        AddError("인공호흡 호흡량 부족");
                    }
                    bool success = breathValidator.TryAddBreath(value);
                    if (success)
                    {
                        Debug.Log("🌬 인공호흡 2회 성공!");
                        setFlowPassed();
                        breathValidator.Reset();
                    }
                }
                break;

            case InfantCPRState.Perform5CyclesOf30To2CPR:
                if (fiveCycleCount % 2 == 0 && type == "압력 센서" && !pressurePassed)
                {
                    if(value < cprValidator.GetMinPressure()){
                        AddError("영유아 가슴압박 압력 부족");
                    }
                    bool complete = cprValidator.TryAddCompression(value);
                    if (complete)
                    {
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();
                        cprValidator.Reset();
                    }
                }
                else if (fiveCycleCount % 2 == 1 && type == "유량 센서" && !flowPassed)
                {
                    if(value < breathValidator.getBreathFlow()){
                        AddError("인공호흡 호흡량 부족");
                    }
                    bool success = breathValidator.TryAddBreath(value);
                    if (success)
                    {
                        Debug.Log("🌬 인공호흡 2회 성공!");
                        setFlowPassed();
                        breathValidator.Reset();
                    }
                }
                break;
        }
    }

    private void HandleGyroData(float roll, float pitch)
    {
        Debug.Log($"📐 자이로 수신 - Roll: {roll}, Pitch: {pitch}");

        switch (currentState)
        {
            case InfantCPRState.CheckConsciousness:
                if (!gyroPassed && Mathf.Abs(roll) > 10f)
                {
                    Debug.Log("🌀 영유아 의식확인 성공!");
                    setGyroPassed();
                }
                break;

            case InfantCPRState.OpenAirway:
                if (!gyroPassed && Mathf.Abs(pitch) > -10f)
                {
                    Debug.Log("🌀 기도 열기 성공!");
                    setGyroPassed();
                }
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
            AddError($"{InfantCPRToErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
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
        while (currentState != InfantCPRState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case InfantCPRState.EnsureSceneSafety:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.WearPPE);
                    break;

                case InfantCPRState.WearPPE:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.CheckConsciousness);
                    break;

                case InfantCPRState.CheckConsciousness:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!gyroPassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.Call119AndRequestAED);
                    break;

                case InfantCPRState.Call119AndRequestAED:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.CheckBreathingAndPulse);
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case InfantCPRState.CheckBreathingAndPulse:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    // 1. 손 인식 먼저 기다리기
                    if (!handTrackingPassed)
                    {
                        voicePassed = false; // 이전 음성 평가 초기화
                        Debug.Log("✋ 손 위치 인식 대기 중...");
                        break;
                    }

                    // 2. 손 인식 통과 후 음성 평가 기다림
                    if (!voicePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.Perform30ChestCompressions);
                    break;

                case InfantCPRState.Perform30ChestCompressions:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!pressurePassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.OpenAirway);
                    break;

                case InfantCPRState.OpenAirway:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!gyroPassed) break;

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.Perform2RescueBreathsWithPocketMask);
                    break;

                case InfantCPRState.Perform2RescueBreathsWithPocketMask:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!flowPassed)
                    {
                        // 호흡 센서 확인 로직
                        break;
                    }

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.Perform5CyclesOf30To2CPR);
                    break;

                case InfantCPRState.Perform5CyclesOf30To2CPR:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (fiveCycleCount < 10)
                    {
                        if (fiveCycleCount % 2 == 0 && pressurePassed)
                        {
                            fiveCycleCount++;
                            flowPassed = false;
                        }
                        else if (fiveCycleCount % 2 == 1 && flowPassed)
                        {
                            fiveCycleCount++;
                            pressurePassed = false;
                        }
                        break;
                    }

                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(InfantCPRState.RecordOnMedicalChart);
                    break;
                    
                case InfantCPRState.RecordOnMedicalChart:
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
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "영아 심폐소생술 훈련 평가 단계",(result) =>
        {
            feedback = result;
            Debug.Log($"📝 영아 심폐소생술 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }

    private void PlayVoiceForStage(InfantCPRState state)
    {
        int stageNumber = (int)state;
        string path = $"SceneStage/InfantCpr/InfantCpr{stageNumber + 1}";

        if (AudioManager.Instance != null)
        {
            Debug.Log($"🔊 영유아 심폐소생술 단계 {stageNumber} 오디오 재생: {path}");
            AudioManager.Instance.PlayVoice(path);
        }
        else
        {
            Debug.LogWarning("❗ AudioManager.Instance is NULL! 음성을 재생할 수 없습니다.");
        }
    }

    private void setState(InfantCPRState nextState)
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

    private void GenerateFeedback()
    {
        StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "영아 심폐소생술 훈련 평가 단계", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 영아 심폐소생술 피드백:\n{feedback}");
            // TODO: UI 업데이트 등 필요 시
        }));
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

        GameManager.Instance.protocolName = "영아 심폐소생술";
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
            protocolName = "영아 심폐소생술",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        ResultHistoryManager.SaveNewResult(newResult);
    }
}
