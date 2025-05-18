using System.Collections;
using System.Collections.Generic;
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

    private Coroutine iconResetCoroutine;
    private float flashTimer;
    private readonly float flashInterval = 0.5f;
    private bool isFlashing = false;

    private readonly Color passColor = new(0f, 1f, 0f, 1f);
    private readonly Color idleColor = new(0.5f, 0.5f, 0.5f, 1f);
    private readonly Color failColor = new(1f, 0f, 0f, 1f);

    // 🔔 알림 시스템 관련
    private Queue<(string message, float duration)> alertQueue = new();
    private bool isAlertShowing = false;

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

    // ✅ 큐 기반 알림 표시 함수
    public void ShowAlert(string message, float duration)
    {
        alertQueue.Enqueue((message, duration));
        if (!isAlertShowing)
        {
            StartCoroutine(ProcessAlertQueue());
        }
    }

    private IEnumerator ProcessAlertQueue()
    {
        isAlertShowing = true;

        while (alertQueue.Count > 0)
        {
            var (message, duration) = alertQueue.Dequeue();

            // 텍스트 설정
            alertText.text = message;

            // Canvas 가져오기
            Transform canvasTransform = alertText.transform.parent;
            canvasTransform.gameObject.SetActive(true); // Canvas 전체 켜기

            // 정면 위치
            Transform cam = Camera.main.transform;
            Vector3 targetPos = cam.position + cam.forward * 0.5f;
            canvasTransform.position = targetPos;
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - cam.position);

            // 사운드 재생
            if (alertAudio != null)
                alertAudio.Play();

            yield return new WaitForSeconds(duration);

            canvasTransform.gameObject.SetActive(false);
        }

        isAlertShowing = false;
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

    public void SetProgress(float value)
    {
        progressBar.value = value;
    }

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
        ShowBreathUI(false);
        yield return new WaitForSeconds(0.2f);
        ShowCompressionUI(true);
    }

    private IEnumerator SwitchToBreathUICoroutine()
    {
        ShowCompressionUI(false);
        yield return new WaitForSeconds(0.2f);
        ShowBreathUI(true);
    }
}
