using System;
using System.Collections;
using UnityEngine;

public class MoveValidation : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    [SerializeField] private GameObject targetEffectPrefab;
    private GameObject activeEffect;
    private Renderer effectRenderer;

    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private int markerId;
    private Vector3 startPos;
    private Vector3 expectedOffset;
    private float tolerance;
    private float requiredStayTime;
    private float currentStayTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;
    private Coroutine hideCoroutine;

    private Vector3 expectedWorldPos;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isInitialized = false;

    public void BeginValidation(int markerId, Vector3 targetOffset, float tolerance, float stayTime, Action onSuccess)
    {
        this.markerId = markerId;
        this.expectedOffset = targetOffset;
        this.tolerance = tolerance;
        this.requiredStayTime = stayTime;
        this.onVerifiedCallback = onSuccess;

        currentStayTime = 0f;
        IsVerified = false;
        isActive = true;
        isInitialized = false;

        // 기존 이펙트가 있다면 제거
        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
            effectRenderer = null;
        }

        // 마커는 이후 Update에서 찾음
        Debug.Log($"📌 이동 검증 시작: 마커 {markerId}, 목표 오프셋 {targetOffset:F3}, 오차 ±{tolerance:F3}, 유지시간 {stayTime}s");
    }

    public void StopValidation()
    {
        isActive = false;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }
    }

    void Update()
    {
        if (!isActive || IsVerified)
            return;

        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            // 마커가 인식되지 않았을 때 이펙트 숨기기
            if (activeEffect != null)
                activeEffect.SetActive(false);
            return;
        }

        if (startPos == Vector3.zero)
            startPos = marker.position;

        Vector3 newTargetPos = startPos + expectedOffset;

        if (!isInitialized)
        {
            if (activeEffect == null && targetEffectPrefab != null)
            {
                activeEffect = Instantiate(targetEffectPrefab, newTargetPos, Quaternion.identity);
                effectRenderer = activeEffect.GetComponentInChildren<Renderer>();
                if (effectRenderer != null)
                {
                    effectRenderer.material = new Material(effectRenderer.material);
                    effectRenderer.material.color = defaultColor;
                }
            }
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
                activeEffect.SetActive(true);
            }
            isInitialized = true;
            lastPosition = newTargetPos;
            return;
        }

        float positionChange = Vector3.Distance(lastPosition, newTargetPos);

        if (positionChange > 0.1f)
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
            }
        }
        else
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = Vector3.Lerp(activeEffect.transform.position, newTargetPos, Time.deltaTime * 10f);
            }
        }

        lastPosition = newTargetPos;

        Vector3 currentPos = marker.position;
        Vector3 moved = currentPos - startPos;
        float diff = Vector3.Distance(moved, expectedOffset);

        bool insideTargetRange = diff <= tolerance;

        if (insideTargetRange)
        {
            currentStayTime += Time.deltaTime;

            if (effectRenderer != null)
                effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, successColor, Time.deltaTime * 8f);

            if (currentStayTime >= requiredStayTime)
            {
                IsVerified = true;
                isActive = false;
                Debug.Log("✅ 이동 검증 성공!");
                onVerifiedCallback?.Invoke();
                HideEffectAfterDelay(0.5f);
            }
        }
        else
        {
            currentStayTime = 0f;

            if (effectRenderer != null)
                effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, defaultColor, Time.deltaTime * 8f);
        }
    }

    private void HideEffectAfterDelay(float delay)
    {
        if (activeEffect == null) return;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideEffectCoroutine(delay));
    }

    private IEnumerator HideEffectCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeEffect != null)
            activeEffect.SetActive(false);
    }

    public void ResetValidation()
    {
        IsVerified = false;
        isActive = false;
        currentStayTime = 0f;
        startPos = Vector3.zero;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }
    }
}
