using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScenario : MonoBehaviour
{
    public EyeTrackingVerifier verifier;          // EyeTrackingVerifier 연결
    public Transform aedTargetTransform;          // 1단계: AED 위치
    public Transform headTargetTransform;         // 2단계: 환자 머리 위치

    public Vector3 targetSize = new Vector3(0.3f, 0.3f, 0.3f);  // 범위 크기

    private int currentStep = 0;

    void Start()
    {
        StartStep1();
    }

    void StartStep1()
    {
        Debug.Log("🟢 1단계 시작: AED를 3초간 바라보세요");
        currentStep = 1;

        verifier.BeginVerification(
            aedTargetTransform.position,
            targetSize,
            3.0f,
            OnStep1Success
        );
    }

    void OnStep1Success()
    {
        Debug.Log("✅ 1단계 완료: AED 검증 성공");
        StartStep2();
    }

    void StartStep2()
    {
        Debug.Log("🟢 2단계 시작: 환자 머리를 2초간 바라보세요");
        currentStep = 2;

        verifier.BeginVerification(
            headTargetTransform.position,
            targetSize,
            2.0f,
            OnStep2Success
        );
    }

    void OnStep2Success()
    {
        Debug.Log("🎉 모든 단계 완료! 훈련 성공!");
        // 여기에 훈련 종료 또는 다음 시나리오 진행 로직 넣을 수 있음
    }
}
