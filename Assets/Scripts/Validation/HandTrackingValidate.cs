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
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isInitialized = false;

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
        isInitialized = false;

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
            effectRenderer = null;
        }
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

        // 디버깅: 마커 인식 상태 확인
        bool isMarkerDetected = OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker);
        
        if (!isMarkerDetected)
        {
            if (activeEffect != null)
                activeEffect.SetActive(false);
            return;
        }

        // 디버깅: 타겟 위치 정보
        Vector3 worldOffset = marker.rotation * targetLocalOffset;
        Vector3 newTargetPos = marker.position + worldOffset;
        Debug.Log($"타겟 위치: {newTargetPos}, 마커 위치: {marker.position}, 오프셋: {worldOffset}");

        if (!isInitialized)
        {
            Debug.Log($"isInitialized: {isInitialized}, targetEffectPrefab: {(targetEffectPrefab != null ? "있음" : "없음")}");
            
            if (activeEffect == null && targetEffectPrefab != null)
            {
                activeEffect = Instantiate(targetEffectPrefab, newTargetPos, marker.rotation);
                Debug.Log($"효과 생성됨: {activeEffect != null}, 위치: {newTargetPos}");
                
                effectRenderer = activeEffect.GetComponentInChildren<Renderer>();
                if (effectRenderer != null)
                {
                    effectRenderer.material = new Material(effectRenderer.material);
                    effectRenderer.material.color = defaultColor;
                    Debug.Log("렌더러 및 재질 설정 완료");
                }
                else
                {
                    Debug.LogError("렌더러를 찾을 수 없습니다!");
                }
            }
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
                activeEffect.transform.rotation = marker.rotation;
                activeEffect.SetActive(true);
            }
            isInitialized = true;
            lastPosition = newTargetPos;
            lastRotation = marker.rotation;
            return;
        }

        float positionChange = Vector3.Distance(lastPosition, newTargetPos);
        float rotationChange = Quaternion.Angle(lastRotation, marker.rotation);

        if (positionChange > 0.1f || rotationChange > 30f)
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
                activeEffect.transform.rotation = marker.rotation;
            }
        }
        else
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = Vector3.Lerp(activeEffect.transform.position, newTargetPos, Time.deltaTime * 10f);
                activeEffect.transform.rotation = Quaternion.Slerp(activeEffect.transform.rotation, marker.rotation, Time.deltaTime * 10f);
            }
        }

        lastPosition = newTargetPos;
        lastRotation = marker.rotation;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose))
        {
            Vector3 handPos = pose.Position;
            float dist = Vector3.Distance(handPos, newTargetPos);

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
