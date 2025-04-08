using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AEDManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI aedMessageText;       // 단계 안내
    [SerializeField] private Slider progressBar;                   // 진행률 바
    [SerializeField] private Image fillMaskImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CountTextDisplay countTextDisplay;

    [SerializeField] private Image checkIcon; // 체크 아이콘 이미지
    

    // 테스트: 코드에서 직접 설정

    private Coroutine iconColorResetCoroutine;

private Color passColor = new Color(0f, 1f, 0f, 1f); // 초록
private Color failColor = new Color(1f, 0f, 0f, 1f); // 빨강
private Color idleColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 회색

   private bool timeOverNotified = false;
    private bool isFlashing = false;
    private float flashTimer = 0f;
    private float flashInterval = 0.5f; // 깜빡이는 속도 (초)
    private CPRState currentState;
    private bool externalInput = false;
    private bool waitingForInput = false;
    private int totalSteps;

    void Start()
    {
        currentState = CPRState.CheckSafety;
        totalSteps = System.Enum.GetValues(typeof(CPRState)).Length - 1; // Completed 제외

        // 시작 시 재시도 메시지 숨기기
        

        StartCoroutine(StartWithDelay());
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
        // ✅ 각 단계에 따라 UI 업데이트
        switch (currentState)
        {
            case CPRState.CheckSafety:
                
                break;

            case CPRState.WearPPE:
                // 예: 장갑 착용 애니메이션 등
                break;

            case CPRState.CheckConsciousness:
                // 예: 환자에게 말걸기 애니메이션
                break;

            case CPRState.Call119AndRequestAED:
                // 예: 전화기 오브젝트 활성화 등
                break;

            case CPRState.CheckBreathingAndPulse:
                // 예: 손 위치 안내 표시
                break;

            case CPRState.ChestCompressions:
                // ✅ 강도 UI를 활성화하거나 압박 깊이 측정 시작
            
                countTextDisplay.ShowCompressionCount(0);
                break;

            case CPRState.OpenAirway:
                // 예: 머리 젖히는 동작 안내
                break;

            case CPRState.ProvideRescueBreaths:
                // 예: 포켓마스크 애니메이션, 숨 불어넣기
                break;

            case CPRState.ContinueCPR:
                
                break;

            case CPRState.DirectAssistants:
                // 예: 손짓 애니메이션
                break;

            case CPRState.TurnOnAED:
                // 예: AED 오브젝트 버튼 활성화
                break;

            case CPRState.AttachPads:
                // 예: 패드 위치 가이드 표시
                break;

            case CPRState.ClearArea:
                // 예: 주위 사람들 물러서게 하는 알림
                break;

            case CPRState.DeliverShock:
                // 예: 버튼 누르면 이펙트 발생
                break;

            case CPRState.ResumeChestCompressions:
                // ✅ 다시 압박 유도 시작
                break;

            case CPRState.RecordDocuments:
                // 예: 기록지 작성 인터랙션
                break;
        }

        yield return StartCoroutine(WaitForInput());
    }

    aedMessageText.text = "🎉 훈련이 완료되었습니다!";
    UpdateProgressBar();
}


   private void UpdateStepUI()
{
    aedMessageText.text = AEDMessageManager.GetMessage(currentState);
    aedMessageText.color = Color.white;

    countTextDisplay.Hide();

    if (checkIcon != null)
        

    UpdateProgressBar();
}


    private void UpdateProgressBar()
    {
        float ratio = (float)(int)currentState / totalSteps;
        if (progressBar != null)
            progressBar.value = ratio;
    }

    private IEnumerator ResetCheckIconColorAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);

    if (checkIcon != null)
        checkIcon.color = idleColor;
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

    if (checkIcon != null)
    {
        // 색상 설정
        checkIcon.color = isPassed ? passColor : failColor;

        // 기존 코루틴 중지
        if (iconColorResetCoroutine != null)
            StopCoroutine(iconColorResetCoroutine);

        // 코루틴 실행: 색 유지 → 복귀 → 다음 단계 처리까지
        iconColorResetCoroutine = StartCoroutine(HandleCheckIconFeedback(isPassed, 3f));
    }

    externalInput = true;
    waitingForInput = false;
}

private IEnumerator HandleCheckIconFeedback(bool isPassed, float delay)
{
    yield return new WaitForSeconds(delay); // ✅ 색 보여주는 시간

    if (checkIcon != null)
        checkIcon.color = idleColor;

    if (isPassed)
    {
        currentState++;
        UpdateStepUI();
    }
    else
    {
        // 실패면 상태 유지, 메시지만 갱신하고 다시 시도
        UpdateStepUI();
    }
}




//외부에서 호출 시 
public void UpdateCompressionCount(int count)
{
    if (currentState == CPRState.ChestCompressions || currentState == CPRState.ResumeChestCompressions)
    {
        countTextDisplay.ShowCompressionCount(count);
    }
}

public void UpdateBreathCount(int count)
{
    countTextDisplay.ShowBreathCount(count);
}



}
