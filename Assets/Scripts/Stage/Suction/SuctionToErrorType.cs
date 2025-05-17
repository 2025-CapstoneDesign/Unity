using System;
using System.Collections.Generic;

public static class SuctionToErrorType
{
    private static readonly Dictionary<SuctionState, string> stateToLabel = new Dictionary<SuctionState, string>
    {
        { SuctionState.EnsureSceneSafety,                "현장 안전 확인" },
        { SuctionState.WearPPE,                          "보호장비 착용" },
        { SuctionState.CheckEquipmentAndSupplies,        "흡인과정의 장비확인" },
        { SuctionState.TurnOnSuctionDevice,              "흡인과정의 흡인기 가동" },
        { SuctionState.CheckSuctionPressure,             "흡인과정의 흡인 압력 확인" },
        { SuctionState.TestSuctionWithSaline,            "PASS" },
        { SuctionState.PerformOralSuction,               "흡인과정의 흡인 시행" },
        { SuctionState.FlushSuctionTipWithSaline,        "흡인과정의 흡인관 세척" },
        { SuctionState.TurnOffSuctionDevice,             "흡인과정의 흡인 종료" },
        { SuctionState.AssembleOxygenTankAndRegulator,   "산소 투여 과정의 압력조절기 조립" },
        { SuctionState.OpenOxygenTankValve,              "산소 투여 과정의 산소 개방" },
        { SuctionState.CheckForLeaksAndStateNoLeaks,     "산소 투여 과정의 산소 압력 확인" },
        { SuctionState.CheckOxygenGaugeAndStateRemainingPressure, "PASS" },
        { SuctionState.ConnectNonRebreatherMask,         "산소 투여 과정의 마스크 연결" },
        { SuctionState.SetOxygenFlowRate,                "산소 투여 과정의 산소 유량조절" },
        { SuctionState.FillReservoirBagAndApplyMask,     "산소 투여 과정의 마스크 끈 재조정" },
        { SuctionState.MonitorPatientRespiration,        "PASS" },
        { SuctionState.RemoveMaskUponInstruction,        "산소 공급 마무리" },
        { SuctionState.TurnOffFlowMeterAndTank,          "유량계 잠금" },
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