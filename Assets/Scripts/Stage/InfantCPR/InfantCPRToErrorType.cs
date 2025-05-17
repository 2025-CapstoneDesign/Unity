using System;
using System.Collections.Generic;

public static class InfantCPRToErrorType
{
    private static readonly Dictionary<InfantCPRState, string> stateToLabel = new Dictionary<InfantCPRState, string>
    {
        { InfantCPRState.EnsureSceneSafety,                     "현장 안전 확인" },
        { InfantCPRState.WearPPE,                               "보호장비 착용" },
        { InfantCPRState.CheckConsciousness,                    "PASS" },
        { InfantCPRState.Call119AndRequestAED,                  "AED 요청" },
        { InfantCPRState.CheckBreathingAndPulse,                "자동제세동기의 맥박 호흡 확인" },
        { InfantCPRState.Perform30ChestCompressions,            "PASS" },
        { InfantCPRState.OpenAirway,                            "PASS" },
        { InfantCPRState.Perform2RescueBreathsWithPocketMask,   "PASS" },
        { InfantCPRState.Perform5CyclesOf30To2CPR,              "PASS" },
        { InfantCPRState.RecordOnMedicalChart,                  "PASS" }
    };

    public static string GetLabel(InfantCPRState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}