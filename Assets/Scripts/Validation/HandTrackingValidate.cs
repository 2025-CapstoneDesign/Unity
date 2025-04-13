using System;
using System.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandTrackingValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    [SerializeField] private GameObject targetEffectPrefab;
    private GameObject activeEffect;
    private Renderer effectRenderer;

    private Vector3 targetLocalOffset;
    private Vector3 targetWorldPos;

    private float radius;
    private float requiredTime;
    private float currentTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;
    private Coroutine hideCoroutine;

    private int markerId;

    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

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

        // 오브젝트는 마커 인식되었을 때 Update()에서 생성하도록 함
    }

    public void StopVerification()
    {
        isActive = false;
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
    }

    void Update()
    {
        if (!isActive || IsVerified)
            return;

        // 마커 인식 여부 확인 후 effect 생성 시도
        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            // 마커 인식 안됐으면 다음 프레임에 재시도
            return;
        }

        // 마커 인식 성공 → 위치 계산
        Vector3 worldOffset = marker.rotation * targetLocalOffset;
        targetWorldPos = marker.position + worldOffset;

        // 오브젝트가 없으면 생성
        if (activeEffect == null && targetEffectPrefab != null)
        {
            activeEffect = Instantiate(targetEffectPrefab, targetWorldPos, Quaternion.identity);
            activeEffect.transform.localScale = Vector3.one * (radius * 0.5f);

            effectRenderer = activeEffect.GetComponentInChildren<Renderer>();
            if (effectRenderer != null)
            {
                effectRenderer.material = new Material(effectRenderer.material);
                effectRenderer.material.color = defaultColor;
            }
        }

        // 위치 갱신
        if (activeEffect != null)
        {
            activeEffect.transform.position = targetWorldPos;
            activeEffect.SetActive(true); // 혹시 비활성화돼 있었으면 활성화
        }

        // 손 위치 체크
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose))
        {
            Vector3 handPos = pose.Position;
            float dist = Vector3.Distance(handPos, targetWorldPos);

            if (dist <= radius)
            {
                currentTime += Time.deltaTime;

                if (effectRenderer != null)
                    effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                if (currentTime >= requiredTime)
                {
                    IsVerified = true;
                    isActive = false;
                    onVerifiedCallback?.Invoke();
                    HideEffectAfterDelay(0.5f);
                }
            }
            else
            {
                currentTime = 0f;

                if (effectRenderer != null)
                    effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, defaultColor, Time.deltaTime * 8f);
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
}
