using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class EyeTrackingValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    [SerializeField]
    private GameObject targetObject; // 검증 위치에 표시될 오브젝트

    private Renderer targetRenderer;
    private Color originalColor;
    private bool isLooking = false;

    private float radius;
    private float requiredTime;
    private float currentTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;

    private int markerId;

    void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);

            // 오브젝트의 Renderer 및 색상 저장
            targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                // 머티리얼 인스턴스를 복사해서 다른 오브젝트에 영향 없게 함
                targetRenderer.material = new Material(targetRenderer.material);
                originalColor = targetRenderer.material.color;
            }
        }
    }

    /// <summary>
    /// 검증 시작: 마커ID, 상대 위치, 반지름, 시간, 콜백
    /// </summary>
    public void BeginVerification(int markerId, Vector3 localOffset, float radius, float holdTime, Action onSuccess)
    {
        this.markerId = markerId;
        this.radius = radius;
        this.requiredTime = holdTime;
        this.onVerifiedCallback = onSuccess;

        IsVerified = false;
        currentTime = 0f;
        isActive = true;

        if (OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            Vector3 worldOffset = marker.rotation * localOffset;
            Vector3 targetWorldPos = marker.position + worldOffset;

            if (targetObject != null)
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    targetObject.transform.position = targetWorldPos;
                    targetObject.transform.rotation = marker.rotation;
                    targetObject.SetActive(true);
                });
            }


            Debug.Log("👁️‍🗨️ 시선 검증 시작됨 (마커 기준)");
        }
        else
        {
            Debug.LogWarning($"❌ 마커 ID {markerId} 를 찾을 수 없습니다.");
            isActive = false;
        }
    }

    public void StopVerification()
    {
        isActive = false;

        if (targetObject != null)
        {
            targetObject.SetActive(false);

            if (targetRenderer != null)
            {
                targetRenderer.material.color = originalColor;
                isLooking = false;
            }
        }

        Debug.Log("⛔ 시선 검증 중단됨");
    }

    void Update()
    {
        if (!isActive || IsVerified || CoreServices.InputSystem?.EyeGazeProvider == null)
            return;

        var gazeProvider = CoreServices.InputSystem.EyeGazeProvider;

        if (!gazeProvider.IsEyeTrackingEnabled)
            return;

        Vector3 origin = gazeProvider.GazeOrigin;
        Vector3 direction = gazeProvider.GazeDirection;
        Ray gazeRay = new Ray(origin, direction);

        bool hitTarget = false;

        if (Physics.Raycast(gazeRay, out RaycastHit hit, 10f))
        {
            Vector3 gazePoint = hit.point;

            if (targetObject != null)
            {
                float sqrDist = (gazePoint - targetObject.transform.position).sqrMagnitude;

                if (sqrDist <= radius * radius)
                {
                    hitTarget = true;
                    currentTime += Time.deltaTime;

                    if (currentTime >= requiredTime)
                    {
                        IsVerified = true;
                        isActive = false;

                        if (targetObject != null)
                            targetObject.SetActive(false);

                        Debug.Log("✅ 시선 검증 성공 (마커 기준)!");
                        onVerifiedCallback?.Invoke();
                    }
                }
            }
        }

        // 색상 처리
        if (targetRenderer != null)
        {
            if (hitTarget && !isLooking)
            {
                targetRenderer.material.color = Color.green;
                isLooking = true;
            }
            else if (!hitTarget && isLooking)
            {
                targetRenderer.material.color = originalColor;
                isLooking = false;
            }
        }
    }
}
