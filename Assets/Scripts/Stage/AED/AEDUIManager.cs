using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AEDUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI aedMessageText;
    [SerializeField] private TextMeshProUGUI countTextDisplay;
    [SerializeField] private Image checkIcon;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image fillMaskImage;
    [SerializeField] private GradationBarUI compressionUI;
    [SerializeField] private GradationBarUI breathUI;

    private Coroutine iconResetCoroutine;
    private float flashTimer;
    private readonly float flashInterval = 0.5f;
    private bool isFlashing = false;

    private readonly Color passColor = new(0f, 1f, 0f, 1f);
    private readonly Color idleColor = new(0.5f, 0.5f, 0.5f, 1f);
    private readonly Color failColor = new(1f, 0f, 0f, 1f);

    public void InitializeUI()
    {
        ShowCompressionUI(false);
        ShowBreathUI(false);
        ShowCountText(false);
        if (checkIcon != null) checkIcon.color = idleColor;
        flashTimer = 0f;
        countTextDisplay.text = string.Empty;
    }

    public void UpdateTimerUI(TimerManager timerManager, CPRState currentState)
    {
        if (timerManager.IsTimeUp() && currentState != CPRState.Completed)
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

    private void FlashFillMask()
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
        iconResetCoroutine = context.StartCoroutine(ResetCheckIcon(2f));
    }

    public void ShowCheckIconFail(MonoBehaviour context)
    {
        checkIcon.color = failColor;
        if (iconResetCoroutine != null) context.StopCoroutine(iconResetCoroutine);
        iconResetCoroutine = context.StartCoroutine(ResetCheckIcon(2f));
    }

    private IEnumerator ResetCheckIcon(float delay)
    {
        yield return new WaitForSeconds(delay);
        checkIcon.color = idleColor;
    }

    public void SetMessage(CPRState state)
    {
        aedMessageText.text = AdapterMessageManager.GetMessage(state);
        aedMessageText.color = Color.white;
    }

    public void SetMessage(object state)
    {
        aedMessageText.text = AdapterMessageManager.GetMessage(state);
        aedMessageText.color = Color.white;
    }

    public void SetProgress(CPRState state, int totalSteps)
    {
        progressBar.value = (float)(int)state / totalSteps;
    }

    public void ShowCompleteMessage()
    {
        aedMessageText.text = "🎉 훈련이 완료되었습니다!";
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
} // end
