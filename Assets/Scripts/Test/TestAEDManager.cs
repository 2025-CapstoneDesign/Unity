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
    [SerializeField] private CPRFeedbackGenerator feedbackGenerator;


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
      
    }

    private IEnumerator InitSequence()
    {
        yield return new WaitUntil(() => TrainingEvaluator.Instance != null);
        TrainingEvaluator.Instance.OnServerResultReceived -= OnServerResultReceivedHandler;
        TrainingEvaluator.Instance.OnServerResultReceived += OnServerResultReceivedHandler;

        yield return new WaitForSeconds(2f);
        timerManager.StartTimer(300f);
        StartCoroutine(CPRProcedure());
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
        uiManager.ShowCheckIconFail(this);
        Debug.Log("🟡 음성 평가 : 오답");
    }
    else
    {
        uiManager.ShowCheckIconFail(this);
        Debug.Log("🔴 음성 평가 : 헛소리");
    }
}


    private IEnumerator CPRProcedure()
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
                    initPlag();
                    setState(CPRState.CheckBreathingAndPulse);
                    // 2. 다음단계에 필요한 손 인식 코드입니다.
                    handValidator.BeginVerification(1, new Vector3(0f, 0.25f, 0.05f), 0.2f, 2f, setHandTrackingPassed);
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
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0f), 0.2f, 2f, setHandTrackingPassed);
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
                    initPlag();
                    setState(CPRState.DirectAssistants);
                    break;

                case CPRState.DirectAssistants:
                    uiManager.ShowCountText(false);
                    if (!voicePassed) break;
                    initPlag();
                    uiManager.ShowCheckIconPass(this);
                    setState(CPRState.TurnOnAED);
                    handValidator.BeginVerification(12, new Vector3(0f, 0.0f, 0f), 0.2f, 1f, setHandTrackingPassed);
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
                    initPlag();
                    setState(CPRState.ClearArea);
                    break;

                case CPRState.ClearArea:
                    
                    if (!voicePassed) break;
                    initPlag();
                    setState(CPRState.DeliverShock);
                    handValidator.BeginVerification(12, new Vector3(0f, 0f, 0f), 0.2f, 1f, setHandTrackingPassed);
                    break;

                case CPRState.DeliverShock:
                    if (!handTrackingPassed) break;
                    initPlag();
                    setState(CPRState.ResumeChestCompressions);
                    break;

                case CPRState.ResumeChestCompressions:
                    
                    if (!pressurePassed) break;
                    initPlag();
                    setState(CPRState.Completed);
                    break;
            }

            yield return new WaitForSeconds(3f);
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
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, (result) => {
            feedback = result;
            Debug.Log($"📝 CPR 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }

    private void setState(CPRState nextState)
    {
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
        StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, (result) =>
        {
            feedback = result;
            Debug.Log($"📝 CPR 피드백:\n{feedback}");
            // TODO: UI 업데이트 등 필요 시
        }));
    }

    private void AddError(string errorType)
    {
        if (checkScore.ContainsKey(errorType))
        {
            checkScore[errorType]++;
        }
        else
        {
            checkScore[errorType] = 1;
        }
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
