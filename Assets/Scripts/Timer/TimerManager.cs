using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("타이머 텍스트")]
    public TextMeshProUGUI timerText;

    [Header("타이머 바")]
    public RectTransform fillTransform;
    public Image fillImage;
    public RectTransform fillParent;

    [Header("타이머 설정")]
    public float totalTime = 10f;

    private float elapsedTime = 0f;
    private bool isRunning = false;

    public void StartTimer(float duration)
    {
        totalTime = duration;
        elapsedTime = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        float remainingTime = Mathf.Max(0f, totalTime - elapsedTime);

        if (timerText != null)
        {
            if (elapsedTime <= totalTime)
            {
                // 남은 시간 표시
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"{minutes}:{seconds:00}";
            }
            else
            {
                // 오버타임 표시
                float overtime = elapsedTime - totalTime;
                timerText.text = $"⏰ +{overtime:F1}초";
            }
        }

        // 프로그레스 바 비율 계산 (0~1)
        float ratio = Mathf.Clamp01(Mathf.Max(0f, totalTime - elapsedTime) / totalTime);

        if (fillTransform != null && fillParent != null)
        {
            float fullWidth = fillParent.rect.width;
            Vector2 size = fillTransform.sizeDelta;
            size.x = ratio * fullWidth;
            fillTransform.sizeDelta = size;
        }

        if (fillImage != null)
        {
            Color brightOrange = new Color32(255, 169, 77, 255);
            Color midOrange    = new Color32(255, 127, 80, 255);
            Color redOrange    = new Color32(255, 77, 77, 255);

            Color blendColor = ratio > 0.5f
                ? Color.Lerp(brightOrange, midOrange, (1f - ratio) * 2f)
                : Color.Lerp(midOrange, redOrange, (0.5f - ratio) * 2f);

            fillImage.color = blendColor;
        }
    }

    public bool IsTimeUp() => elapsedTime >= totalTime;

    public float GetRemainingTime() => Mathf.Max(0f, totalTime - elapsedTime);

    public float GetOverTime() => Mathf.Max(0f, elapsedTime - totalTime);

    public float GetElapsedTime() => elapsedTime;
}
