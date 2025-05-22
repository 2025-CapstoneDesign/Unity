using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class SpinialMotionRestrictionManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;
    [SerializeField] private SpinialMotionRestrictionUIManager uiManager;
    [SerializeField] private GPTFeedbackGenerator feedbackGenerator;

    private SpinalMotionRestrictionState currentState;
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

    public EyeTrackingValidate eyeTrackingValidator;
    public HandTrackingValidate handValidator;
    public MarkerPositionValidate markerPositionValidator;
    public MarkerDistanceValidate markerDistanceValidator;
    public MoveValidation moveValidator;

    private int score = 100;
    private Dictionary<string, int> checkScore = new Dictionary<string, int>();
    String feedback = "wait";

    // 각 단계별 제한 시간 (초)
    private Dictionary<SpinalMotionRestrictionState, float> stageTimeLimit = new Dictionary<SpinalMotionRestrictionState, float>();

    // 현재 단계 시작 시간
    private float currentStageStartTime = 0f;

    // 점수 차감 관련 설정
    private const int TIME_PENALTY_PER_SECOND = 1;  // 초과 시간당 차감할 점수
    
    private bool hasPlayedVoice = false;

    void Start()
    {
        currentState = SpinalMotionRestrictionState.EnsureSceneSafety;
        totalSteps = System.Enum.GetValues(typeof(SpinalMotionRestrictionState)).Length - 1;
        setState(SpinalMotionRestrictionState.EnsureSceneSafety);
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
        // 척추고정 관련 단계별 제한시간 설정 (초 단위)
        stageTimeLimit[SpinalMotionRestrictionState.EnsureSceneSafety] = 10f;
        stageTimeLimit[SpinalMotionRestrictionState.WearPPE] = 15f;
        stageTimeLimit[SpinalMotionRestrictionState.PerformLogRoll] = 30f;
        stageTimeLimit[SpinalMotionRestrictionState.PositionPatientOnSpineBoard] = 20f;
        stageTimeLimit[SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody] = 25f;
        stageTimeLimit[SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard] = 30f;
        stageTimeLimit[SpinalMotionRestrictionState.ApplyHeadImmobilizer] = 20f;
        stageTimeLimit[SpinalMotionRestrictionState.SecureHands] = 15f;
        stageTimeLimit[SpinalMotionRestrictionState.AssessPMSOfExtremities] = 20f;
        stageTimeLimit[SpinalMotionRestrictionState.RecordOnMedicalChart] = 30f;
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
            case SpinalMotionRestrictionState.PerformLogRoll:
                if (!gyroPassed && Mathf.Abs(roll) > 50f)
                {
                    Debug.Log("🌀 로그롤 움직임 감지 성공!");
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
            AddError($"{SpinalMotionRestrictionToErrorType.GetLabel(currentState)} 단계 음성 오답", 1);
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
        while (currentState != SpinalMotionRestrictionState.RecordOnMedicalChart)
        {
            Debug.Log($"🧭 현재 단계: {currentState}");
            uiManager.SetMessage(currentState);
            uiManager.SetProgress(currentState, totalSteps);

            switch (currentState)
            {
                case SpinalMotionRestrictionState.EnsureSceneSafety:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.WearPPE);
                    break;

                case SpinalMotionRestrictionState.WearPPE:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.PerformLogRoll);
                    break;
                
                case SpinalMotionRestrictionState.PerformLogRoll:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!gyroPassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.PositionPatientOnSpineBoard);
                    break;
                
                case SpinalMotionRestrictionState.PositionPatientOnSpineBoard:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody);
                    break;
                
                case SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard);
                    break;
                
                case SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.ApplyHeadImmobilizer);
                    break;
                
                case SpinalMotionRestrictionState.ApplyHeadImmobilizer:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.SecureHands);
                    break;
                
                case SpinalMotionRestrictionState.SecureHands:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.AssessPMSOfExtremities);
                    break;
                
                case SpinalMotionRestrictionState.AssessPMSOfExtremities:
                    if (!hasPlayedVoice)
                    {
                        PlayVoiceForStage(currentState);
                        hasPlayedVoice = true;
                    }
                    if (!voicePassed) break;
                    
                    uiManager.ShowCheckIconPass(this);
                    initPlag();
                    setState(SpinalMotionRestrictionState.RecordOnMedicalChart);
                    break;
                
                case SpinalMotionRestrictionState.RecordOnMedicalChart:
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
        yield return StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "척추고정 훈련 평가 단계",(result) =>
        {
            feedback = result;
            Debug.Log($"📝 척추고정 평가 훈련 요약:\n{feedback}");
            // TODO: UI에 피드백 표시 로직 추가
        }));
    }
    
    private void PlayVoiceForStage(SpinalMotionRestrictionState state)
    {
        int stageNumber = (int)state;
        string path = $"SceneStage/Spinal/Spinal{stageNumber + 1}";

        if (AudioManager.Instance != null)
        {
            Debug.Log($"🔊 척추고정 단계 {stageNumber} 오디오 재생: {path}");
            AudioManager.Instance.PlayVoice(path);
        }
        else
        {
            Debug.LogWarning("❗ AudioManager.Instance is NULL! 음성을 재생할 수 없습니다.");
        }
    }

    private void setState(SpinalMotionRestrictionState nextState)
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
        // VoiceSender에 현재 상태 전달
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
        StartCoroutine(feedbackGenerator.GenerateFeedback(checkScore, "척추고정 훈련 평가 단계", (result) =>
        {
            feedback = result;
            Debug.Log($"📝 척추고정 평가 피드백:\n{feedback}");
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

        GameManager.Instance.protocolName = "척추고정";
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
            protocolName = "척추고정",
            date = today,
            duration = durationString,
            score = score,
            feedback = feedback
        };

        GetComponent<ResultHistoryManager>().SaveNewResult(newResult);
    }
}
