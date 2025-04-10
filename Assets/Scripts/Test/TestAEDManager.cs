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
    private bool isFlashing = false;
    private float flashTimer = 0f;
    private float flashInterval = 0.5f; // 깜빡이는 속도 (초)
    private CPRState currentState;
    private bool externalInput = false;
    private bool waitingForInput = false;
    private int totalSteps;

    // 검증 플래그
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
        Debug.Log("음성 인식 결과를 수신하였습니다.");
        
        if (score >= 2)
        {
            voicePassed = true;
            Debug.Log("🟢 음성 평가 통과 (이벤트)");
        }
        else
        {
            Debug.Log("🔴 음성 평가 실패 (이벤트)");
        }
        Debug.Log($"📊 현재 단계: {currentState}, voicePassed: {voicePassed}");
    }
    void Start()
    {
        Debug.Log("🟡 TestAEDManager.Start 실행됨");

        currentState = CPRState.CheckSafety;
        totalSteps = System.Enum.GetValues(typeof(CPRState)).Length - 1;

        var cg = retryMessageText.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0f;

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

        Debug.Log("🟢 TrainingEvaluator.Instance 확인됨, 이벤트 구독 시작");
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
            UpdateStepUI();

            switch (currentState)
            {
                case CPRState.CheckSafety:
                    if (voicePassed)
                    {
                        initPlag();
                        currentState = CPRState.WearPPE;
                    }
                    break;
                case CPRState.WearPPE:
                    bool wearPassed = true; // 보호장구착용추가 착용 감지
                    if (wearPassed && voicePassed)
                    {
                        initPlag();
                        currentState = CPRState.CheckConsciousness;
                    }
                    break;
                case CPRState.CheckConsciousness:
                    if (gyroPassed)
                    {
                        initPlag();
                        currentState = CPRState.Call119AndRequestAED;
                    }
                    break;
                case CPRState.Call119AndRequestAED:
                    // 이전 단계 통과 시 사람이 나와야해요!
                    if (voicePassed)
                    {
                        initPlag();
                        currentState = CPRState.CheckBreathingAndPulse;
                        handValidator.BeginVerification(1, new Vector3(0f, 0.32f, 0f), 0.2f, 2f, setHandTrackingPassed);
                    }
                    break;

                case CPRState.CheckBreathingAndPulse:
                    if (voicePassed && handTrackingPassed)
                    {
                        initPlag();
                        currentState = CPRState.ChestCompressions;
                    }
                    break;
                case CPRState.ChestCompressions:
                    if (pressurePassed)
                    {
                        initPlag();
                        currentState = CPRState.OpenAirway;
                    }
                    break;
                case CPRState.OpenAirway:
                    if (gyroPassed)
                    {
                        initPlag();
                        currentState = CPRState.ProvideRescueBreaths;
                    }
                    break;

                case CPRState.ProvideRescueBreaths:
                    if (flowPassed)
                    {
                        initPlag();
                        currentState = CPRState.ContinueCPR;
                    }
                    break;

                case CPRState.ContinueCPR:
                    if (pressurePassed && flowPassed) // 5주기 변경
                    {
                        initPlag();
                        currentState = CPRState.DirectAssistants;
                    }
                    break;

                case CPRState.DirectAssistants:
                    if (voicePassed)
                    {
                        initPlag();
                        currentState = CPRState.TurnOnAED;
                        handValidator.BeginVerification(10, new Vector3(0f, 0.0f, 0f), 0.2f, 1f, setHandTrackingPassed);
                    }
                    break;

                case CPRState.TurnOnAED:
                    if (handTrackingPassed)
                    {
                        initPlag();
                        currentState = CPRState.AttachPads;
                        markerPositionValidator.BeginValidation(
                            1,
                            11,
                            new Vector3(-0.1f, 0.1f, 0f),
                            0.1f,
                            1f, // ✅ 1초 이상 위치 유지해야 통과
                            setMarkerPostionFristPassed
                        );
                        markerPositionValidator.BeginValidation(
                            2,
                            11,
                            new Vector3(0.1f, -0.1f, 0f),
                            0.1f,
                            1f, // ✅ 1초 이상 위치 유지해야 통과
                            setMarkerPositionSecondPassed
                        );
                    }
                    break;

                case CPRState.AttachPads:
                    if (markerPositionFirstPassed && markerPositionSecondPassed)
                    {
                        initPlag();
                        currentState = CPRState.ClearArea;
                    }
                    break;

                case CPRState.ClearArea:
                    if (voicePassed)
                    {
                        initPlag();
                        currentState = CPRState.DeliverShock;
                        handValidator.BeginVerification(10, new Vector3(0f, 0f, 0f), 0.2f, 1f, setHandTrackingPassed);
                    }
                    break;

                case CPRState.DeliverShock:
                    if (handTrackingPassed)
                    {
                        initPlag();
                        currentState = CPRState.ResumeChestCompressions;
                    }
                    break;

                case CPRState.ResumeChestCompressions:
                    if (pressurePassed)
                    {
                        initPlag();
                        currentState = CPRState.Completed;
                    }
                    break;
            }

            yield return new WaitForSeconds(1f); // 1초 기다림
        }

        aedMessageText.text = "🎉 훈련이 완료되었습니다!";
        UpdateProgressBar();
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

    private IEnumerator WaitForInput()
    {
        externalInput = false;
        waitingForInput = true;

        while (!externalInput)
        {
            yield return null;
        }
    }

    public void ReceiveInputResult(bool isPassed)
    {
        if (!waitingForInput || externalInput) return;

        if (isPassed)
        {
            Debug.Log($"✅ {currentState} 단계 통과!");
            StartCoroutine(ShowFeedbackMessage("✅ 통과했습니다!", Color.green, true));
        }
        else
        {
            Debug.Log($"❌ {currentState} 단계 실패!");
            StartCoroutine(ShowFeedbackMessage("❌ 다시 시도해주세요!", Color.red, false));
        }

        externalInput = true;
        waitingForInput = false;
    }


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
        markerPositionSecondPassed = false;
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
    }

    void OnDisable()
    {
        SensorEvents.OnSensorDataReceived -= HandleSensorData;
    }

    private void HandleSensorData(string type, float value)
    {
        // 단계별로 센서 타입과 조건에 따라 처리
        switch (currentState)
        {
            case CPRState.CheckConsciousness:
                if (type == "자이로 센서" && Mathf.Abs(value) > 30f && !gyroPassed)
                {
                    Debug.Log("🌀 고개 기울이기 성공!");
                    setGyroPassed();
                }
                break;

            case CPRState.ChestCompressions:
                if (type == "압력 센서" && !pressurePassed)
                {
                    bool complete = cprValidator.TryAddCompression(value);
                    if (complete)
                    {
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();         // 플래그 처리
                        cprValidator.Reset();        // 리셋
                    }
                }
                break;

            case CPRState.OpenAirway:
                if (type == "자이로 센서" && Mathf.Abs(value) > 30f && !gyroPassed)
                {
                    Debug.Log("🌀 고개 기울이기 성공!");
                    setGyroPassed();
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
                if (type == "압력 센서" && !pressurePassed)
                {
                    bool complete = cprValidator.TryAddCompression(value);
                    if (complete)
                    {
                        Debug.Log("🫀 CPR 30회 성공!");
                        setPressurePassed();         // 플래그 처리
                        cprValidator.Reset();        // 리셋
                    }
                }
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

}
