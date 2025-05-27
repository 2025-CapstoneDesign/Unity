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
        [NonSerialized] public Quaternion lastRotation;
        [NonSerialized] public bool isInitialized = false;
        [NonSerialized] public Coroutine hideCoroutine;

        private Vector3 previousPosition;
        private Quaternion previousRotation;
        private const int historySize = 10;
        private readonly Queue<Vector3> positionHistory = new Queue<Vector3>(historySize);
        private readonly Queue<Quaternion> rotationHistory = new Queue<Quaternion>(historySize);

        public void UpdateHistory(Vector3 newPosition, Quaternion newRotation)
        {
            // 히스토리에 새로운 위치와 회전을 추가
            positionHistory.Enqueue(newPosition);
            rotationHistory.Enqueue(newRotation);

            // 히스토리 크기가 초과되면 오래된 데이터 제거
            if (positionHistory.Count > historySize)
            {
                positionHistory.Dequeue();
                rotationHistory.Dequeue();
            }

            previousPosition = positionHistory.Peek();
            previousRotation = rotationHistory.Peek();
        }

        public Vector3 GetStablePosition()
        {
            // 히스토리의 평균 위치 계산
            Vector3 sum = Vector3.zero;
            foreach (var pos in positionHistory)
            {
                sum += pos;
            }
            return sum / positionHistory.Count;
        }

        public Quaternion GetStableRotation()
        {
            // 히스토리의 평균 회전 계산 (쿼터니언 보간 사용)
            Quaternion avgRot = Quaternion.identity;
            foreach (var rot in rotationHistory)
            {
                avgRot *= rot;
            }
            return Quaternion.Normalize(avgRot);
        }
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
                if (validation.effect != null)
                    validation.effect.SetActive(false);
                continue;
            }
            else
            {
                if (validation.effect != null && !validation.effect.activeSelf)
                    validation.effect.SetActive(true);
            }

            if (!validation.isInitialized)
            {
                validation.startPos = marker.position;
                validation.lastRotation = marker.rotation;
                
                if (validation.effect == null && targetEffectPrefab != null)
                {
                    Vector3 targetOffset = marker.rotation * validation.expectedOffset;
                    Vector3 targetPos = marker.position + targetOffset;
                    
                    validation.effect = Instantiate(targetEffectPrefab, targetPos, marker.rotation);
                    validation.effectRenderer = validation.effect.GetComponentInChildren<Renderer>();
                    if (validation.effectRenderer != null)
                    {
                        validation.effectRenderer.material = new Material(validation.effectRenderer.material);
                        validation.effectRenderer.material.color = defaultColor;
                    }
                    
                    validation.effect?.SetActive(true);
                    validation.isInitialized = true;
                    Debug.Log($"🎯 마커 {validation.markerId}의 지연된 초기 위치 설정(Update): {validation.startPos:F3}, 목표 위치: {targetPos:F3}");
                }
                
                validation.UpdateHistory(marker.position, marker.rotation);
                continue;
            }

            // 타겟 위치 계산
            Vector3 worldOffset = marker.rotation * validation.expectedOffset;
            Vector3 newTargetPos = marker.position + worldOffset;

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

            float positionChange = Vector3.Distance(validation.lastPosition, newTargetPos);
            float rotationChange = Quaternion.Angle(validation.lastRotation, targetRotation);

            if (positionChange > 0.1f || rotationChange > 30f)
            {
                if (validation.effect != null)
                {
                    // 급격한 변화 시 안정화된 위치/회전 기반으로 천천히 보간
                    validation.effect.transform.position = Vector3.Lerp(stablePosition, newTargetPos, Time.deltaTime * 3f);
                    validation.effect.transform.rotation = Quaternion.Lerp(stableRotation, targetRotation, Time.deltaTime * 3f);
                }
            }
            else
            {
                if (validation.effect != null)
                {
                    validation.effect.transform.position = Vector3.Lerp(validation.effect.transform.position, newTargetPos, Time.deltaTime * 8f);
                    validation.effect.transform.rotation = Quaternion.Lerp(validation.effect.transform.rotation, targetRotation, Time.deltaTime * 8f);
                }
            }

            // 히스토리 업데이트
            validation.lastPosition = validation.effect.transform.position;
            validation.lastRotation = validation.effect.transform.rotation;
            validation.UpdateHistory(validation.lastPosition, validation.lastRotation);

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
