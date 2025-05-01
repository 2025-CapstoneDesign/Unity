using System;
using System.Collections.Generic;

public static class SpinalMotionRestrictionToErrorType
{
    private static readonly Dictionary<SpinalMotionRestrictionState, string> stateToLabel = new Dictionary<SpinalMotionRestrictionState, string>
    {
        { SpinalMotionRestrictionState.EnsureSceneSafety,            "PASS" },
        { SpinalMotionRestrictionState.WearPPE,                      "PASS" },
        { SpinalMotionRestrictionState.PerformLogRoll,               "PASS" },
        { SpinalMotionRestrictionState.PositionPatientOnSpineBoard,  "PASS" },
        { SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody, "PASS" },
        { SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard,    "PASS" },
        { SpinalMotionRestrictionState.ApplyHeadImmobilizer,         "PASS" },
        { SpinalMotionRestrictionState.SecureHands,                  "PASS" },
        { SpinalMotionRestrictionState.AssessPMSOfExtremities,       "PASS" },
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