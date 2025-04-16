using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input; // Hand tracking utilities
using Microsoft.MixedReality.Toolkit.Utilities; // TrackedHandJoint, Handedness

public class HandPositionDetection : MonoBehaviour
{
    void Update()
    {
        // 오른손 감지 및 위치 출력
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose rightPose))
        {
            Debug.Log("오른손 위치: " + rightPose.Position);
        }

        // 왼손 감지 및 위치 출력
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Left, out MixedRealityPose leftPose))
        {
            Debug.Log("왼손 위치: " + leftPose.Position);
        }
    }
}
