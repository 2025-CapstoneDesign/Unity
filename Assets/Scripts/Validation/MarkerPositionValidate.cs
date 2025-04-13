using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerPositionValidate : MonoBehaviour
{
    [Serializable]
    public class MarkerValidation
    {
        public int baseMarkerId;
        public int targetMarkerId;
        public Vector3 expectedOffset;
        public float tolerance;
        public float requiredStayTime;
        public Action onVerifiedCallback;

        [NonSerialized] public float currentStayTime = 0f;
        [NonSerialized] public bool isVerified = false;
        [NonSerialized] public GameObject effect;
        [NonSerialized] public Renderer renderer;
        [NonSerialized] public Vector3 targetWorldPos;
        [NonSerialized] public Coroutine hideCoroutine;
    }

    public bool IsAnyVerified => validations.Exists(v => v.isVerified);

    [SerializeField] private GameObject targetEffectPrefab;
    private readonly List<MarkerValidation> validations = new();
    private readonly Color defaultColor = Color.red;
    private readonly Color successColor = Color.green;

    private bool isActive = false;

    public void BeginValidation(int baseMarkerId, int targetMarkerId, Vector3 expectedOffset, float tolerance, float stayTime, Action onSuccess)
    {
        MarkerValidation v = new()
        {
            baseMarkerId = baseMarkerId,
            targetMarkerId = targetMarkerId,
            expectedOffset = expectedOffset,
            tolerance = tolerance,
            requiredStayTime = stayTime,
            onVerifiedCallback = onSuccess
        };
        validations.Add(v);
        isActive = true;

        Debug.Log($"📌 마커 위치 검증 등록: 기준 {baseMarkerId} → 대상 {targetMarkerId}, 목표 오프셋: {expectedOffset:F3}, 오차 ±{tolerance:F3}, 유지시간 {stayTime}s");
    }

    public void StopValidation()
    {
        isActive = false;
        foreach (var v in validations)
        {
            if (v.effect != null)
            {
                v.effect.SetActive(false);
                if (v.renderer != null)
                    v.renderer.material.color = defaultColor;
            }
        }
    }

    void Update()
    {
        if (!isActive)
            return;

        var map = OptimizedArUcoMarkerDetection.markerMap;

        foreach (var v in validations)
        {
            if (v.isVerified)
                continue;

            if (!map.TryGetValue(v.baseMarkerId, out MarkerData baseMarker))
                continue;

            v.targetWorldPos = baseMarker.position + (baseMarker.rotation * v.expectedOffset);

            if (v.effect == null && targetEffectPrefab != null)
            {
                v.effect = Instantiate(targetEffectPrefab, v.targetWorldPos, baseMarker.rotation);
                v.effect.transform.localScale = Vector3.one * (v.tolerance * 0.5f);

                v.renderer = v.effect.GetComponentInChildren<Renderer>();
                if (v.renderer != null)
                {
                    v.renderer.material = new Material(v.renderer.material);
                    v.renderer.material.color = defaultColor;
                }
            }

            if (v.effect != null)
            {
                v.effect.transform.position = v.targetWorldPos;
                v.effect.transform.rotation = baseMarker.rotation;
                v.effect.SetActive(true);
            }

            if (!map.TryGetValue(v.targetMarkerId, out MarkerData targetMarker))
                continue;

            Vector3 actualOffset = Quaternion.Inverse(baseMarker.rotation) * (targetMarker.position - baseMarker.position);
            float diff = Vector3.Distance(actualOffset, v.expectedOffset);
            bool insideRange = diff <= v.tolerance;

            if (insideRange)
            {
                v.currentStayTime += Time.deltaTime;

                if (v.renderer != null)
                    v.renderer.material.color = Color.Lerp(v.renderer.material.color, successColor, Time.deltaTime * 8f);

                if (v.currentStayTime >= v.requiredStayTime)
                {
                    v.isVerified = true;
                    Debug.Log($"✅ 마커 {v.targetMarkerId} 검증 통과!");
                    v.onVerifiedCallback?.Invoke();
                    if (v.effect != null && v.hideCoroutine == null)
                    {
                        v.hideCoroutine = StartCoroutine(HideEffectAfterDelay(v));
                    }
                }
            }
            else
            {
                v.currentStayTime = 0f;
                if (v.renderer != null)
                    v.renderer.material.color = Color.Lerp(v.renderer.material.color, defaultColor, Time.deltaTime * 8f);
            }
        }
    }

    private IEnumerator HideEffectAfterDelay(MarkerValidation v)
    {
        yield return new WaitForSeconds(0.5f);
        if (v.effect != null)
            v.effect.SetActive(false);
        v.hideCoroutine = null;
    }

    public void ResetValidation()
    {
        isActive = false;
        foreach (var v in validations)
        {
            if (v.effect != null)
                Destroy(v.effect);
        }
        validations.Clear();
    }
}
