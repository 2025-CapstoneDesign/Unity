using System;
using System.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class EyeTrackingValidate : MonoBehaviour
{
    public bool IsVerified { get; private set; } = false;

    [SerializeField] private GameObject targetEffectPrefab;
    private GameObject activeEffect;
    private Renderer effectRenderer;

    private Color defaultColor = Color.red;
    private Color successColor = Color.green;

    private float radius;
    private float requiredTime;
    private float currentTime = 0f;

    private bool isActive = false;
    private Action onVerifiedCallback;
    private Coroutine hideCoroutine;
    private bool isLooking = false;

    private int markerId;
    private Vector3 targetLocalOffset;
    private Vector3 targetWorldPos;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isInitialized = false;

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
        isInitialized = false;

        if (activeEffect != null)
        {
            Destroy(activeEffect);
            activeEffect = null;
            effectRenderer = null;
        }
    }

    public void StopVerification()
    {
        isActive = false;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }

        isLooking = false;
    }

    void Update()
    {
        if (!isActive || IsVerified || CoreServices.InputSystem?.EyeGazeProvider == null)
            return;

        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            if (activeEffect != null)
                activeEffect.SetActive(false);
            return;
        }

        Vector3 worldOffset = marker.rotation * targetLocalOffset;
        Vector3 newTargetPos = marker.position + worldOffset;

        if (!isInitialized)
        {
            if (activeEffect == null && targetEffectPrefab != null)
            {
                activeEffect = Instantiate(targetEffectPrefab, newTargetPos, marker.rotation);
                effectRenderer = activeEffect.GetComponentInChildren<Renderer>();
                if (effectRenderer != null)
                {
                    effectRenderer.material = new Material(effectRenderer.material);
                    effectRenderer.material.color = defaultColor;
                }
            }
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
                activeEffect.transform.rotation = marker.rotation;
                activeEffect.SetActive(true);
            }
            isInitialized = true;
            lastPosition = newTargetPos;
            lastRotation = marker.rotation;
            return;
        }

        float positionChange = Vector3.Distance(lastPosition, newTargetPos);
        float rotationChange = Quaternion.Angle(lastRotation, marker.rotation);

        if (positionChange > 0.1f || rotationChange > 30f)
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = newTargetPos;
                activeEffect.transform.rotation = marker.rotation;
            }
        }
        else
        {
            if (activeEffect != null)
            {
                activeEffect.transform.position = Vector3.Lerp(activeEffect.transform.position, newTargetPos, Time.deltaTime * 10f);
                activeEffect.transform.rotation = Quaternion.Slerp(activeEffect.transform.rotation, marker.rotation, Time.deltaTime * 10f);
            }
        }

        lastPosition = newTargetPos;
        lastRotation = marker.rotation;

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

            if (activeEffect != null)
            {
                float sqrDist = (gazePoint - activeEffect.transform.position).sqrMagnitude;

                if (sqrDist <= radius * radius)
                {
                    hitTarget = true;
                    currentTime += Time.deltaTime;

                    if (effectRenderer != null)
                        effectRenderer.material.color = Color.Lerp(effectRenderer.material.color, successColor, Time.deltaTime * 8f);

                    if (currentTime >= requiredTime)
                    {
                        IsVerified = true;
                        isActive = false;
                        HideEffectAfterDelay(0.5f);
                        onVerifiedCallback?.Invoke();
                    }
                }
            }
        }

        if (effectRenderer != null)
        {
            if (hitTarget && !isLooking)
            {
                isLooking = true;
            }
            else if (!hitTarget && isLooking)
            {
                effectRenderer.material.color = defaultColor;
                isLooking = false;
                currentTime = 0f;
            }
        }
    }

    private void HideEffectAfterDelay(float delay)
    {
        if (activeEffect == null) return;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideEffectCoroutine(delay));
    }

    private IEnumerator HideEffectCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activeEffect != null)
            activeEffect.SetActive(false);
    }

    public void ResetVerification()
    {
        IsVerified = false;
        isActive = false;
        currentTime = 0f;

        if (activeEffect != null)
        {
            activeEffect.SetActive(false);
            if (effectRenderer != null)
                effectRenderer.material.color = defaultColor;
        }

        isLooking = false;
    }
}
