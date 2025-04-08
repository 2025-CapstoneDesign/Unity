using System;
using UnityEngine;

public class ValidateScenarioCotnroller : MonoBehaviour
{
    public MarkerDistanceValidate distanceValidator;
    public EyeTrackingValidate eyeValidator;
    public HandTrackingValidate handValidator;

    public GameObject eyeTargetObject;

    private enum Step
    {
        None,
        Distance,
        EyeTracking,
        HandTracking,
        Completed
    }

    private Step currentStep = Step.None;

    private void Start()
    {
        currentStep = Step.Distance;

        InvokeRepeating(nameof(LogCurrentStep), 1f, 1f);

        // Step 1: 거리 검증
        distanceValidator.BeginValidation(1, 2, 0.2f, 0.3f, () =>
        {
            Debug.Log("[NS] : 마커 거리 검증 통과!");
            StartEyeTrackingStep();
        });
    }

    private void StartEyeTrackingStep()
    {
        currentStep = Step.EyeTracking;

        // Step 2: 시선 검증
        eyeValidator.BeginVerification(1, new Vector3(0.1f, 0f, 0.1f), 0.1f, 2f, () =>
        {
            Debug.Log("[NS] : 시선 검증 통과!");
            StartHandTrackingStep();
        });
    }

    private void StartHandTrackingStep()
    {
        currentStep = Step.HandTracking;

        // Step 3: 손 위치 검증
        handValidator.BeginVerification(2, new Vector3(0.1f, 0f, 0.1f), 0.1f, 2f, () =>
        {
            Debug.Log("[NS] : 손 검증 통과!");
            OnAllValidated();
        });
    }

    private void OnAllValidated()
    {
        currentStep = Step.Completed;

        Debug.Log("[NS] : 모든 검증 완료! 미션 성공!");

        CancelInvoke(nameof(LogCurrentStep));
    }

    private void LogCurrentStep()
    {
        switch (currentStep)
        {   
            case Step.None:
                Debug.Log("[NS] 현재 단계 : 시작 중..");
                break;
            case Step.Distance:
                Debug.Log("[NS] 🔍 현재 단계: 거리 검증 중...");
                break;
            case Step.EyeTracking:
                Debug.Log("[NS] 👁️ 현재 단계: 시선 검증 중...");
                break;
            case Step.HandTracking:
                Debug.Log("[NS] ✋ 현재 단계: 손 위치 검증 중...");
                break;
            case Step.Completed:
                Debug.Log("[NS] 🎉 모든 검증 완료!");
                break;
        }
    }
}
