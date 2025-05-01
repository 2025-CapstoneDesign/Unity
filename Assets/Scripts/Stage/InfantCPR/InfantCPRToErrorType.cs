using System;
using System.Collections.Generic;

public static class InfantCPRToErrorType
{
    private static readonly Dictionary<InfantCPRState, string> stateToLabel = new Dictionary<InfantCPRState, string>
    {
        { InfantCPRState.EnsureSceneSafety,                     "PASS" },
        { InfantCPRState.WearPPE,                               "PASS" },
        { InfantCPRState.CheckConsciousness,                    "PASS" },
        { InfantCPRState.Call119AndRequestAED,                  "PASS" },
        { InfantCPRState.CheckBreathingAndPulse,                "PASS" },
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