using System.Collections.Generic;

public static class SpinalMotionRestrictionMessageManager
{
    private static readonly Dictionary<SpinalMotionRestrictionState, string> messages = new Dictionary<SpinalMotionRestrictionState, string>
    {
        { SpinalMotionRestrictionState.EnsureSceneSafety, "수정" },
        { SpinalMotionRestrictionState.WearPPE, "수정" },
        { SpinalMotionRestrictionState.PerformLogRoll, "수정" },
        { SpinalMotionRestrictionState.PositionPatientOnSpineBoard, "수정" },
        { SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody, "수정" },
        { SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard, "수정" },
        { SpinalMotionRestrictionState.ApplyHeadImmobilizer, "수정" },
        { SpinalMotionRestrictionState.SecureHands, "수정" },
        { SpinalMotionRestrictionState.AssessPMSOfExtremities, "수정" },
        { SpinalMotionRestrictionState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(SpinalMotionRestrictionState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}