using System.Collections.Generic;

public static class InfantCPRMessageManager
{
    private static readonly Dictionary<InfantCPRState, string> messages = new Dictionary<InfantCPRState, string>
    {
        { InfantCPRState.EnsureSceneSafety, "현장 안전을 확인하세요." },
        { InfantCPRState.WearPPE, "감염 방지를 위한 개인보호장구를 착용하세요." },
        { InfantCPRState.CheckConsciousness, "의식을 확인하세요." },
        { InfantCPRState.Call119AndRequestAED, "119 신고 및 AED를 요청하세요." },
        { InfantCPRState.CheckBreathingAndPulse, "호흡과 맥박을 동시에 확인하세요." },
        { InfantCPRState.Perform30ChestCompressions, "가슴압박을 30회 실시하세요." },
        { InfantCPRState.OpenAirway, "기도를 개방하세요." },
        { InfantCPRState.Perform2RescueBreathsWithPocketMask, "포켓마스크를 사용하여 인공호흡을 2회 실시하세요." },
        { InfantCPRState.Perform5CyclesOf30To2CPR, "가슴압박과 인공호흡을 30 : 2로 5주기 실시하세요." },
        { InfantCPRState.RecordOnMedicalChart, "의무기록지에 기록하세요." }
    };

    public static string GetMessage(InfantCPRState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}