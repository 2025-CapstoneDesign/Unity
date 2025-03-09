using System.Collections.Generic;

public static class AEDMessageManager
{
    private static readonly Dictionary<CPRState, string> messages = new Dictionary<CPRState, string>
    {
        { CPRState.CheckSafety, "1. 현장 안전을 확인한다." },
        { CPRState.WearPPE, "2. 감염 방지를 위한 개인보호장구를 착용한다." },
        { CPRState.CheckConsciousness, "3. 의식을 확인한다." },
        { CPRState.Call119AndRequestAED, "4. 119 신고 및 AED를 요청한다." },
        { CPRState.CheckBreathingAndPulse, "5. 호흡과 맥박을 동시에 확인한다." },
        { CPRState.ChestCompressions, "6. 가슴압박을 30회 실시한다." },
        { CPRState.OpenAirway, "7. 기도를 개방한다." },
        { CPRState.ProvideRescueBreaths, "8. 포켓마스크를 사용하여 인공호흡을 2회 실시한다." },
        { CPRState.ContinueCPR, "9. 가슴압박과 인공호흡을 30:2로 5주기 실시한다." },
        { CPRState.DirectAssistants, "10. 보조요원에게 CPR을 지시한다." },
        { CPRState.TurnOnAED, "11. AED의 전원을 켠다." },
        { CPRState.AttachPads, "12. 제세동 패드를 부착한다." },
        { CPRState.ClearArea, "13. 분석 전과 제세동 전에 주위 사람들을 물러나도록 한다." },
        { CPRState.DeliverShock, "14. 쇼크 버튼을 누른다." },
        { CPRState.ResumeChestCompressions, "15. 즉시 가슴압박을 시작한다." },
        { CPRState.RecordDocuments, "16. 의무기록지에 기록한다." },
        { CPRState.Completed, "CPR 과정이 완료되었습니다." }
    };

    public static string GetMessage(CPRState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}