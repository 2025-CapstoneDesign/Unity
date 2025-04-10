using System;
using UnityEngine;

public class MarkerPositionValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    private int baseMarkerId;
    private int targetMarkerId;
    private Vector3 expectedOffset;
    private float tolerance;
    private float requiredStayTime;
    private float currentStayTime = 0f;

    private Action onVerifiedCallback;
    private bool isActive = false;

    public void BeginValidation(int baseMarkerId, int targetMarkerId, Vector3 expectedOffset, float tolerance, float stayTime, Action onSuccess)
    {
        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(baseMarkerId, out MarkerData baseMarker) ||
            !OptimizedArUcoMarkerDetection.markerMap.TryGetValue(targetMarkerId, out MarkerData targetMarker))
        {
            Debug.LogWarning("❌ 시작 시 마커 위치를 찾을 수 없습니다.");
            return;
        }

        this.baseMarkerId = baseMarkerId;
        this.targetMarkerId = targetMarkerId;
        this.expectedOffset = expectedOffset;
        this.tolerance = tolerance;
        this.requiredStayTime = stayTime;
        this.onVerifiedCallback = onSuccess;

        currentStayTime = 0f;
        IsVerified = false;
        isActive = true;

        Debug.Log($"📌 마커 상대 위치 검증 시작: 기준 {baseMarkerId} → 대상 {targetMarkerId}, 목표 오프셋: {expectedOffset:F3}, 오차 ±{tolerance:F3}, 유지시간 {stayTime}s");
    }

    public void StopValidation()
    {
        isActive = false;
        Debug.Log("⛔ 마커 위치 검증 중단됨");
    }

    void Update()
    {
        if (!isActive || IsVerified)
            return;

        var map = OptimizedArUcoMarkerDetection.markerMap;

        if (!map.TryGetValue(baseMarkerId, out MarkerData baseMarker) ||
            !map.TryGetValue(targetMarkerId, out MarkerData targetMarker))
        {
            Debug.LogWarning("❌ 마커 인식되지 않음");
            return;
        }

        // 기준 마커 기준 좌표계에서 상대 위치 계산
        Vector3 actualOffset = Quaternion.Inverse(baseMarker.rotation) * (targetMarker.position - baseMarker.position);
        float diff = Vector3.Distance(actualOffset, expectedOffset);

        bool insideRange = diff <= tolerance;

        if (insideRange)
        {
            currentStayTime += Time.deltaTime;

            Debug.Log($"[POS DEBUG ✅] 실제 오프셋: {actualOffset:F3} | 차이: {diff:F3} | 머문 시간: {currentStayTime:F2}/{requiredStayTime}s");

            if (currentStayTime >= requiredStayTime)
            {
                IsVerified = true;
                isActive = false;
                Debug.Log("✅ 마커 상대 위치 + 유지시간 검증 성공!");
                onVerifiedCallback?.Invoke();
            }
        }
        else
        {
        }
    }
}
