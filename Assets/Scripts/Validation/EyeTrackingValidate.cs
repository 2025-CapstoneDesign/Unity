using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class EyeTrackingValidate : MonoBehaviour
{
    [Serializable]
    public class EyeValidation
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
        [NonSerialized] public bool isLooking = false;
    }

    [SerializeField] private GameObject targetEffectPrefab;
    private List<EyeValidation> validations = new List<EyeValidation>();
    
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = true;

    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
        // 동일한 마커 ID로 이미 검증이 진행 중인지 확인
        EyeValidation existingValidation = validations.Find(v => v.markerId == markerId);
        
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
        EyeValidation validation = new EyeValidation
        {
            markerId = markerId,
            targetLocalOffset = localOffset,
            radius = radius,
            requiredTime = holdTime,
            onVerifiedCallback = onSuccess,
            isVerified = false,
            currentTime = 0f,
            isInitialized = false,
            isLooking = false
        };
        
        validations.Add(validation);
        isActive = true;
    }

    public void StopVerification()
    {
        isActive = false;

        foreach (var validation in validations)
        {
            if (validation.effect != null)
            {
                validation.effect.SetActive(false);
                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = defaultColor;
            }

            validation.isLooking = false;
        }
    }

    public void ResetVerification()
    {
        foreach (var validation in validations)
        {
            validation.isVerified = false;
            validation.currentTime = 0f;
            validation.isLooking = false;

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
        if (!isActive || CoreServices.InputSystem?.EyeGazeProvider == null)
            return;

        for (int i = 0; i < validations.Count; i++)
        {
            var validation = validations[i];
            
            if (validation.isVerified)
                continue;

            if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(validation.markerId, out MarkerData marker))
            {
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                continue;
            }

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

            var gazeProvider = CoreServices.InputSystem.EyeGazeProvider;
            if (!gazeProvider.IsEyeTrackingEnabled)
                continue;

            Vector3 origin = gazeProvider.GazeOrigin;
            Vector3 direction = gazeProvider.GazeDirection;
            Ray gazeRay = new Ray(origin, direction);

            bool hitTarget = false;

            if (Physics.Raycast(gazeRay, out RaycastHit hit, 10f))
            {
                Vector3 gazePoint = hit.point;

                if (validation.effect != null)
                {
                    float sqrDist = (gazePoint - validation.effect.transform.position).sqrMagnitude;

                    if (sqrDist <= validation.radius * validation.radius)
                    {
                        hitTarget = true;
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
                }
            }

            if (validation.effectRenderer != null)
            {
                if (hitTarget && !validation.isLooking)
                {
                    validation.isLooking = true;
                }
                else if (!hitTarget && validation.isLooking)
                {
                    validation.effectRenderer.material.color = defaultColor;
                    validation.isLooking = false;
                    validation.currentTime = 0f;
                }
            }
        }
    }

    private void HideEffectAfterDelay(EyeValidation validation, float delay)
    {
        if (validation.effect == null) return;

        if (validation.hideCoroutine != null)
            StopCoroutine(validation.hideCoroutine);

        validation.hideCoroutine = StartCoroutine(HideEffectCoroutine(validation, delay));
    }

    private IEnumerator HideEffectCoroutine(EyeValidation validation, float delay)
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
