using System;
using System.Collections.Generic;

public static class TractionSplintToErrorType
{
    private static readonly Dictionary<TractionSplintState, string> stateToLabel = new Dictionary<TractionSplintState, string>
    {
        { TractionSplintState.EnsureSceneSafety,             "현장 안전 확인" },
        { TractionSplintState.WearPPE,                       "보호장비 착용" },
        { TractionSplintState.ExposeAndSupportFracture,      "PASS" },
        { TractionSplintState.AssessDistalPulseMotorSensation, "PASS" },
        { TractionSplintState.ApplyManualTractionAndDelegate, "견인부목에서의 통증 확인 및 인계" },
        { TractionSplintState.MeasureSplintLength,           "PASS" },
        { TractionSplintState.ApplyTractionSplint,           "PASS" },
        { TractionSplintState.ApplyIschialStrap,             "PASS" },
        { TractionSplintState.ApplyAnkleHitch,               "PASS" },
        { TractionSplintState.ConnectAndTightenAnkleTraction, "PASS" },
        { TractionSplintState.ApplySupportStraps,            "PASS" },
        { TractionSplintState.ReassessDistalPMS,             "PASS" },
        { TractionSplintState.StateLogRollTransferToSpineBoard, "긴척추의 통나무굴리기법" },
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