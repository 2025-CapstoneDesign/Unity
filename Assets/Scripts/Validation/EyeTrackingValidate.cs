using System;
using System.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class EyeTrackingValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    [SerializeField] private GameObject targetEffectPrefab;
    private GameObject activeEffect;
    private Renderer effectRenderer;

    private Color defaultColor = Color.red;
    private Color successColor = Color.green;

    private float radius;
    private float requiredTime;
    private float currentTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;
    private Coroutine hideCoroutine;
    private bool isLooking = false;

    private int markerId;
    private Vector3 targetLocalOffset;
    private Vector3 targetWorldPos;

    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
        this.markerId = markerId;
        this.targetLocalOffset = localOffset;
        this.radius = radius;
        this.requiredTime = holdTime;
        this.onVerifiedCallback = onSuccess;

        IsVerified = false;
        currentTime = 0f;
        isActive = true;

        // 🎯 마커가 없으면 나중에 Update에서 처리
    }

    public void StopVerification()
    {
        isActive = false;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }

        isLooking = false;
    }

    void Update()
    {
        if (!isActive || IsVerified || CoreServices.InputSystem?.EyeGazeProvider == null)
            return;

        // ✅ 마커가 인식되었는지 확인
        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
            return;

        Vector3 worldOffset = marker.rotation * targetLocalOffset;
        targetWorldPos = marker.position + worldOffset;

        // ✅ 오브젝트 지연 생성
        if (activeEffect == null && targetEffectPrefab != null)
        {
            activeEffect = Instantiate(targetEffectPrefab, targetWorldPos, marker.rotation);

            effectRenderer = activeEffect.GetComponentInChildren<Renderer>();
            if (effectRenderer != null)
            {
                effectRenderer.material = new Material(effectRenderer.material);
                effectRenderer.material.color = defaultColor;
            }
        }

        // ✅ 오브젝트 위치 갱신
        if (activeEffect != null)
        {
            activeEffect.transform.position = targetWorldPos;
            activeEffect.transform.rotation = marker.rotation;
            activeEffect.SetActive(true);
        }

        var gazeProvider = CoreServices.InputSystem.EyeGazeProvider;
        if (!gazeProvider.IsEyeTrackingEnabled)
            return;

        Vector3 origin = gazeProvider.GazeOrigin;
        Vector3 direction = gazeProvider.GazeDirection;
        Ray gazeRay = new Ray(origin, direction);

        bool hitTarget = false;

        if (Physics.Raycast(gazeRay, out RaycastHit hit, 10f))
        {
            Vector3 gazePoint = hit.point;

            if (activeEffect != null)
            {
                float sqrDist = (gazePoint - activeEffect.transform.position).sqrMagnitude;

                if (sqrDist <= radius * radius)
                {
                    hitTarget = true;
                    currentTime += Time.deltaTime;

                    if (effectRenderer != null)
                        effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                    if (currentTime >= requiredTime)
                    {
                        IsVerified = true;
                        isActive = false;
                        HideEffectAfterDelay(0.5f);
                        onVerifiedCallback?.Invoke();
                    }
                }
            }
        }

        // 🔁 시선 벗어났을 때 초기화
        if (effectRenderer != null)
        {
            if (hitTarget && !isLooking)
            {
                isLooking = true;
            }
            else if (!hitTarget && isLooking)
            {
                effectRenderer.material.color = defaultColor;
                isLooking = false;
                currentTime = 0f;
            }
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

    public void ResetVerification()
    {
        IsVerified = false;
        isActive = false;
        currentTime = 0f;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }

        isLooking = false;
    }
}
