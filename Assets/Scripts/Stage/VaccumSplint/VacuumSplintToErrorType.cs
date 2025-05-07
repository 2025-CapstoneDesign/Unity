using System;
using System.Collections.Generic;

public static class VacuumSplintToErrorType
{
    private static readonly Dictionary<VacuumSplintState, string> stateToLabel = new Dictionary<VacuumSplintState, string>
    {
        { VacuumSplintState.EnsureSceneSafety,           "현장 안전 확인" },
        { VacuumSplintState.WearPPE,                     "개인보호장비 착용" },
        { VacuumSplintState.ExposeAndSupportFracture,    "손상 부위 노출 및 지지" },
        { VacuumSplintState.AssessDistalPMS,             "원위부 맥박/운동/감각 평가" },
        { VacuumSplintState.MeasureSplintSize,           "부목 크기 측정" },
        { VacuumSplintState.ApplySplintToInjury,         "부목 적용" },
        { VacuumSplintState.AttachVacuumPumpAndEvacuateAir, "진공펌프 연결 및 공기 제거" },
        { VacuumSplintState.ReSecureSplintStraps,        "부목 스트랩 재고정" },
        { VacuumSplintState.SecureArmToBody,             "팔을 몸에 고정" },
        { VacuumSplintState.ReassessDistalPMS,           "원위부 재평가" },
        { VacuumSplintState.RecordOnMedicalChart,        "의료기록지 작성" }
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