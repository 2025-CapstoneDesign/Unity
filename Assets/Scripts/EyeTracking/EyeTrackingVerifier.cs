using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

/*
    사용 예시 :
    Vector3 targetPos = new Vector3(0, 1.5f, 2f);
    Vector3 targetSize = new Vector3(0.4f, 0.4f, 0.4f);
    float lookTime = 2.5f;
    verifier.BeginVerification(targetPos, targetSize, lookTime, OnEyeVerified);
*/
public class EyeTrackingVerifier : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 targetSize;
    private float requiredLookTime;
    private float currentLookTime = 0f;

    private bool isVerified = false;
    private bool isActive = false;

    private Action onVerifiedCallback;

    public bool IsVerified => isVerified;

    /// <summary>
    /// 외부에서 좌표, 크기, 시간, 성공 콜백을 한 번에 넣어 검증을 시작합니다.
    /// </summary>
    public void BeginVerification(Vector3 position, Vector3 size, float lookTime, Action onSuccess)
    {
        targetPosition = position;
        targetSize = size;
        requiredLookTime = lookTime;
        onVerifiedCallback = onSuccess;

        ResetVerification();
        isActive = true;

        Debug.Log("👁️‍🗨️ 시선 검증 시작됨");
    }

    /// <summary>
    /// 검증 강제 중단
    /// </summary>
    public void StopVerification()
    {
        isActive = false;
        Debug.Log("⛔ 시선 검증 중단됨");
    }

    /// <summary>
    /// 내부 상태 초기화
    /// </summary>
    private void ResetVerification()
    {
        isVerified = false;
        currentLookTime = 0f;
    }

    void Update()
    {
        if (!isActive || isVerified || CoreServices.InputSystem?.EyeGazeProvider == null)
            return;

        var eyeGazeProvider = CoreServices.InputSystem.EyeGazeProvider;

        if (eyeGazeProvider.IsEyeTrackingEnabled)
        {
            Vector3 origin = eyeGazeProvider.GazeOrigin;
            Vector3 direction = eyeGazeProvider.GazeDirection;
            Ray gazeRay = new Ray(origin, direction);

            if (Physics.Raycast(gazeRay, out RaycastHit hit, 10f))
            {
                Vector3 gazePoint = hit.point;

                Debug.Log($"👁️ 시선이 '{hit.collider.gameObject.name}' 오브젝트에 닿음");

                Bounds bounds = new Bounds(targetPosition, targetSize);

                if (bounds.Contains(gazePoint))
                {
                    currentLookTime += Time.deltaTime;

                    if (currentLookTime >= requiredLookTime)
                    {
                        isVerified = true;
                        isActive = false;
                        Debug.Log("✅ 시선 검증 성공!");
                        onVerifiedCallback?.Invoke();
                    }
                }
            }
        }
    }
}
