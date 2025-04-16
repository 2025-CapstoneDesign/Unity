using UnityEngine;
using UnityEngine.UI;

public class GradationBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform indicator;
    public RectTransform gradientBar;
    public Image gradientImage;

    [Header("Force Settings")]
    public float minForce = CPRValidator.minPressure;
    public float maxForce = CPRValidator.maxPressure;

    [Header("Transparency Settings")]
    public float minAlpha = 0.3f;
    public float maxAlpha = 1.0f;

    [Header("Animation Settings")]
    public float smoothSpeed = 5f;

    private float targetForce = 0f;
    private float displayedForce = 0f;
    private float animationTimer = 0f;
    private float holdDuration = 0.3f; // 올라간 후 유지 시간
    private enum AnimationState { Idle, Rising, Holding, Falling }
    private AnimationState state = AnimationState.Idle;

    void Start()
    {
        // 시작 시 indicator를 바닥으로 초기화
        MoveIndicator(minForce);
    }

    void Update()
    {
        switch (state)
        {
            case AnimationState.Rising:
                displayedForce = Mathf.Lerp(displayedForce, targetForce, Time.deltaTime * smoothSpeed);
                UpdateIndicator(displayedForce);
                if (Mathf.Abs(displayedForce - targetForce) < 0.5f)
                {
                    state = AnimationState.Holding;
                    animationTimer = 0f;
                }
                break;

            case AnimationState.Holding:
                animationTimer += Time.deltaTime;
                if (animationTimer >= holdDuration)
                {
                    state = AnimationState.Falling;
                }
                break;

            case AnimationState.Falling:
                displayedForce = Mathf.Lerp(displayedForce, minForce, Time.deltaTime * smoothSpeed);
                UpdateIndicator(displayedForce);
                if (Mathf.Abs(displayedForce - minForce) < 0.5f)
                {
                    state = AnimationState.Idle;
                }
                break;
        }
    }

    public void SetForce(float force)
    {
        targetForce = Mathf.Clamp(force, minForce, maxForce);
        displayedForce = minForce; // 바닥에서 시작해서 올라가도록
        state = AnimationState.Rising;
        Debug.Log($"💥 압력 들어옴: {targetForce}");
    }

    private void MoveIndicator(float force)
    {
        float normalized = Mathf.InverseLerp(minForce, maxForce, force);
        float barHeight = gradientBar.rect.height;
        float yPos = Mathf.Lerp(-barHeight / 2f, barHeight / 2f, normalized);
        indicator.anchoredPosition = new Vector2(indicator.anchoredPosition.x, yPos);

        Color color = gradientImage.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, normalized);
        gradientImage.color = color;
    }

    private void UpdateIndicator(float force)
    {
        MoveIndicator(force);
    }
}
