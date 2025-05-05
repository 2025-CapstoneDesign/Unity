// Refactored TestAEDManager with full UIManager integration
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.SceneManagement;

public class TestAEDManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private AEDUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;


    private CPRState currentState;
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

    public HandTrackingValidate handValidator;
    public MarkerPositionValidate markerPositionValidator;

    private CPRValidator cprValidator;
    private BreathValidator breathValidator;

    private int score = 100;
    private Dictionary<string, int> checkScore = new Dictionary<string, int>();
    String feedback = "wait";
    
    // 각 단계별 제한 시간 (초)
    private Dictionary<CPRState, float> stageTimeLimit = new Dictionary<CPRState, float>();
    
    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;
    
    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수

    void Start()
    {
        cprValidator = new CPRValidator(uiManager);
        breathValidator = new BreathValidator(uiManager);
        currentState = CPRState.CheckSafety;
        totalSteps = System.Enum.GetValues(typeof(CPRState)).Length - 1;
        setState(CPRState.CheckSafety);
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
        // 각 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[CPRState.CheckSafety] = 10f;
        stageTimeLimit[CPRState.WearPPE] = 15f;
        stageTimeLimit[CPRState.CheckConsciousness] = 10f; 
        stageTimeLimit[CPRState.Call119AndRequestAED] = 15f;
        stageTimeLimit[CPRState.CheckBreathingAndPulse] = 10f;
        stageTimeLimit[CPRState.ChestCompressions] = 20f;
        stageTimeLimit[CPRState.OpenAirway] = 10f;
        stageTimeLimit[CPRState.ProvideRescueBreaths] = 10f;
        stageTimeLimit[CPRState.ContinueCPR] = 120f; // 5사이클 수행 시간
        stageTimeLimit[CPRState.DirectAssistants] = 10f;
        stageTimeLimit[CPRState.TurnOnAED] = 15f;
        stageTimeLimit[CPRState.AttachPads] = 20f;
        stageTimeLimit[CPRState.ClearArea] = 10f;
        stageTimeLimit[CPRState.DeliverShock] = 10f;
        stageTimeLimit[CPRState.ResumeChestCompressions] = 20f;
    }

    private IEnumerator InitSequence()
    {
        yield return new WaitUntil(() => TrainingEvaluator.Instance != null);
        TrainingEvaluator.Instance.OnServerResultReceived -= OnServerResultReceivedHandler;
        TrainingEvaluator.Instance.OnServerResultReceived += OnServerResultReceivedHandler;

        yield return new WaitForSeconds(5f);
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
            case CPRState.ChestCompressions:
                if (type == "압력 센서" && !pressurePassed)
                {
                    if(value < CPRValidator.minPressure){
                        AddError("심폐소생술 흉부 압박 약함");
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

            case CPRState.ProvideRescueBreaths:
                if (type == "유량 센서" && !flowPassed)
                {
                    if(value < 1000){
                        AddError("인공호흡 호흡량 약함");
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

            case CPRState.ContinueCPR:
                if (fiveCycleCount % 2 == 0 && type == "압력 센서" && !pressurePassed)
                {
                    
                    bool complete = cprValidator.TryAddCompression(value);
                   if(value < 1000){
                        AddError("심폐소생술 흉부 압박 약함");
                    }
                    if (complete)
                    {
                       
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();
                        cprValidator.Reset();
                        breathValidator.Reset();
                    }
                }

                if (fiveCycleCount % 2 == 1 && type == "유량 센서" && !flowPassed)
                {
              
                    bool success = breathValidator.TryAddBreath(value);

                    if (success)
                    {
                        Debug.Log("🌬 인공호흡 2회 성공!");
                        setFlowPassed();
                        cprValidator.Reset();
                        breathValidator.Reset();
                    }
                }
                break;

            case CPRState.ResumeChestCompressions:
                if (type == "압력 센서" && !pressurePassed)
                {
                    bool complete = cprValidator.TryAddCompression(value);
                  
                    if (complete)
                    {
                        Debug.Log("🫀 재압박 30회 성공!");
                        setPressurePassed();
                        cprValidator.Reset();
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
        case CPRState.CheckConsciousness:
            if (!gyroPassed && Mathf.Abs(pitch) > 50f)
            {
                Debug.Log("🌀 생존 확인 성공!");
                setGyroPassed();
            }
            break;

        case CPRState.OpenAirway:
            if (!gyroPassed && Mathf.Abs(pitch) > 30f)
            {
                Debug.Log("🌀 고개 기울이기 성공!");
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
        AddError($"{AdapterErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
        Debug.Log("🟡 음성 평가 : 오답");
    }
    else
    {
        Debug.Log("🔴 음성 평가 : 헛소리");
           
    }
}


    private IEnumerator Procedure()
    {
        
        while (currentState != CPRState.Completed)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case CPRState.CheckSafety:
                    
                    if (!voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.WearPPE);
                    break;

                case CPRState.WearPPE:
                   
                    wearPassed = true;
                    if (!wearPassed || !voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.CheckConsciousness);
                    break;

                case CPRState.CheckConsciousness:
                   
                    if (!gyroPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.Call119AndRequestAED);
                    break;

                case CPRState.Call119AndRequestAED:
                    // 이전 단계 로직에 통과 시 사람이 나오게 호출해야해요! (추후에)
                    if (!voicePassed) // 1. 음성인식 안되면 패스
                    {
                        break;
                    }
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.CheckBreathingAndPulse);
                    // 2. 다음단계에 필요한 손 인식 코드입니다.
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
                    break;

                case CPRState.CheckBreathingAndPulse:
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
                    setState(CPRState.ChestCompressions);
                    break; 

                case CPRState.ChestCompressions:
                 
                    if (!pressurePassed) break;
                    initPlag();
                    uiManager.ShowCheckIconPass(this);
                    setState(CPRState.OpenAirway);
                    break;

                case CPRState.OpenAirway:
                   
                    if (!gyroPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    uiManager.ShowBreathUI(false);
                    setState(CPRState.ProvideRescueBreaths);
                    break;

                case CPRState.ProvideRescueBreaths:
                    
                    if (!flowPassed) break;
                    initPlag();
                    uiManager.ShowCheckIconPass(this);
                    setState(CPRState.ContinueCPR);
                    break;

                case CPRState.ContinueCPR:
                   
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
                    setState(CPRState.DirectAssistants);
                    break;

                case CPRState.DirectAssistants:
                    uiManager.ShowCountText(false);
                    if (!voicePassed) break;
                    initPlag();
                    uiManager.ShowCheckIconPass(this);
                    setState(CPRState.TurnOnAED);
                    handValidator.BeginVerification(10, new Vector3(0.05f, 0f, 0.05f), 0.2f, 1f, setHandTrackingPassed);
                    break;

                case CPRState.TurnOnAED:
                    uiManager.ShowCountText(false);
                    if (!handTrackingPassed) break;
                    initPlag();
                    uiManager.ShowCheckIconPass(this);
                    setState(CPRState.AttachPads);
                    markerPositionValidator.BeginValidation(1, 11, new Vector3(0.1f, 0.1f, 0f), 0.1f, 1f, setMarkerPostionFristPassed);
                    markerPositionValidator.BeginValidation(1, 12, new Vector3(-0.1f, -0.1f, 0f), 0.1f, 1f, setMarkerPositionSecondPassed);
                    break;

                case CPRState.AttachPads:
                    if (!markerPositionFirstPassed || !markerPositionSecondPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.ClearArea);
                    break;

                case CPRState.ClearArea:
                    
                    if (!voicePassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.DeliverShock);
                    handValidator.BeginVerification(10, new Vector3(0.05f, 0f, 0.05f), 0.2f, 1f, setHandTrackingPassed);
                    break;

                case CPRState.DeliverShock:
                    if (!handTrackingPassed) break;
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(CPRState.ResumeChestCompressions);
                    break;

                case CPRState.ResumeChestCompressions:
                    
                    if (!pressurePassed) break;
                    uiManager.ShowCheckIconPass(this);  
                    initPlag();
                    setState(CPRState.Completed);
                    break;
            }

            yield return new WaitForSeconds(2f); // 반응성을 위해 더 짧은 주기로 체크
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
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "자동제세동기(AED) 사용법 단계 훈련", (result) => {
            feedback = result;
            Debug.Log($"📝 CPR 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }

    private void setState(CPRState nextState)
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
        StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "자동제세동기(AED) 사용법 단계 훈련", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 CPR 피드백:\n{feedback}");
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

        GameManager.Instance.sceneName = "AEDScene";
        GameManager.Instance.protocolName = "자동제세동기 사용";
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
        protocolName = "자동제세동기 사용",
        date = today,
        duration = durationString,
        score = score,
        feedback = feedback
    };

    GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
}




} // end
