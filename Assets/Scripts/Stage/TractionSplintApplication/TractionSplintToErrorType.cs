using System;
using System.Collections.Generic;

public static class TractionSplintToErrorType
{
    private static readonly Dictionary<TractionSplintState, string> stateToLabel = new Dictionary<TractionSplintState, string>
    {
        { TractionSplintState.EnsureSceneSafety,             "PASS" },
        { TractionSplintState.WearPPE,                       "PASS" },
        { TractionSplintState.ExposeAndSupportFracture,      "PASS" },
        { TractionSplintState.AssessDistalPulseMotorSensation, "PASS" },
        { TractionSplintState.ApplyManualTractionAndDelegate, "PASS" },
        { TractionSplintState.MeasureSplintLength,           "PASS" },
        { TractionSplintState.ApplyTractionSplint,           "PASS" },
        { TractionSplintState.ApplyIschialStrap,             "PASS" },
        { TractionSplintState.ApplyAnkleHitch,               "PASS" },
        { TractionSplintState.ConnectAndTightenAnkleTraction, "PASS" },
        { TractionSplintState.ApplySupportStraps,            "PASS" },
        { TractionSplintState.ReassessDistalPMS,             "PASS" },
        { TractionSplintState.StateLogRollTransferToSpineBoard, "PASS" },
        { TractionSplintState.RecordOnMedicalChart,          "PASS" }
    };

    public static string GetLabel(TractionSplintState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}