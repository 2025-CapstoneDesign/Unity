using System.Collections.Generic;

public static class VacuumSplintMessageManager
{
    private static readonly Dictionary<VacuumSplintState, string> messages = new Dictionary<VacuumSplintState, string>
    {
        { VacuumSplintState.EnsureSceneSafety, "현장안전을 확인한다." },
        { VacuumSplintState.WearPPE, "감염방지를 위해 개인보호장구를 착용한다." },
        { VacuumSplintState.ExposeAndSupportFracture, "골절부위를 노출하고 지지한 후 보조요원에게 인계한다." },
        { VacuumSplintState.AssessDistalPMS, "손상된 팔 원위부의 맥박, 운동, 감각을 평가한다." },
        { VacuumSplintState.MeasureSplintSize, "부목의 길이를 측정한다." },
        { VacuumSplintState.ApplySplintToInjury, "손상된 팔에 부목을 적용한다." },
        { VacuumSplintState.AttachVacuumPumpAndEvacuateAir, "진공펌프를 연결하고 공기를 제거한다." },
        { VacuumSplintState.ReSecureSplintStraps, "부목 고정끈을 다시 고정한다." },
        { VacuumSplintState.SecureArmToBody, "손상된 팔을 몸에 고정한다." },
        { VacuumSplintState.ReassessDistalPMS, "손상된 팔 원위부의 맥박, 운동, 감각을 재평가한다." },
        { VacuumSplintState.RecordOnMedicalChart, "의무기록지에 기록한다." }
    };

    public static string GetMessage(VacuumSplintState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}