using System;
using System.Collections.Generic;

public static class SpinalMotionRestrictionToErrorType
{
    private static readonly Dictionary<SpinalMotionRestrictionState, string> stateToLabel = new Dictionary<SpinalMotionRestrictionState, string>
    {
        { SpinalMotionRestrictionState.EnsureSceneSafety,            "현장 안전 확인" },
        { SpinalMotionRestrictionState.WearPPE,                      "보호장비 착용" },
        { SpinalMotionRestrictionState.PerformLogRoll,               "척추 및 외상확인의 통나무굴리기법 실시" },
        { SpinalMotionRestrictionState.PositionPatientOnSpineBoard,  "긴척추고정판 위치" },
        { SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody, "긴척추고정판 패드" },
        { SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard,    "긴척추고정판 몸통 다리 고정" },
        { SpinalMotionRestrictionState.ApplyHeadImmobilizer,         "척추운동제한의 머리고정대 고정" },
        { SpinalMotionRestrictionState.SecureHands,                  "척추운동제한의 환자 손 고정" },
        { SpinalMotionRestrictionState.AssessPMSOfExtremities,       "척추운동제한의 팔다리 PMS 확인" },
        { SpinalMotionRestrictionState.RecordOnMedicalChart,         "PASS" }
    };

    public static string GetLabel(SpinalMotionRestrictionState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}