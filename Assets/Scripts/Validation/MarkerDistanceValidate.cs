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
    }

    public void StopValidation()
    {
        isActive = false;
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

                onVerifiedCallback?.Invoke();
            }
            else
            {
            }
        }
        else
        {
        }
    }
}
