using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandTrackingValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    private Vector3 targetLocalOffset;
    private float radius;
    private float requiredTime;
    private float currentTime = 0f;

    private Vector3 targetWorldPos;
    private bool isActive = false;
    private Action onVerifiedCallback;

    private int markerId;

    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
        this.markerId = markerId;
        this.targetLocalOffset = localOffset;
        this.radius = radius;
        this.requiredTime = holdTime;
        this.onVerifiedCallback = onSuccess;

        IsVerified = false;
        currentTime = 0f;
        isActive = true;

    }

    public void StopVerification()
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

        // 마커 기준 상대 위치를 월드 위치로 변환
        Vector3 worldOffset = marker.rotation * targetLocalOffset;
        targetWorldPos = marker.position + worldOffset;

        // 손 위치 (오른손 기준)
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose))
        {
            Vector3 handPos = pose.Position;
        float dist = Vector3.Distance(handPos, targetWorldPos);


            if (dist <= radius)
            {
                currentTime += Time.deltaTime;

                if (currentTime >= requiredTime)
                {
                    IsVerified = true;
                    isActive = false;
                    onVerifiedCallback?.Invoke();
                }
            }
        }
    }
}
