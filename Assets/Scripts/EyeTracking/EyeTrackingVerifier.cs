using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class EyeTrackingVerifier : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 targetSize;
    private float requiredLookTime;
    private float currentLookTime = 0f;
    private float outOfBoundsTimer = 0f;

    public float allowedLookAwayTime = 0.5f;

    private bool isVerified = false;
    private bool isInitialized = false;
    private bool isActive = false;

    public Action OnVerified;

    public void Initialize(Vector3 position, Vector3 size, float lookTime, Action onSuccess)
    {
        targetPosition = position;
        targetSize = size;
        requiredLookTime = lookTime;
        OnVerified = onSuccess;

        isInitialized = true;
        ResetVerification();
    }

    public void StartVerification()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("🔴 EyeTrackingVerifier: Initialize() 먼저 호출해야 합니다.");
            return;
        }

        isActive = true;
        Debug.Log("👁️‍🗨️ 시선 검증 시작됨");
    }

    public void StopVerification()
    {
        isActive = false;
        Debug.Log("⛔ 시선 검증 중단됨");
    }

    public void ResetVerification()
    {
        isVerified = false;
        currentLookTime = 0f;
        outOfBoundsTimer = 0f;
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

            RaycastHit hit;
            if (Physics.Raycast(gazeRay, out hit, 10f))
            {
                Vector3 gazePoint = hit.point;

                // 🔍 디버그용 로그: 무엇을 바라보고 있는가?
                Debug.Log($"👁️ 시선이 '{hit.collider.gameObject.name}' 오브젝트에 닿음");

                Bounds bounds = new Bounds(targetPosition, targetSize);

                if (bounds.Contains(gazePoint))
                {
                    currentLookTime += Time.deltaTime;
                    outOfBoundsTimer = 0f;

                    if (currentLookTime >= requiredLookTime)
                    {
                        isVerified = true;
                        isActive = false;
                        Debug.Log("✅ 시선 검증 성공!");
                        OnVerified?.Invoke();
                    }
                }
                else
                {
                    outOfBoundsTimer += Time.deltaTime;

                    if (outOfBoundsTimer >= allowedLookAwayTime)
                    {
                        currentLookTime = 0f;
                        outOfBoundsTimer = 0f;
                        Debug.Log("🔁 시선 너무 오래 벗어남, 누적 시간 리셋");
                    }
                }
            }

        }
    }
}