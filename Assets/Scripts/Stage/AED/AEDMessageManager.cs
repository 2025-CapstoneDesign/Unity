using System.Collections.Generic;

public static class AEDMessageManager
{
    private static readonly Dictionary<CPRState, string> messages = new Dictionary<CPRState, string>
    {
        { CPRState.CheckSafety, "현장의 안전을 확인하세요!" }, //  타이머
        { CPRState.WearPPE, "감염 방지를 위해 개인보호장구를 착용하세요." }, //색깔, 타이머 - 장갑
        { CPRState.CheckConsciousness, "환자의 의식을 확인하세요." }, 
        { CPRState.Call119AndRequestAED, "119에 신고하고 AED를 요청하세요." },
        { CPRState.CheckBreathingAndPulse, "환자의 호흡과 맥박을 동시에 확인하세요." },
        { CPRState.ChestCompressions, "가슴압박을 30회 실시하세요." },
        { CPRState.OpenAirway, "기도를 개방하세요." },
        { CPRState.ProvideRescueBreaths, "포켓마스크를 사용하여 인공호흡을 2회 실시하세요." },
        { CPRState.ContinueCPR, "가슴압박과 인공호흡을 30:2 비율로 5주기 반복하세요." },
        { CPRState.DirectAssistants, "보조요원에게 CPR을 지시하세요." },
        { CPRState.TurnOnAED, "AED의 전원을 켜세요." },
        { CPRState.AttachPads, "제세동 패드를 정확히 부착하세요." },
        { CPRState.ClearArea, "분석 및 제세동 전, 주위 사람들을 물러나게 하세요." },
        { CPRState.DeliverShock, "AED의 쇼크 버튼을 눌러 제세동을 시행하세요." },
        { CPRState.ResumeChestCompressions, "즉시 가슴압박을 재개하세요." },
        { CPRState.RecordDocuments, "의무기록지에 CPR 내용을 기록하세요." },
        { CPRState.Completed, "🎉 CPR 훈련이 완료되었습니다! 수고하셨습니다." }
    };

    public static string GetMessage(CPRState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}
