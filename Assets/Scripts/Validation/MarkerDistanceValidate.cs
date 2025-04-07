using System;
using UnityEngine;

public class MarkerDistanceValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    private int markerId1;
    private int markerId2;
    private float minDistance;
    private float maxDistance;
    private Action onVerifiedCallback;

    private bool isActive = false;

    public void BeginValidation(int markerId1, int markerId2, float minDistance, float maxDistance, Action onSuccess)
    {
        this.markerId1 = markerId1;
        this.markerId2 = markerId2;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
        this.onVerifiedCallback = onSuccess;

        IsVerified = false;
        isActive = true;

        Debug.Log($"📏 마커 거리 검증 시작: ID {markerId1} ↔ {markerId2}, 범위 [{minDistance:F2} ~ {maxDistance:F2}]");
    }

    public void StopValidation()
    {
        isActive = false;
        Debug.Log("⛔ 마커 거리 검증 중단됨");
    }

    void Update()
    {
        if (!isActive || IsVerified)
            return;

        var map = OptimizedArUcoMarkerDetection.markerMap;

        if (map.TryGetValue(markerId1, out MarkerData marker1) && map.TryGetValue(markerId2, out MarkerData marker2))
        {
            float distance = Vector3.Distance(marker1.position, marker2.position);

            if (distance >= minDistance && distance <= maxDistance)
            {
                IsVerified = true;
                isActive = false;
                Debug.Log($"✅ 마커 거리 검증 성공! 거리: {distance:F2}m");

                onVerifiedCallback?.Invoke();
            }
            else
            {
                Debug.Log($"❌ 거리 {distance:F2}m - 범위 [{minDistance:F2} ~ {maxDistance:F2}] 벗어남");
            }
        }
        else
        {
            Debug.LogWarning("❌ 마커 위치를 찾을 수 없음: 하나 또는 둘 다 인식되지 않음");
        }
    }
}
