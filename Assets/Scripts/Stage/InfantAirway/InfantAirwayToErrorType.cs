using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class InfantAirwayToErrorType
{
    private static readonly Dictionary<InfantAirwayState, string> stateToLabel = new Dictionary<InfantAirwayState, string>
    {
        { InfantAirwayState.EnsureSceneSafety,                  "현장 안전 확인" },
        { InfantAirwayState.WearPPE,                            "보호장비 착용" },
        { InfantAirwayState.Call119AndRequestAED,               "AED 요청" },
        { InfantAirwayState.Perform5BackBlows,                  "PASS" },
        { InfantAirwayState.Perform5ChestThrusts,               "PASS" },
        { InfantAirwayState.RepeatBackBlowsAndChestThrusts,     "PASS" },
        { InfantAirwayState.IfUnconsciousPlaceSupine,           "PASS" },
        { InfantAirwayState.Perform30ChestCompressions,         "PASS" },
        { InfantAirwayState.OpenAirwayAndCheckForObstruction,   "영아 심폐 소생술의 기도 개방" },
        { InfantAirwayState.Perform1RescueBreath,               "PASS" },
        { InfantAirwayState.ReopenAirwayAndPerform1RescueBreath, "PASS" },
        { InfantAirwayState.Perform30To2CPRCycle,               "PASS" },
        { InfantAirwayState.RecordOnMedicalChart,               "PASS" }
    };

    public static string GetLabel(InfantAirwayState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}
