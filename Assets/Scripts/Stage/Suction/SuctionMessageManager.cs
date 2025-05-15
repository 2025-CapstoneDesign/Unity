using System.Collections.Generic;

public static class SuctionMessageManager
{
    private static readonly Dictionary<SuctionState, string> messages = new Dictionary<SuctionState, string>
    {
        { SuctionState.EnsureSceneSafety, "현장 안전을 확인한다." },
        { SuctionState.WearPPE, "감염 방지를 위한 개인보호장구를 착용한다." },
        { SuctionState.CheckEquipmentAndSupplies, "장비 및 물품을 점검한다." },
        { SuctionState.TurnOnSuctionDevice, "흡인기 전원을 켠다." },
        { SuctionState.CheckSuctionPressure, "흡인압력을 확인한다." },
        { SuctionState.TestSuctionWithSaline, "흡인기 작동을 생리식염수에 넣어 시험한다." },
        { SuctionState.PerformOralSuction, "환자 입안에 흡인팁을 삽입하고 흡인을 시행한다." },
        { SuctionState.FlushSuctionTipWithSaline, "흡인팁을 생리식염수에 넣어 세척한다." },
        { SuctionState.TurnOffSuctionDevice, "흡인기 전원을 끈다." },
        { SuctionState.AssembleOxygenTankAndRegulator, "산소탱크와 압력조절기를 조립한다." },
        { SuctionState.OpenOxygenTankValve, "산소탱크 개방밸브를 연다." },
        { SuctionState.CheckForLeaksAndStateNoLeaks, "산소가 새는지 확인하고 \"산소가 새지 않음.\" 이라고 말한다." },
        { SuctionState.CheckOxygenGaugeAndStateRemainingPressure, "산소 압력계를 보고 산소압을 말한다." },
        { SuctionState.ConnectNonRebreatherMask, "비재호흡마스크를 저장주머니에 연결한다." },
        { SuctionState.SetOxygenFlowRate, "유량계를 조절한다." },
        { SuctionState.FillReservoirBagAndApplyMask, "산소탱크의 산소로 저장주머니를 채운 후 환자에게 적용한다." },
        { SuctionState.MonitorPatientRespiration, "환자의 호흡 상태를 확인한다." },
        { SuctionState.RemoveMaskUponInstruction, "환자 입안에 흡인팁을 삽입하고 흡인을 시행한다." },
        { SuctionState.TurnOffFlowMeterAndTank, "유량계를 마스고 산소탱크 개방밸브를 닫는다." },
        { SuctionState.RecordOnMedicalChart, "의무기록지에 기록한다." }
    };

    public static string GetMessage(SuctionState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}