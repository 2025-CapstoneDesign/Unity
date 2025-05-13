using System;
using System.Collections.Generic;

public static class SuctionToErrorType
{
    private static readonly Dictionary<SuctionState, string> stateToLabel = new Dictionary<SuctionState, string>
    {
        { SuctionState.EnsureSceneSafety,                "현장 안전 확인" },
        { SuctionState.WearPPE,                          "보호장비 착용" },
        { SuctionState.CheckEquipmentAndSupplies,        "PASS" },
        { SuctionState.TurnOnSuctionDevice,              "흡인과정의 장비확인" },
        { SuctionState.CheckSuctionPressure,             "흡인과정의 흡인준비" },
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