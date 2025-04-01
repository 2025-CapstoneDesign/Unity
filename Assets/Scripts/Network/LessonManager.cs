using UnityEngine;

public class LessonManager : MonoBehaviour
{
    public static LessonManager Instance;

    private int currentStep = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 테스트: 시작 시 첫 단계 실행
        StartStep(1);
    }

    void Update()
    {
        // 테스트: N 키 누르면 다음 단계로
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextStep();
        }

        // 테스트: S 키 누르면 Submit 평가 요청
        if (Input.GetKeyDown(KeyCode.S))
        {
            TrainingEvaluator.Instance.SubmitAction();
        }
    }

    public void StartStep(int step)
    {
        currentStep = step;
        Debug.Log($"📘 Step {currentStep} 시작");

        if (currentStep == 2) // 평가가 필요한 단계
        {
            VoiceSender.Instance.StartCapture();
        }
        else
        {
            VoiceSender.Instance.StopCapture();
        }

        // 필요 시 UI 등 다른 동작 추가
    }

    public void GoToNextStep()
    {
        StartStep(currentStep + 1);
    }
}
