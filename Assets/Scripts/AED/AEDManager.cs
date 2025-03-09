using System.Collections;
using UnityEngine;
using TMPro;

public class AEDManager : MonoBehaviour
{
    [SerializeField] private TimerManager timerManager; // 타이머 매니저 추가
    private CPRState currentState;
    private bool externalInput;  // 외부 입력값 (True/False
    //private LLMManager llmManager;
    private float timeLimit;


    [SerializeField] private TextMeshProUGUI aedMessageText;

    void Start()
    {
        currentState = CPRState.CheckSafety;
        externalInput = false;  // 외부 입력은 기본적으로 false로 설정
        StartCoroutine(CPRProcedure());
    }


    private IEnumerator CPRProcedure()
    {
        while (currentState != CPRState.Completed)
        {
            aedMessageText.text = AEDMessageManager.GetMessage(currentState);
            switch (currentState)
            {
                case CPRState.CheckSafety:
                    Debug.Log("1. 현장 안전을 확인한다.");

                    // 해당 상태에 대한 제한 시간 설정
                    timeLimit = GetStateTimeLimit(currentState);
                    timerManager.StartTimer(timeLimit);
                    Debug.Log("AEDManager : 타이머 시작");

                    // 🔹 타이머 반복문 추가
                    while (!timerManager.IsTimeUp())
                    {
                        yield return null;  // 매 프레임 대기
                    }
                    if (timerManager.IsTimeUp())
                    {
                        HandleTimeoutMessage();
                    }
                    
                    break;

                case CPRState.WearPPE:
                    Debug.Log("2. 감염 방지를 위한 개인보호장구를 착용한다.");

                    timeLimit = GetStateTimeLimit(currentState);
                    timerManager.StartTimer(timeLimit);

                    // 🔹 타이머 반복문 추가
                    while (!timerManager.IsTimeUp())
                    {
                        yield return null;  // 매 프레임 대기
                    }

                    if (timerManager.IsTimeUp())
                    {
                        HandleTimeoutMessage();
                        yield break;  // 시간 초과 시 종료
                    }

                    break;

                case CPRState.CheckConsciousness:
                    Debug.Log("3. 의식을 확인한다.");
                    break;

                case CPRState.Call119AndRequestAED:
                    Debug.Log("4. 119 신고 및 AED를 요청한다.");

                    break;

                case CPRState.CheckBreathingAndPulse:
                    Debug.Log("5. 호흡과 맥박을 동시에 확인한다.");

                    break;

                case CPRState.ChestCompressions:
                    Debug.Log("6. 가슴압박을 30회 실시한다.");
                    // CPRManager cprManager = new CPRManager(sensorManager);
                    // while (!cprManager.IsCPRPassed)
                    // {
                    //     Debug.Log("wait 프레임");
                    //     yield return null; // 다음 프레임 대기
                    // }
                    Debug.Log("CPR 통과!!!!!!!!!!!!");

                    break;

                case CPRState.OpenAirway:
                    Debug.Log("7. 기도를 개방한다.");

                    break;

                case CPRState.ProvideRescueBreaths:
                    Debug.Log("8. 포켓마스크를 사용하여 인공호흡을 2회 실시한다.");

                    break;

                case CPRState.ContinueCPR:
                    Debug.Log("9. 가슴압박과 인공호흡을 30 : 2로 5주기 실시한다.");

                    break;

                case CPRState.DirectAssistants:
                    Debug.Log("10. 보조요원에게 CPR을 지시한다.");

                    break;

                case CPRState.TurnOnAED:
                    Debug.Log("11. AED의 전원을 켠다.");

                    break;

                case CPRState.AttachPads:
                    Debug.Log("12. 제세동 패드를 부착한다.");

                    break;

                case CPRState.ClearArea:
                    Debug.Log("13. 분석 전과 제세동 전에 주위 사람들을 물러나도록 한다.");
                    break;

                case CPRState.DeliverShock:
                    Debug.Log("14. 쇼크 버튼을 누른다.");
                    break;

                case CPRState.ResumeChestCompressions:
                    Debug.Log("15. 즉시 가슴압박을 시작한다.");
                    break;

                case CPRState.RecordDocuments:
                    Debug.Log("16. 의무기록지에 기록한다.");
                    break;
            }
        }
    }

    // 외부 입력값을 기다리는 코루틴
    private IEnumerator WaitForInput()
    {
        // 외부 입력이 true가 될 때까지 대기
        while (!externalInput)
        {
            yield return null;  // 프레임마다 계속 대기
        }   

        // 외부 입력이 true일 경우, 다음 단계로 넘어감
        externalInput = false;  // 입력을 받은 후, 상태를 초기화하여 재시도하지 않도록 함
        currentState++;  // 다음 단계로 넘어감
    }


    private IEnumerator WaitForInputOrTimeout()
    {
        while (!externalInput && !timerManager.IsTimeUp())
        {
            yield return null;
        }

        externalInput = false;
    }

    private void HandleTimeoutMessage()
    {
        Debug.Log($"🚨 {currentState} 단계에서 시간 초과! 조치 필요.");
    }

    private float GetStateTimeLimit(CPRState state)
    {
        switch (state)
        {
            case CPRState.CheckSafety: return 5f;
            case CPRState.WearPPE: return 20f;
            case CPRState.CheckConsciousness: return 5f;
            case CPRState.Call119AndRequestAED: return 10f;
            case CPRState.CheckBreathingAndPulse: return 7f;
            case CPRState.ChestCompressions: return 15f;
            case CPRState.OpenAirway: return 5f;
            case CPRState.ProvideRescueBreaths: return 6f;
            case CPRState.ContinueCPR: return 30f;
            case CPRState.DirectAssistants: return 10f;
            case CPRState.TurnOnAED: return 5f;
            case CPRState.AttachPads: return 8f;
            case CPRState.ClearArea: return 4f;
            case CPRState.DeliverShock: return 3f;
            case CPRState.ResumeChestCompressions: return 12f;
            case CPRState.RecordDocuments: return 10f;
            default: return 0f;
        }
    }
}
