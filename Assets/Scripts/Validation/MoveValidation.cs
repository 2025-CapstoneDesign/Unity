using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveValidation : MonoBehaviour
{
    [Serializable]
    public class MoveValidationData
    {
        public int markerId;
        public Vector3 expectedOffset;
        public float tolerance;
        public float requiredStayTime;
        public Action onVerifiedCallback;

        [NonSerialized] public float currentStayTime = 0f;
        [NonSerialized] public bool isVerified = false;
        [NonSerialized] public GameObject effect;
        [NonSerialized] public Renderer effectRenderer;
        [NonSerialized] public Vector3 startPos;
        [NonSerialized] public Vector3 lastPosition;
        [NonSerialized] public bool isInitialized = false;
        [NonSerialized] public Coroutine hideCoroutine;
    }
    
    [SerializeField] private GameObject targetEffectPrefab;
    private readonly List<MoveValidationData> validations = new List<MoveValidationData>();
    
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = true;

    public void BeginValidation(int markerId, Vector3 targetOffset, float tolerance, float stayTime, Action onSuccess)
    {
        // 동일한 마커 ID로 이미 검증이 진행 중인지 확인
        MoveValidationData existingValidation = validations.Find(v => v.markerId == markerId);
        
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
        MoveValidationData validation = new MoveValidationData
        {
            markerId = markerId,
            expectedOffset = targetOffset,
            tolerance = tolerance,
            requiredStayTime = stayTime,
            onVerifiedCallback = onSuccess,
            isVerified = false,
            currentStayTime = 0f,
            // isInitialized and startPos will be set below or in Update
            isInitialized = false, 
            startPos = Vector3.zero 
        };
        
        // Try to set initial position and effect if marker is already visible
        if (OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData currentMarkerData))
        {
            validation.startPos = currentMarkerData.position;
            validation.isInitialized = true;
            Vector3 targetPos = validation.startPos + validation.expectedOffset;
            validation.lastPosition = targetPos; // Though lastPosition is currently unused

            if (targetEffectPrefab != null)
            {
                validation.effect = Instantiate(targetEffectPrefab, targetPos, Quaternion.identity);
                validation.effectRenderer = validation.effect.GetComponentInChildren<Renderer>();
                if (validation.effectRenderer != null)
                {
                    validation.effectRenderer.material = new Material(validation.effectRenderer.material);
                    validation.effectRenderer.material.color = defaultColor;
                }
                validation.effect.SetActive(true);
            }
            Debug.Log($"🎯 마커 {validation.markerId}의 초기 위치 즉시 설정(BeginValidation): {validation.startPos:F3}, 목표 위치: {targetPos:F3}");
        }
        else
        {
            // If marker not visible, initialization will be deferred to Update loop
            Debug.LogWarning($"마커 {markerId}가 BeginValidation 시점에 보이지 않아 초기 위치 설정이 Update에서 지연됩니다.");
        }

        validations.Add(validation);
        isActive = true;
        
        Debug.Log($"📌 이동 검증 시작: 마커 {markerId}, 목표 오프셋 {targetOffset:F3}, 오차 ±{tolerance:F3}, 유지시간 {stayTime}s");
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

            // 마커가 인식될 때만 처리
            if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(validation.markerId, out MarkerData marker))
            {
                // 마커가 인식되지 않았을 때 이펙트 숨기기
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                // If marker was initialized but now lost, DO NOT reset currentStayTime or color
                // if (validation.isInitialized) 
                // {
                //     validation.currentStayTime = 0f;
                //     if (validation.effectRenderer != null)
                //         validation.effectRenderer.material.color = defaultColor; // Reset color if it was changing
                // }
                continue;
            }
            else
            {
                // 마커가 다시 인식되었을 때 이펙트 다시 표시
                if (validation.effect != null && !validation.effect.activeSelf)
                    validation.effect.SetActive(true);
            }

            // 이펙트와 시작 위치 초기화 (최초 한 번만, if not done in BeginValidation)
            if (!validation.isInitialized)
            {
                validation.startPos = marker.position;
                Vector3 targetPos = validation.startPos + validation.expectedOffset;
                validation.lastPosition = targetPos; // Though lastPosition is currently unused
                
                if (validation.effect == null && targetEffectPrefab != null)
                {
                    validation.effect = Instantiate(targetEffectPrefab, targetPos, Quaternion.identity);
                    validation.effectRenderer = validation.effect.GetComponentInChildren<Renderer>();
                    if (validation.effectRenderer != null)
                    {
                        validation.effectRenderer.material = new Material(validation.effectRenderer.material);
                        validation.effectRenderer.material.color = defaultColor;
                    }
                }
                
                validation.effect?.SetActive(true);
                validation.isInitialized = true;
                Debug.Log($"🎯 마커 {validation.markerId}의 지연된 초기 위치 설정(Update): {validation.startPos:F3}, 목표 위치: {targetPos:F3}");
            }

            Vector3 currentPos = marker.position;
            Vector3 moved = currentPos - validation.startPos;
            float diff = Vector3.Distance(moved, validation.expectedOffset);

            bool insideTargetRange = diff <= validation.tolerance;

            if (insideTargetRange)
            {
                validation.currentStayTime += Time.deltaTime;

                if (validation.effectRenderer != null)
                    validation.effectRenderer.material.color = Color.Lerp(validation.effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                if (validation.currentStayTime >= validation.requiredStayTime)
                {
                    validation.isVerified = true;
                    Debug.Log("✅ 이동 검증 성공!");
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

    private void HideEffectAfterDelay(MoveValidationData validation, float delay)
    {
        if (validation.effect == null) return;

        if (validation.hideCoroutine != null)
            StopCoroutine(validation.hideCoroutine);

        validation.hideCoroutine = StartCoroutine(HideEffectCoroutine(validation, delay));
    }

    private IEnumerator HideEffectCoroutine(MoveValidationData validation, float delay)
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
            // 코루틴이 실행 중이라면 중지
            if (validation.hideCoroutine != null)
            {
                StopCoroutine(validation.hideCoroutine);
                validation.hideCoroutine = null;
            }
            
            // 이펙트가 있다면 제거
            if (validation.effect != null)
            {
                Destroy(validation.effect);
                validation.effect = null;
                validation.effectRenderer = null;
            }
            
            // 모든 상태 초기화
            validation.isVerified = false;
            validation.currentStayTime = 0f;
            validation.startPos = Vector3.zero;
            validation.lastPosition = Vector3.zero;
            validation.isInitialized = false;  // 중요: isInitialized 플래그 초기화
        }
        
        isActive = true;  // 검증을 다시 활성화하여 새로운 초기화가 가능하도록 함
        Debug.Log("🔄 검증 시스템 초기화 완료");
    }
}
