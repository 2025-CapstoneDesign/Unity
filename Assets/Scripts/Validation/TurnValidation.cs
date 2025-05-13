using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnValidation : MonoBehaviour
{
    [Serializable]
    public class TurnValidationData
    {
        public int markerId;
        public float targetAngle;    // 목표 회전 각도(도 단위)
        public float tolerance;      // 각도 오차 허용 범위(도 단위)
        public float requiredStayTime;
        public Action onVerifiedCallback;

        [NonSerialized] public float currentStayTime = 0f;
        [NonSerialized] public bool isVerified = false;
        [NonSerialized] public GameObject effect;
        [NonSerialized] public Renderer effectRenderer;
        [NonSerialized] public Quaternion startRotation;
        [NonSerialized] public bool isInitialized = false;
        [NonSerialized] public Coroutine hideCoroutine;
        [NonSerialized] public float currentRotationAmount = 0f;
        [NonSerialized] public bool hasStartRotation = false; // 초기 회전값 저장 여부를 확인하는 플래그 추가
    }
    
    [SerializeField] private GameObject targetEffectPrefab;
    private readonly List<TurnValidationData> validations = new List<TurnValidationData>();
    
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = true;

    // 간소화된 검증 시작 메서드 - 회전 축을 지정하지 않음
    public void BeginValidation(int markerId, float targetAngle, float tolerance, float stayTime, Action onSuccess)
    {
        // 동일한 마커 ID로 이미 검증이 진행 중인지 확인
        TurnValidationData existingValidation = validations.Find(v => v.markerId == markerId);
        
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
        TurnValidationData validation = new TurnValidationData
        {
            markerId = markerId,
            targetAngle = targetAngle,
            tolerance = tolerance,
            requiredStayTime = stayTime,
            onVerifiedCallback = onSuccess,
            isVerified = false,
            currentStayTime = 0f,
            isInitialized = false,
            startRotation = Quaternion.identity,
            currentRotationAmount = 0f,
            hasStartRotation = false
        };
        
        validations.Add(validation);
        isActive = true;
        
        Debug.Log($"📌 회전 검증 시작: 마커 {markerId}, 목표 회전 {targetAngle}도, 오차 ±{tolerance}도, 유지시간 {stayTime}s");
    }

    public void StopValidation()
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
        }
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

            if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(validation.markerId, out MarkerData marker))
            {
                // 마커가 인식되지 않았을 때 이펙트 숨기기
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                continue;
            }

            // Quaternion.identity 비교 대신 boolean 플래그 사용
            if (!validation.hasStartRotation)
            {
                validation.startRotation = marker.rotation;
                validation.hasStartRotation = true;
            }

            if (!validation.isInitialized)
            {
                if (validation.effect == null && targetEffectPrefab != null)
                {
                    validation.effect = Instantiate(targetEffectPrefab, marker.position, Quaternion.identity);
                    validation.effectRenderer = validation.effect.GetComponentInChildren<Renderer>();
                    if (validation.effectRenderer != null)
                    {
                        validation.effectRenderer.material = new Material(validation.effectRenderer.material);
                        validation.effectRenderer.material.color = defaultColor;
                    }
                }
                
                if (validation.effect != null)
                {
                    validation.effect.transform.position = marker.position;
                    validation.effect.SetActive(true);
                }
                
                validation.isInitialized = true;
                continue;
            }

            if (validation.effect != null)
            {
                validation.effect.transform.position = marker.position;
            }

            // 현재 회전과 시작 회전 사이의 각도 계산
            float rotationAngle = Quaternion.Angle(validation.startRotation, marker.rotation);
            
            // 누적 회전량을 업데이트 (점진적인 회전을 위해)
            validation.currentRotationAmount = rotationAngle;
            
            // 목표 회전각과의 차이 계산
            float angularDeviation = Mathf.Abs(validation.targetAngle - validation.currentRotationAmount);
            
            // 오차가 허용 범위 내인지 확인
            bool withinTolerance = angularDeviation <= validation.tolerance;

            if (withinTolerance)
            {
                validation.currentStayTime += Time.deltaTime;

                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = Color.Lerp(validation.effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                if (validation.currentStayTime >= validation.requiredStayTime)
                {
                    validation.isVerified = true;
                    Debug.Log($"✅ 회전 검증 성공! ({validation.currentRotationAmount:F1}도 회전)");
                    validation.onVerifiedCallback?.Invoke();
                    HideEffectAfterDelay(validation, 0.5f);
                }
            }
            else
            {
                validation.currentStayTime = 0f;

                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = Color.Lerp(validation.effectRenderer.material.color, defaultColor, Time.deltaTime * 8f);
            }
        }
    }

    private void HideEffectAfterDelay(TurnValidationData validation, float delay)
    {
        if (validation.effect == null) return;

        if (validation.hideCoroutine != null)
            StopCoroutine(validation.hideCoroutine);

        validation.hideCoroutine = StartCoroutine(HideEffectCoroutine(validation, delay));
    }

    private IEnumerator HideEffectCoroutine(TurnValidationData validation, float delay)
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

    public void ResetValidation()
    {
        foreach (var validation in validations)
        {
            validation.isVerified = false;
            validation.currentStayTime = 0f;
            validation.hasStartRotation = false; // 플래그도 초기화
            validation.startRotation = Quaternion.identity;
            validation.currentRotationAmount = 0f;

            if (validation.effect != null)
            {
                validation.effect.SetActive(false);
                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = defaultColor;
            }
        }
        
        isActive = false;
    }
}