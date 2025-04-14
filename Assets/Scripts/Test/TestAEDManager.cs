using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestAEDManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI aedMessageText;       // 단계 안내
    [SerializeField] private TextMeshProUGUI retryMessageText;     // 통과/실패 메시지
    [SerializeField] private Slider progressBar;                   // 진행률 바
    [SerializeField] private Image fillMaskImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool timeOverNotified = false;
    private float flashTimer = 0f;
    private float flashInterval = 0.5f; // 깜빡이는 속도 (초)
    private CPRState currentState;
    private bool externalInput = false;
    private int totalSteps;

    // 검증 플래그
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

    private int fiveCycleCount = 0; // 5주기 카운트

    public HandTrackingValidate handValidator;
    public MarkerPositionValidate markerPositionValidator;

    private CPRValidator cprValidator = new();
    private BreathValidator breathValidator = new();

    void OnDestroy()
    {
        // 구독 해제(메모리 누수 방지)
        if (TrainingEvaluator.Instance != null)
            TrainingEvaluator.Instance.OnServerResultReceived -= OnServerResultReceivedHandler;
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
            Debug.Log("🟡 음성 평가 : 오답");
        }
        else if (score == 0)
        {
            Debug.Log("🔴 음성 평가 : 헛소리");
        }
    }
    void Start()
    {
        currentState = CPRState.CheckSafety;
        totalSteps = System.Enum.GetValues(typeof(CPRState)).Length - 1;

        var cg = retryMessageText.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;
        setState(CPRState.CheckSafety);
        ResetValidationFlags(); // 초기화
        StartCoroutine(InitSequence());
    }

    private IEnumerator InitSequence()
    {
        yield return StartCoroutine(WaitForEvaluatorAndSubscribe());
        yield return StartCoroutine(StartWithDelay()); // 이 순서 보장됨
    }

    private IEnumerator WaitForEvaluatorAndSubscribe()
    {
        yield return new WaitUntil(() => TrainingEvaluator.Instance != null);

        // 중복 방지: 기존에 구독돼 있으면 먼저 제거
        TrainingEvaluator.Instance.OnServerResultReceived -= OnServerResultReceivedHandler;
        TrainingEvaluator.Instance.OnServerResultReceived += OnServerResultReceivedHandler;
    }

    void Update()
    {
        if (timerManager.IsTimeUp() && currentState != CPRState.Completed)
        {
            float overtime = timerManager.GetOverTime();

            // ✅ FillMask 색상을 빨간색으로 바꿈 (한 번만)
            if (!timeOverNotified && fillMaskImage != null)
            {
                fillMaskImage.color = Color.red;
                timeOverNotified = true;
            }

            // ✅ 초과 시간 표시
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(overtime / 60f);
                int seconds = Mathf.FloorToInt(overtime % 60f);
                timerText.text = $"+ {minutes}:{seconds:00}";
            }

            // ✅ 초과 시간이 10초 넘으면 FillMask 깜빡임
            if (overtime >= 10f && fillMaskImage != null)
            {
                FlashFillMask();
            }
        }
    }
    private void FlashFillMask()
    {
        flashTimer += Time.deltaTime;

        if (flashTimer >= flashInterval)
        {
            // 색이 빨강 ↔ 투명 반복
            if (fillMaskImage.color.a > 0.9f)
            {
                fillMaskImage.color = new Color(1f, 0f, 0f, 0f); // 완전 투명
            }
            else
            {
                fillMaskImage.color = new Color(1f, 0f, 0f, 1f); // 불투명 빨강
            }

            flashTimer = 0f;
        }
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(2f);
        timerManager.StartTimer(300f);
        StartCoroutine(CPRProcedure());
    }

    private IEnumerator CPRProcedure()
    {
        while (currentState != CPRState.Completed)
        {
            Debug.Log($"🧭 현재 단계: {currentState}"); // ✅ 추가: 현재 진행 단계 로그
            UpdateStepUI();

            switch (currentState)
            {
                case CPRState.CheckSafety:
                    if (!voicePassed)
                    { // 1. 음성통과인식 안되면 패스
                        break;
                    }
                    initPlag();
                    setState(CPRState.WearPPE);
                    break;

                case CPRState.WearPPE:
                    // 1. 보호장비 착용 감지 먼저 (외부에서 wearPassed = true로 설정된 경우)
                    wearPassed = true;
                    if (!wearPassed)
                    {
                        voicePassed = false; // 이전 음성 평가 초기화
                        Debug.Log("🧤 보호장비 착용 대기 중...");
                        break;
                    }

                    // 2. 착용 감지 후, 음성 평가 대기
                    if (!voicePassed)
                    {
                        Debug.Log("🎤 음성 평가 대기 중...");
                        break;
                    }

                    // 3. 둘 다 통과 시 다음 단계
                    Debug.Log("✅ 보호장비 착용 + 음성 평가 완료");
                    initPlag();
                    setState(CPRState.CheckConsciousness);
                    break;

                case CPRState.CheckConsciousness:
                    if (!gyroPassed) // 1. 자이로 센서 통과 안되면 패스
                    {
                        break;
                    }
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
                    handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0f), 0.2f, 2f, setHandTrackingPassed);
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
                    if (!voicePassed)
                    {
                        Debug.Log("🎤 손 인식 완료됨! 음성 평가 대기 중...");
                        break;
                    }

                    // 3. 둘 다 통과 시 다음 단계로 이동
                    Debug.Log("✅ 손 위치 + 음성 평가 완료");
                    initPlag();
                    setState(CPRState.ChestCompressions);
                    break;

                case CPRState.ChestCompressions:
                    if (!pressurePassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.OpenAirway);
                    break;

                case CPRState.OpenAirway:
                    if (!gyroPassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.ProvideRescueBreaths);
                    break;

                case CPRState.ProvideRescueBreaths:
                    if (!flowPassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.ContinueCPR);
                    break;

                // fiveCycleCount % 2 == 0 : 심폐소생술
                // fiveCycleCount % 2 == 1 : 인공호흡
                case CPRState.ContinueCPR:
                    if (fiveCycleCount < 10)
                    {
                        if (fiveCycleCount % 2 == 0 && pressurePassed)
                        {
                            Debug.Log($"🫀 심폐소생술 {fiveCycleCount / 2 + 1}주기 - 압박 완료"); // ✅ 추가
                            fiveCycleCount++;
                            flowPassed = false;
                            break;
                        }
                        if (fiveCycleCount % 2 == 1 && flowPassed)
                        {
                            Debug.Log($"🌬 심폐소생술 {fiveCycleCount / 2 + 1}주기 - 인공호흡 완료"); // ✅ 추가
                            fiveCycleCount++;
                            pressurePassed = false;
                            break;
                        }
                        break;
                    }
                    initPlag();
                    setState(CPRState.DirectAssistants);
                    break;

                case CPRState.DirectAssistants:
                    if (!voicePassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.TurnOnAED);
                    handValidator.BeginVerification(12, new Vector3(0f, 0.0f, 0f), 0.2f, 1f, setHandTrackingPassed);
                    break;

                case CPRState.TurnOnAED:
                    if (!handTrackingPassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.AttachPads);
                    markerPositionValidator.BeginValidation(
                        1,
                        11,
                        new Vector3(0.1f, 0.1f, 0f),
                        0.1f,
                        1f, // ✅ 1초 이상 위치 유지해야 통과
                        setMarkerPostionFristPassed
                    );
                    markerPositionValidator.BeginValidation(
                        1,
                        12,
                        new Vector3(-0.1f, -0.1f, 0f),
                        0.1f,
                        1f, // ✅ 1초 이상 위치 유지해야 통과
                        setMarkerPositionSecondPassed
                    );
                    break;

                case CPRState.AttachPads:
                    if (!markerPositionFirstPassed || !markerPositionSecondPassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.ClearArea);
                    break;

                case CPRState.ClearArea:
                    if (!voicePassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.DeliverShock);
                    handValidator.BeginVerification(12, new Vector3(0f, 0f, 0f), 0.2f, 1f, setHandTrackingPassed);
                    break;

                case CPRState.DeliverShock:
                    if (!handTrackingPassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.ResumeChestCompressions);
                    break;

                case CPRState.ResumeChestCompressions:
                    if (!pressurePassed)
                    {
                        break;
                    }
                    initPlag();
                    setState(CPRState.Completed);
                    break;
            }

            yield return new WaitForSeconds(0.5f); // 0.5초 기다림
        }

        aedMessageText.text = "🎉 훈련이 완료되었습니다!";
        UpdateProgressBar();
    }

    private void setState(CPRState nextState)
    {
        currentState = nextState;
        VoiceSender.Instance.CurrentStageTag = nextState.ToVoiceTag();
        Debug.Log($"➡️ 상태 전환: {currentState}");
    }

       // roll, pitch : 자이로 센서 각도 값
    // 이 메서드는 SensorEvents.OnGyroDataReceived 이벤트에 의해 호출됨
    private void HandleGyroData(float roll, float pitch)
    {
        Debug.Log($"📐 자이로 수신 - Roll: {roll}, Pitch: {pitch}");

        // 단계별로 자이로 센서 조건에 따라 처리
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

    // type : 센서 타입 (예: "압력 센서", "유량 센서")
    // value : 센서 값 (예: 압력 값, 유량 값)
    // 이 메서드는 SensorEvents.OnSensorDataReceived 이벤트에 의해 호출됨
    private void HandleSensorData(string type, float value)
    {
        // 단계별로 센서 타입과 조건에 따라 처리
        switch (currentState)
        {
            // cprValidator.compressionTimestamps.Count : 심폐소생술 횟수 접근
            // value : 압력 값 (예: 압력 센서 값)
            // cprValidator 내부에 속도에 따른 분기 로직이 존재함 필요 시 사용 가능
            case CPRState.ChestCompressions:
                if (type == "압력 센서" && !pressurePassed)
                {
                    bool complete = cprValidator.TryAddCompression(value);
                    Debug.Log($"압력 센서 : {cprValidator.compressionTimestamps.Count} 횟수 입니다.");
                    if (complete)
                    {
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();         // 플래그 처리
                        cprValidator.Reset();        // 리셋
                    }
                }
                break;

            // breathValidator.breathValidator : 심폐소생술 횟수 접근
            // value : 유량 값 (예: 유량 센서 값)
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
                        setPressurePassed();         // 플래그 처리
                        cprValidator.Reset();        // 리셋
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
                        cprValidator.Reset();        // 리셋
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

    private void UpdateStepUI()
    {
        aedMessageText.text = AEDMessageManager.GetMessage(currentState);
        aedMessageText.color = Color.white;
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        float ratio = (float)(int)currentState / totalSteps;
        if (progressBar != null)
            progressBar.value = ratio;
    }

    // public void ReceiveInputResult(bool isPassed)
    // {
    //     if (!waitingForInput || externalInput) return;

    //     if (isPassed)
    //     {
    //         Debug.Log($"✅ {currentState} 단계 통과!");
    //         StartCoroutine(ShowFeedbackMessage("✅ 통과했습니다!", Color.green, true));
    //     }
    //     else
    //     {
    //         Debug.Log($"❌ {currentState} 단계 실패!");
    //         StartCoroutine(ShowFeedbackMessage("❌ 다시 시도해주세요!", Color.red, false));
    //     }

    //     externalInput = true;
    //     waitingForInput = false;
    // }


    private IEnumerator ShowFeedbackMessage(string message, Color color, bool advanceStep)
    {
        if (retryMessageText == null) yield break;

        retryMessageText.text = message;
        retryMessageText.color = color;

        CanvasGroup cg = retryMessageText.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        // Fade In
        float t = 0f;
        while (t < 0.3f)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        // Fade Out
        t = 0f;
        while (t < 0.3f)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;

        // ✅ 피드백 메시지가 완전히 사라진 후에 다음 단계로 이동
        if (advanceStep)
        {
            currentState++;
            UpdateStepUI(); // 다음 단계 안내문구 갱신
        }
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

}
