using System.Collections.Generic;

public static class VacuumSplintMessageManager
{
    private static readonly Dictionary<VacuumSplintState, string> messages = new Dictionary<VacuumSplintState, string>
    {
        { VacuumSplintState.EnsureSceneSafety, "수정" },
        { VacuumSplintState.WearPPE, "수정" },
        { VacuumSplintState.ExposeAndSupportFracture, "수정" },
        { VacuumSplintState.AssessDistalPMS, "수정" },
        { VacuumSplintState.MeasureSplintSize, "수정" },
        { VacuumSplintState.ApplySplintToInjury, "수정" },
        { VacuumSplintState.AttachVacuumPumpAndEvacuateAir, "수정" },
        { VacuumSplintState.ReSecureSplintStraps, "수정" },
        { VacuumSplintState.SecureArmToBody, "수정" },
        { VacuumSplintState.ReassessDistalPMS, "수정" },
        { VacuumSplintState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(VacuumSplintState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}