using System.Collections.Generic;

public static class InfantCPRMessageManager
{
    private static readonly Dictionary<InfantCPRState, string> messages = new Dictionary<InfantCPRState, string>
    {
        { InfantCPRState.EnsureSceneSafety, "수정" },
        { InfantCPRState.WearPPE, "수정" },
        { InfantCPRState.CheckConsciousness, "수정" },
        { InfantCPRState.Call119AndRequestAED, "수정" },
        { InfantCPRState.CheckBreathingAndPulse, "수정" },
        { InfantCPRState.Perform30ChestCompressions, "수정" },
        { InfantCPRState.OpenAirway, "수정" },
        { InfantCPRState.Perform2RescueBreathsWithPocketMask, "수정" },
        { InfantCPRState.Perform5CyclesOf30To2CPR, "수정" },
        { InfantCPRState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(InfantCPRState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}