using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("타이머 텍스트")]
    public TextMeshProUGUI timerText;

    [Header("타이머 바")]
    public RectTransform fillParent;     // 전체 바 영역
    public RectTransform fillTransform;  // Fill Mask → Fill Image
    public Image fillImage;

    [Header("타이머 설정")]
    public float totalTime = 10f;
    [HideInInspector] public float elapsedTime = 0f;

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
        // 남은 시간 및 텍스트
        float remaining = Mathf.Max(0f, totalTime - elapsedTime);
        if (timerText != null)
        {
            if (elapsedTime <= totalTime)
            {
                int m = Mathf.FloorToInt(remaining / 60f);
                int s = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"{m}:{s:00}";
            }
            else
            {
                float over = elapsedTime - totalTime;
                timerText.text = $"⏰ +{over:F1}초";
            }
        }

        // 비율 계산
        float ratio = Mathf.Clamp01((totalTime - elapsedTime) / totalTime);

        // Width만 조절 (Mask + RectTransform)
        if (fillTransform != null && fillParent != null)
        {
            float fullW = fillParent.rect.width;
            float newW = ratio * fullW;
            fillTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                newW
            );
        }

        // 색상 그라데이션
        if (fillImage != null)
        {
            Color c1 = new Color32(255,169,77,255);
            Color c2 = new Color32(255,127,80,255);
            Color c3 = new Color32(255,77,77,255);
            Color blend = ratio > 0.5f
                ? Color.Lerp(c1, c2, (1f - ratio) * 2f)
                : Color.Lerp(c2, c3, (0.5f - ratio) * 2f);
            fillImage.color = blend;
        }
    }

    public bool IsTimeUp()      => elapsedTime >= totalTime;
    public float GetRemaining() => Mathf.Max(0f, totalTime - elapsedTime);
    public float GetOverTime()  => Mathf.Max(0f, elapsedTime - totalTime);
}
