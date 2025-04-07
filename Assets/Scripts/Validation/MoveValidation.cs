using System;
using UnityEngine;

public class MoveValidation : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    private int markerId;
    private Vector3 startPos;
    private Vector3 expectedOffset;
    private float tolerance;
    private float requiredStayTime;
    private float currentStayTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;

    public void BeginValidation(int markerId, Vector3 targetOffset, float tolerance, float stayTime, Action onSuccess)
    {
        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            return;
        }

        this.markerId = markerId;
        this.expectedOffset = targetOffset;
        this.tolerance = tolerance;
        this.requiredStayTime = stayTime;
        this.onVerifiedCallback = onSuccess;

        startPos = marker.position;
        currentStayTime = 0f;
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

        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            return;
        }

        Vector3 currentPos = marker.position;
        Vector3 moved = currentPos - startPos;

        float diff = Vector3.Distance(moved, expectedOffset);

        bool insideTargetRange = diff <= tolerance;

        if (insideTargetRange)
        {
            currentStayTime += Time.deltaTime;


            if (currentStayTime >= requiredStayTime)
            {
                IsVerified = true;
                isActive = false;
                onVerifiedCallback?.Invoke();
            }
        }
        else
        {
            if (currentStayTime > 0f)

            currentStayTime = 0f;
        }
    }
}
