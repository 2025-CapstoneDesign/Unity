using System.Collections.Generic;

public static class SuctionMessageManager
{
    private static readonly Dictionary<SuctionState, string> messages = new Dictionary<SuctionState, string>
    {
        { SuctionState.EnsureSceneSafety, "수정" },
        { SuctionState.WearPPE, "수정" },
        { SuctionState.CheckEquipmentAndSupplies, "수정" },
        { SuctionState.TurnOnSuctionDevice, "수정" },
        { SuctionState.CheckSuctionPressure, "수정" },
        { SuctionState.TestSuctionWithSaline, "수정" },
        { SuctionState.PerformOralSuction, "수정" },
        { SuctionState.FlushSuctionTipWithSaline, "수정" },
        { SuctionState.TurnOffSuctionDevice, "수정" },
        { SuctionState.AssembleOxygenTankAndRegulator, "수정" },
        { SuctionState.OpenOxygenTankValve, "수정" },
        { SuctionState.CheckForLeaksAndStateNoLeaks, "수정" },
        { SuctionState.CheckOxygenGaugeAndStateRemainingPressure, "수정" },
        { SuctionState.ConnectNonRebreatherMask, "수정" },
        { SuctionState.SetOxygenFlowRate, "수정" },
        { SuctionState.FillReservoirBagAndApplyMask, "수정" },
        { SuctionState.MonitorPatientRespiration, "수정" },
        { SuctionState.RemoveMaskUponInstruction, "수정" },
        { SuctionState.TurnOffFlowMeterAndTank, "수정" },
        { SuctionState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(SuctionState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}