using System.Collections.Generic;

public static class TractionSplintMessageManager
{
    private static readonly Dictionary<TractionSplintState, string> messages = new Dictionary<TractionSplintState, string>
    {
        { TractionSplintState.EnsureSceneSafety, "수정" },
        { TractionSplintState.WearPPE, "수정" },
        { TractionSplintState.ExposeAndSupportFracture, "수정" },
        { TractionSplintState.AssessDistalPulseMotorSensation, "수정" },
        { TractionSplintState.ApplyManualTractionAndDelegate, "수정" },
        { TractionSplintState.MeasureSplintLength, "수정" },
        { TractionSplintState.ApplyTractionSplint, "수정" },
        { TractionSplintState.ApplyIschialStrap, "수정" },
        { TractionSplintState.ApplyAnkleHitch, "수정" },
        { TractionSplintState.ConnectAndTightenAnkleTraction, "수정" },
        { TractionSplintState.ApplySupportStraps, "수정" },
        { TractionSplintState.ReassessDistalPMS, "수정" },
        { TractionSplintState.StateLogRollTransferToSpineBoard, "수정" },
        { TractionSplintState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(TractionSplintState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}