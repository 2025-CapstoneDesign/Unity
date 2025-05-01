using System;
using System.Collections.Generic;

public static class SuctionToErrorType
{
    private static readonly Dictionary<SuctionState, string> stateToLabel = new Dictionary<SuctionState, string>
    {
        { SuctionState.EnsureSceneSafety,                "PASS" },
        { SuctionState.WearPPE,                          "PASS" },
        { SuctionState.CheckEquipmentAndSupplies,        "PASS" },
        { SuctionState.TurnOnSuctionDevice,              "PASS" },
        { SuctionState.CheckSuctionPressure,             "PASS" },
        { SuctionState.TestSuctionWithSaline,            "PASS" },
        { SuctionState.PerformOralSuction,               "PASS" },
        { SuctionState.FlushSuctionTipWithSaline,        "PASS" },
        { SuctionState.TurnOffSuctionDevice,             "PASS" },
        { SuctionState.AssembleOxygenTankAndRegulator,   "PASS" },
        { SuctionState.OpenOxygenTankValve,              "PASS" },
        { SuctionState.CheckForLeaksAndStateNoLeaks,     "PASS" },
        { SuctionState.CheckOxygenGaugeAndStateRemainingPressure, "PASS" },
        { SuctionState.ConnectNonRebreatherMask,         "PASS" },
        { SuctionState.SetOxygenFlowRate,                "PASS" },
        { SuctionState.FillReservoirBagAndApplyMask,     "PASS" },
        { SuctionState.MonitorPatientRespiration,        "PASS" },
        { SuctionState.RemoveMaskUponInstruction,        "PASS" },
        { SuctionState.TurnOffFlowMeterAndTank,          "PASS" },
        { SuctionState.RecordOnMedicalChart,             "PASS" }
    };

    public static string GetLabel(SuctionState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}