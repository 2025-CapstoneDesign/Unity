using System;
using System.Collections.Generic;

public static class VacuumSplintToErrorType
{
    private static readonly Dictionary<VacuumSplintState, string> stateToLabel = new Dictionary<VacuumSplintState, string>
    {
        { VacuumSplintState.EnsureSceneSafety,           "현장 안전 확인" },
        { VacuumSplintState.WearPPE,                     "보호장비 착용" },
        { VacuumSplintState.ExposeAndSupportFracture,    "골절부위 노출 및 인계" },
        { VacuumSplintState.AssessDistalPMS,             "진공부목에서의 통증 여부 질의" },
        { VacuumSplintState.MeasureSplintSize,           "부목 길이 측정" },
        { VacuumSplintState.ApplySplintToInjury,         "진공부목에서의 붕대 적용" },
        { VacuumSplintState.AttachVacuumPumpAndEvacuateAir, "진공부목에서의 공기제거" },
        { VacuumSplintState.ReSecureSplintStraps,        "진공부목에서의 고정끈 재조정" },
        { VacuumSplintState.SecureArmToBody,             "PASS" },
        { VacuumSplintState.ReassessDistalPMS,           "진공부목에서의 통증 여부 질의" },
        { VacuumSplintState.RecordOnMedicalChart,        "PASS" }
    };

    public static string GetLabel(VacuumSplintState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return enumName.ToString();
    }
}