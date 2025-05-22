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
        [NonSerialized] public bool isInitialized = false;
        [NonSerialized] public Coroutine hideCoroutine;
        [NonSerialized] public bool isLooking = false;
        [NonSerialized] public Queue<Vector3> positionHistory = new Queue<Vector3>();
        [NonSerialized] public Queue<Quaternion> rotationHistory = new Queue<Quaternion>();
        [NonSerialized] public int historyLength = 20; // 위치 평균을 계산할 프레임 수를 20으로 증가
        [NonSerialized] public float maxPositionDelta = 0.1f; // 허용되는 최대 위치 변화량
        [NonSerialized] public float maxRotationDelta = 30f; // 최대 회전 변화량 (도)
        [NonSerialized] public int requiredInitialSamples = 5; // 초기 안정성 확인에 필요한 샘플 수
        [NonSerialized] public float initialStabilityThreshold = 0.03f; // 초기 위치의 안정성 판단 임계값
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
            isLooking = false,
            positionHistory = new Queue<Vector3>() // 히스토리 큐 초기화
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
                validation.positionHistory.Clear();
                validation.rotationHistory.Clear();
                continue;
            }

            // 현재 마커 데이터를 히스토리에 추가
            validation.positionHistory.Enqueue(marker.position);
            validation.rotationHistory.Enqueue(marker.rotation);
            
            // 초기화 전이고 충분한 샘플이 모였다면 안정성 체크
            if (!validation.isInitialized && validation.positionHistory.Count >= validation.requiredInitialSamples)
            {
                // 평균 위치 계산
                Vector3 meanPos = Vector3.zero;
                foreach (Vector3 pos in validation.positionHistory)
                {
                    meanPos += pos;
                }
                meanPos /= validation.positionHistory.Count;

                // 평균 회전 계산
                Vector4 meanRotation = Vector4.zero;
                foreach (Quaternion rot in validation.rotationHistory)
                {
                    meanRotation += new Vector4(rot.x, rot.y, rot.z, rot.w);
                }
                meanRotation /= validation.rotationHistory.Count;
                Quaternion averageRotation = new Quaternion(meanRotation.x, meanRotation.y, meanRotation.z, meanRotation.w).normalized;

                // 위치 분산 계산
                float positionVariance = 0f;
                foreach (Vector3 pos in validation.positionHistory)
                {
                    positionVariance += Vector3.Distance(pos, meanPos);
                }
                positionVariance /= validation.positionHistory.Count;

                // 회전 분산 계산 (각도 차이의 평균)
                float rotationVariance = 0f;
                foreach (Quaternion rot in validation.rotationHistory)
                {
                    rotationVariance += Quaternion.Angle(rot, averageRotation);
                }
                rotationVariance /= validation.rotationHistory.Count;

                // 분산이 임계값보다 크면 안정적이지 않다고 판단
                if (positionVariance > validation.initialStabilityThreshold || rotationVariance > validation.maxRotationDelta)
                {
                    validation.positionHistory.Dequeue();
                    validation.rotationHistory.Dequeue();
                    continue;
                }
            }

            // 히스토리 크기 관리
            while (validation.positionHistory.Count > validation.historyLength)
            {
                validation.positionHistory.Dequeue();
                validation.rotationHistory.Dequeue();
            }

            // 평균 위치 계산
            Vector3 smoothedMarkerPosition = Vector3.zero;
            foreach (Vector3 pos in validation.positionHistory)
            {
                smoothedMarkerPosition += pos;
            }
            smoothedMarkerPosition /= validation.positionHistory.Count;

            // 평균 회전 계산
            Vector4 smoothedRotation = Vector4.zero;
            foreach (Quaternion rot in validation.rotationHistory)
            {
                smoothedRotation += new Vector4(rot.x, rot.y, rot.z, rot.w);
            }
            smoothedRotation.Normalize();
            Quaternion smoothedMarkerRotation = new Quaternion(smoothedRotation.x, smoothedRotation.y, smoothedRotation.z, smoothedRotation.w);

            // 이미 초기화된 경우 현재 값이 평균에서 크게 벗어나는지 체크
            if (validation.isInitialized)
            {
                float positionChange = Vector3.Distance(marker.position, smoothedMarkerPosition);
                float rotationChange = Quaternion.Angle(marker.rotation, smoothedMarkerRotation);

                if (positionChange > validation.maxPositionDelta || rotationChange > validation.maxRotationDelta)
                {
                    continue;
                }
            }

            Vector3 worldOffset = smoothedMarkerRotation * validation.targetLocalOffset;
            Vector3 newTargetPos = smoothedMarkerPosition + worldOffset;

            if (!validation.isInitialized)
            {
                // 충분한 샘플이 모이고 안정성이 확인된 경우에만 이펙트 초기화
                if (validation.positionHistory.Count >= validation.requiredInitialSamples)
                {
                    if (validation.effect == null && targetEffectPrefab != null)
                    {
                        validation.effect = Instantiate(targetEffectPrefab, newTargetPos, smoothedMarkerRotation);
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
                        validation.effect.transform.rotation = smoothedMarkerRotation;
                        validation.effect.SetActive(true);
                    }
                    
                    validation.isInitialized = true;
                }
                continue;
            }

            // 부드러운 이동 및 회전
            if (validation.effect != null)
            {
                validation.effect.transform.position = Vector3.Lerp(validation.effect.transform.position, newTargetPos, Time.deltaTime * 5f);
                validation.effect.transform.rotation = Quaternion.Slerp(validation.effect.transform.rotation, smoothedMarkerRotation, Time.deltaTime * 5f);
                validation.effect.SetActive(true);
            }

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
