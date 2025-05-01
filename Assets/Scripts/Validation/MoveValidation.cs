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
            isInitialized = false,
            startPos = Vector3.zero
        };
        
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

            if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(validation.markerId, out MarkerData marker))
            {
                // 마커가 인식되지 않았을 때 이펙트 숨기기
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                continue;
            }

            if (validation.startPos == Vector3.zero)
                validation.startPos = marker.position;

            Vector3 newTargetPos = validation.startPos + validation.expectedOffset;

            if (!validation.isInitialized)
            {
                if (validation.effect == null && targetEffectPrefab != null)
                {
                    validation.effect = Instantiate(targetEffectPrefab, newTargetPos, Quaternion.identity);
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
                    validation.effect.SetActive(true);
                }
                
                validation.isInitialized = true;
                validation.lastPosition = newTargetPos;
                continue;
            }

            float positionChange = Vector3.Distance(validation.lastPosition, newTargetPos);

            if (positionChange > 0.1f)
            {
                if (validation.effect != null)
                {
                    validation.effect.transform.position = newTargetPos;
                }
            }
            else
            {
                if (validation.effect != null)
                {
                    validation.effect.transform.position = Vector3.Lerp(validation.effect.transform.position, newTargetPos, Time.deltaTime * 10f);
                }
            }

            validation.lastPosition = newTargetPos;

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
            validation.isVerified = false;
            validation.currentStayTime = 0f;
            validation.startPos = Vector3.zero;

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
