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
        [NonSerialized] public Vector3[] positionHistory = new Vector3[10];
        [NonSerialized] public Quaternion[] rotationHistory = new Quaternion[10];
        [NonSerialized] public int historyIndex = 0;
        [NonSerialized] public bool hasFullHistory = false;

        public void UpdateHistory(Vector3 newPosition, Quaternion newRotation)
        {
            positionHistory[historyIndex] = newPosition;
            rotationHistory[historyIndex] = newRotation;
            
            historyIndex = (historyIndex + 1) % positionHistory.Length;
            if (historyIndex == 0) hasFullHistory = true;
        }
        
        public Vector3 GetStablePosition()
        {
            if (!hasFullHistory) return lastPosition;
            
            Vector3 avgPosition = Vector3.zero;
            int count = hasFullHistory ? positionHistory.Length : historyIndex;
            
            for (int i = 0; i < count; i++)
                avgPosition += positionHistory[i];
                
            return avgPosition / count;
        }
        
        public Quaternion GetStableRotation()
        {
            if (!hasFullHistory) return lastRotation;
            
            Vector3 avgForward = Vector3.zero;
            Vector3 avgUp = Vector3.zero;
            int count = hasFullHistory ? rotationHistory.Length : historyIndex;
            
            for (int i = 0; i < count; i++)
            {
                avgForward += rotationHistory[i] * Vector3.forward;
                avgUp += rotationHistory[i] * Vector3.up;
            }
            
            avgForward /= count;
            avgUp /= count;
            
            return Quaternion.LookRotation(avgForward.normalized, avgUp.normalized);
        }
    }

    [SerializeField] private GameObject targetEffectPrefab;
    private List<HandValidation> validations = new List<HandValidation>();
    
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = true;

    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
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
                validation.UpdateHistory(newTargetPos, marker.rotation);
                continue;
            }

            // 쿼터니언 부호 검사 및 보정
            Quaternion targetRotation = marker.rotation;
            if (Quaternion.Dot(targetRotation, validation.lastRotation) < 0f)
            {
                targetRotation = new Quaternion(
                    -targetRotation.x,
                    -targetRotation.y,
                    -targetRotation.z,
                    -targetRotation.w
                );
            }

            // 안정화된 위치와 회전 계산
            Vector3 stablePosition = validation.GetStablePosition();
            Quaternion stableRotation = validation.GetStableRotation();

            // 급격한 변화 감지
            float positionChange = Vector3.Distance(validation.lastPosition, newTargetPos);
            float rotationChange = Quaternion.Angle(validation.lastRotation, targetRotation);

            if (validation.effect != null)
            {
                if (positionChange > 0.1f || rotationChange > 30f)
                {
                    // 급격한 변화 시 천천히 보간
                    validation.effect.transform.position = Vector3.Lerp(stablePosition, newTargetPos, Time.deltaTime * 3f);
                    validation.effect.transform.rotation = Quaternion.Lerp(stableRotation, targetRotation, Time.deltaTime * 3f);
                }
                else
                {
                    // 작은 변화는 더 빠르게 보간
                    validation.effect.transform.position = Vector3.Lerp(validation.effect.transform.position, newTargetPos, Time.deltaTime * 8f);
                    validation.effect.transform.rotation = Quaternion.Lerp(validation.effect.transform.rotation, targetRotation, Time.deltaTime * 8f);
                }
            }

            // 현재 상태 저장
            validation.lastPosition = validation.effect.transform.position;
            validation.lastRotation = validation.effect.transform.rotation;
            validation.UpdateHistory(validation.lastPosition, validation.lastRotation);

            bool rightHandValid = HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose rightPose);
            bool leftHandValid = HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Left, out MixedRealityPose leftPose);

            if (rightHandValid || leftHandValid)
            {
                Vector3 handPos;
                if (rightHandValid && leftHandValid)
                {
                    // 두 손이 모두 인식될 경우, 더 가까운 손의 위치를 사용
                    float rightDist = Vector3.Distance(rightPose.Position, validation.effect.transform.position);
                    float leftDist = Vector3.Distance(leftPose.Position, validation.effect.transform.position);
                    handPos = rightDist < leftDist ? rightPose.Position : leftPose.Position;
                }
                else
                {
                    // 한 손만 인식될 경우 해당 손의 위치를 사용
                    handPos = rightHandValid ? rightPose.Position : leftPose.Position;
                }

                float dist = Vector3.Distance(handPos, validation.effect.transform.position);

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