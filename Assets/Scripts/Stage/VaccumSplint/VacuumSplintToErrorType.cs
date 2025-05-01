using System;
using System.Collections.Generic;

public static class VacuumSplintToErrorType
{
    private static readonly Dictionary<VacuumSplintState, string> stateToLabel = new Dictionary<VacuumSplintState, string>
    {
        { VacuumSplintState.EnsureSceneSafety,            "PASS" },
        { VacuumSplintState.WearPPE,                      "PASS" },
        { VacuumSplintState.ExposeAndSupportFracture,     "PASS" },
        { VacuumSplintState.AssessDistalPMS,              "PASS" },
        { VacuumSplintState.MeasureSplintSize,            "PASS" },
        { VacuumSplintState.ApplySplintToInjury,          "PASS" },
        { VacuumSplintState.AttachVacuumPumpAndEvacuateAir, "PASS" },
        { VacuumSplintState.ReSecureSplintStraps,         "PASS" },
        { VacuumSplintState.SecureArmToBody,              "PASS" },
        { VacuumSplintState.ReassessDistalPMS,            "PASS" },
        { VacuumSplintState.RecordOnMedicalChart,         "PASS" }
    };

    public static string GetLabel(VacuumSplintState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}