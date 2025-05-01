using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class InfantAirwayToErrorType
{
    private static readonly Dictionary<InfantAirwayState, string> stateToLabel = new Dictionary<InfantAirwayState, string>
    {
        { InfantAirwayState.EnsureSceneSafety,                  "PASS" },
        { InfantAirwayState.WearPPE,                            "PASS" },
        { InfantAirwayState.Call119AndRequestAED,               "PASS" },
        { InfantAirwayState.Perform5BackBlows,                  "PASS" },
        { InfantAirwayState.Perform5ChestThrusts,               "PASS" },
        { InfantAirwayState.RepeatBackBlowsAndChestThrusts,     "PASS" },
        { InfantAirwayState.IfUnconsciousPlaceSupine,           "PASS" },
        { InfantAirwayState.Perform30ChestCompressions,         "PASS" },
        { InfantAirwayState.OpenAirwayAndCheckForObstruction,   "PASS" },
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
