using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BaseUIManager : MonoBehaviour, IUIManager
{
    [Header("공통 UI 요소")]
    [SerializeField] protected TextMeshProUGUI messageText;
    [SerializeField] protected TextMeshProUGUI countTextDisplay;
    [SerializeField] protected Image checkIcon;
    [SerializeField] protected Slider progressBar;
    [SerializeField] protected Image fillMaskImage;
    [SerializeField] protected GradationBarUI compressionUI;
    [SerializeField] protected GradationBarUI breathUI;
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private AudioSource alertAudio;

    private Coroutine alertCoroutine;

    protected Coroutine iconResetCoroutine;
    protected float flashTimer;
    protected readonly float flashInterval = 0.5f;
    protected bool isFlashing = false;

    protected readonly Color passColor = new(0f, 1f, 0f, 1f);
    protected readonly Color idleColor = new(0.5f, 0.5f, 0.5f, 1f);
    protected readonly Color failColor = new(1f, 0f, 0f, 1f);

    public virtual void InitializeUI()
    {
        ShowCompressionUI(false);
        ShowBreathUI(false);
        ShowCountText(false);
        if (checkIcon != null) checkIcon.color = idleColor;
        flashTimer = 0f;
        countTextDisplay.text = string.Empty;
    }

    protected void FlashFillMask()
    {
        flashTimer += Time.deltaTime;
        if (flashTimer >= flashInterval)
        {
            if (fillMaskImage.color.a > 0.9f)
                fillMaskImage.color = new Color(1f, 0f, 0f, 0f);
            else
                fillMaskImage.color = new Color(1f, 0f, 0f, 1f);

            flashTimer = 0f;
        }
    }

    public void ShowAlert(string message, float duration)
    {
        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);

        alertCoroutine = StartCoroutine(ShowAlertCoroutine(message, duration));
    }

    private IEnumerator ShowAlertCoroutine(string message, float duration)
    {
        alertText.text = message;
        alertText.gameObject.SetActive(true);

        if (alertAudio != null)
            alertAudio.Play();

        // 🔥 핵심: alertText가 있는 Canvas를 정면 위치로 이동시켜야 함
        Transform cam = Camera.main.transform;
        Vector3 targetPos = cam.position + cam.forward * 0.5f;

        // 🟡 alertText가 직접 붙은 Canvas의 Transform을 이동해야 해!
        Transform canvasTransform = alertText.transform.parent;
        canvasTransform.position = targetPos;
        canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - cam.position);

        yield return new WaitForSeconds(duration);

        alertText.gameObject.SetActive(false);
        alertCoroutine = null;
    }


    public void ShowCompressionUI(bool visible) => compressionUI.gameObject.SetActive(visible);
    public void ShowBreathUI(bool visible) => breathUI.gameObject.SetActive(visible);
    public void SetCompressionForce(float value) => compressionUI.SetForce(value);
    public void SetBreathForce(float value) => breathUI.SetForce(value);
    public void ShowCountText(bool visible) => countTextDisplay.gameObject.SetActive(visible);
    public void UpdateCountText(string text) => countTextDisplay.text = text;

    public void ShowCheckIconPass(MonoBehaviour context)
    {
        checkIcon.color = passColor;
        if (iconResetCoroutine != null) context.StopCoroutine(iconResetCoroutine);
        iconResetCoroutine = context.StartCoroutine(ResetCheckIcon(1f));
    }

    public void ShowCheckIconFail(MonoBehaviour context)
    {
        checkIcon.color = failColor;
        if (iconResetCoroutine != null) context.StopCoroutine(iconResetCoroutine);
        iconResetCoroutine = context.StartCoroutine(ResetCheckIcon(1f));
    }

    protected IEnumerator ResetCheckIcon(float delay)
    {
        yield return new WaitForSeconds(delay);
        checkIcon.color = idleColor;
    }

    public abstract void SetMessage(object state);

    public void ShowCompleteMessage()
    {
        messageText.text = "🎉 훈련이 완료되었습니다!";
    }

    public IEnumerator HideCompressionUIWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ShowCompressionUI(false);
    }

    public IEnumerator HideBreathUIWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ShowBreathUI(false);
        ShowCountText(false);
    }

    // 진행 상태 업데이트를 위한 공통 메서드 추가
    public void SetProgress(float value)
    {
        progressBar.value = value;
    }

    // 타이머 UI 업데이트를 위한 일반화된 메서드
    protected void UpdateTimerUICommon(TimerManager timerManager, bool isCompleted)
    {
        if (timerManager.IsTimeUp() && !isCompleted)
        {
            float overtime = timerManager.GetOverTime();

            if (fillMaskImage != null)
            {
                fillMaskImage.color = Color.red;
                if (overtime >= 10f) FlashFillMask();
            }

            if (timerManager != null && timerManager.timerText != null)
            {
                int minutes = Mathf.FloorToInt(overtime / 60f);
                int seconds = Mathf.FloorToInt(overtime % 60f);
                timerManager.timerText.text = $"+ {minutes}:{seconds:00}";
            }
        }
    }

    public void StartHideCompressionUICoroutine(float seconds)
    {
        StartCoroutine(HideCompressionUIWithDelay(seconds));
    }

    public void StartHideBreathUICoroutine(float seconds)
    {
        StartCoroutine(HideBreathUIWithDelay(seconds));
    }
    public void SwitchToCompressionUI()
    {
        StartCoroutine(SwitchToCompressionUICoroutine());
    }

    public void SwitchToBreathUI()
    {
        StartCoroutine(SwitchToBreathUICoroutine());
    }

    private IEnumerator SwitchToCompressionUICoroutine()
    {
        ShowBreathUI(false); // 먼저 숨기고
        yield return new WaitForSeconds(0.2f); // 약간의 대기 시간
        ShowCompressionUI(true); // 그다음 보이게
    }

    private IEnumerator SwitchToBreathUICoroutine()
    {
        ShowCompressionUI(false); // 먼저 숨기고
        yield return new WaitForSeconds(0.2f); // 약간의 대기 시간
        ShowBreathUI(true); // 그다음 보이게
    }
}