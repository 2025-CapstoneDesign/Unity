using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandTrackingValidate : MonoBehaviour
{
    [Serializable]
    public class HandValidation
    {
        public int markerId;
        public Vector3 targetLocalOffset;
        public float radius;
        public float requiredTime;
        public Action onVerifiedCallback;

        [NonSerialized] public float currentTime = 0f;
        [NonSerialized] public bool isVerified = false;
        [NonSerialized] public GameObject effect;
        [NonSerialized] public Renderer effectRenderer;
        [NonSerialized] public Vector3 lastPosition;
        [NonSerialized] public Quaternion lastRotation;
        [NonSerialized] public bool isInitialized = false;
        [NonSerialized] public Coroutine hideCoroutine;
    }

    [SerializeField] private GameObject targetEffectPrefab;
    private List<HandValidation> validations = new List<HandValidation>();
    
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = true;

    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
        // 동일한 마커 ID로 이미 검증이 진행 중인지 확인
        HandValidation existingValidation = validations.Find(v => v.markerId == markerId);
        
        // 이미 존재하는 검증이 있다면 정리
        if (existingValidation != null)
        {
            // 코루틴이 실행 중이라면 중지
            if (existingValidation.hideCoroutine != null)
            {
                StopCoroutine(existingValidation.hideCoroutine);
                existingValidation.hideCoroutine = null;
            }
            
            // 이펙트가 있다면 제거
            if (existingValidation.effect != null)
            {
                Destroy(existingValidation.effect);
                existingValidation.effect = null;
                existingValidation.effectRenderer = null;
            }
            
            // 리스트에서 제거
            validations.Remove(existingValidation);
        }
        
        // 새 검증 생성
        HandValidation validation = new HandValidation
        {
            markerId = markerId,
            targetLocalOffset = localOffset,
            radius = radius,
            requiredTime = holdTime,
            onVerifiedCallback = onSuccess,
            isVerified = false,
            currentTime = 0f,
            isInitialized = false
        };
        
        validations.Add(validation);
        isActive = true;
    }

    public void StopVerification()
    {
        isActive = false;
    }

    public void ResetVerification()
    {
        foreach (var validation in validations)
        {
            validation.isVerified = false;
            validation.currentTime = 0f;

            if (validation.effect != null)
            {
                validation.effect.SetActive(false);
                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = defaultColor;
            }
        }
        isActive = false;
    }

    void Update()
    {
        if (!isActive)
            return;

        for (int i = 0; i < validations.Count; i++)
        {
            var validation = validations[i];
            
            if (validation.isVerified)
                continue;

            // 마커 인식 상태 확인
            bool isMarkerDetected = OptimizedArUcoMarkerDetection.markerMap.TryGetValue(validation.markerId, out MarkerData marker);
            
            if (!isMarkerDetected)
            {
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                continue;
            }

            // 타겟 위치 계산
            Vector3 worldOffset = marker.rotation * validation.targetLocalOffset;
            Vector3 newTargetPos = marker.position + worldOffset;

            if (!validation.isInitialized)
            {
                if (validation.effect == null && targetEffectPrefab != null)
                {
                    validation.effect = Instantiate(targetEffectPrefab, newTargetPos, marker.rotation);
                    
                    validation.effectRenderer = validation.effect.GetComponentInChildren<Renderer>();
                    if (validation.effectRenderer != null)
                    {
                        validation.effectRenderer.material = new Material(validation.effectRenderer.material);
                        validation.effectRenderer.material.color = defaultColor;
                    }
                }
                
                if (validation.effect != null)
                {
                    validation.effect.transform.position = newTargetPos;
                    validation.effect.transform.rotation = marker.rotation;
                    validation.effect.SetActive(true);
                }
                
                validation.isInitialized = true;
                validation.lastPosition = newTargetPos;
                validation.lastRotation = marker.rotation;
                continue;
            }

            float positionChange = Vector3.Distance(validation.lastPosition, newTargetPos);
            float rotationChange = Quaternion.Angle(validation.lastRotation, marker.rotation);

            if (positionChange > 0.1f || rotationChange > 30f)
            {
                if (validation.effect != null)
                {
                    validation.effect.transform.position = newTargetPos;
                    validation.effect.transform.rotation = marker.rotation;
                }
            }
            else
            {
                if (validation.effect != null)
                {
                    validation.effect.transform.position = Vector3.Lerp(validation.effect.transform.position, newTargetPos, Time.deltaTime * 10f);
                    validation.effect.transform.rotation = Quaternion.Slerp(validation.effect.transform.rotation, marker.rotation, Time.deltaTime * 10f);
                }
            }

            validation.lastPosition = newTargetPos;
            validation.lastRotation = marker.rotation;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose))
            {
                Vector3 handPos = pose.Position;
                float dist = Vector3.Distance(handPos, newTargetPos);

                if (dist <= validation.radius)
                {
                    validation.currentTime += Time.deltaTime;

                    if (validation.effectRenderer != null)
                        validation.effectRenderer.material.color = Color.Lerp(validation.effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                    if (validation.currentTime >= validation.requiredTime)
                    {
                        validation.isVerified = true;
                        HideEffectAfterDelay(validation, 0.5f);
                        validation.onVerifiedCallback?.Invoke();
                    }
                }
                else
                {
                    validation.currentTime = 0f;

                    if (validation.effectRenderer != null)
                        validation.effectRenderer.material.color = Color.Lerp(validation.effectRenderer.material.color, defaultColor, Time.deltaTime * 8f);
                }
            }
        }
    }

    private void HideEffectAfterDelay(HandValidation validation, float delay)
    {
        if (validation.effect == null) return;

        if (validation.hideCoroutine != null)
            StopCoroutine(validation.hideCoroutine);

        validation.hideCoroutine = StartCoroutine(HideEffectCoroutine(validation, delay));
    }

    private IEnumerator HideEffectCoroutine(HandValidation validation, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (validation.effect != null)
        {
            Destroy(validation.effect);
            validation.effect = null;
            validation.effectRenderer = null;
        }
        validation.hideCoroutine = null;
    }
}
