using System.Collections.Generic;

public static class AEDStateToErrorType
{
    private static readonly Dictionary<CPRState, string> stateToLabel = new Dictionary<CPRState, string>
    {
        { CPRState.CheckSafety,             "현장 안전 확인" },
        { CPRState.WearPPE,                 "보호장비 착용" },
        { CPRState.CheckConsciousness,      "PASS" },
        { CPRState.Call119AndRequestAED,    "AED 요청" },
        { CPRState.CheckBreathingAndPulse,  "자동 제세동기의 맥박 호흡 확인" },
        { CPRState.ChestCompressions,       "PASS" },
        { CPRState.OpenAirway,              "PASS" },
        { CPRState.ProvideRescueBreaths,    "PASS" },
        { CPRState.ContinueCPR,             "PASS" },
        { CPRState.DirectAssistants,        "자동제세동기의 CPR 진행" },
        { CPRState.TurnOnAED,               "PASS" },
        { CPRState.AttachPads,              "PASS" },
        { CPRState.ClearArea,               "쇼크버튼 누르기" },
        { CPRState.DeliverShock,            "PASS" },
        { CPRState.ResumeChestCompressions, "PASS" },
        { CPRState.RecordDocuments,         "PASS" },
        { CPRState.Completed,               "PASS" }
    };

    public static string GetLabel(CPRState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}
