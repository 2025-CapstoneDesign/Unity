using UnityEngine;
using UnityEngine.UI;

public class CompressionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform indicator;
    public RectTransform gradientBar;
    public Image gradientImage;

    [Header("Force Settings")]
    [Range(0, 100)]
    public float currentForce = 0f;
    private float displayedForce = 0f;

    public float minForce = 40f;
    public float maxForce = 100f;

    [Header("Transparency Settings")]
    public float minAlpha = 0.3f;
    public float maxAlpha = 1.0f;

    [Header("Animation Settings")]
    public float smoothSpeed = 5f;

    private bool isAnimating = false;

    void Update()
    {
        if (!isAnimating) return;

        // 1. 보간하여 위로 올라오게 함
        displayedForce = Mathf.Lerp(displayedForce, currentForce, Time.deltaTime * smoothSpeed);

        // 2. 정규화
        float normalized = Mathf.InverseLerp(minForce, maxForce, displayedForce);

        // 3. 위치 이동
        float barHeight = gradientBar.rect.height;
        float newY = Mathf.Lerp(-barHeight / 2f, barHeight / 2f, normalized);
        indicator.anchoredPosition = new Vector2(indicator.anchoredPosition.x, newY);

        // 4. 투명도 조절
        Color color = gradientImage.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, normalized);
        gradientImage.color = color;

        // 5. 도달하면 애니메이션 종료
        if (Mathf.Abs(displayedForce - currentForce) < 0.1f)
        {
            isAnimating = false;
        }
    }

    public void SetForce(float value)
    {
        currentForce = Mathf.Clamp(value, minForce, maxForce);

        // 아래로 떨어뜨리고 애니메이션 시작
        displayedForce = minForce;     // 또는 더 낮은 0f로 완전 아래로 시작해도 됨
        isAnimating = true;

        Debug.Log($"SetForce 호출됨: {currentForce}");
    }
}
