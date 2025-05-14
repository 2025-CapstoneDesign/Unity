using System.Collections.Generic;

public static class SpinalMotionRestrictionMessageManager
{
    private static readonly Dictionary<SpinalMotionRestrictionState, string> messages = new Dictionary<SpinalMotionRestrictionState, string>
    {
        { SpinalMotionRestrictionState.EnsureSceneSafety, "현장 안전을 확인하세요." },
        { SpinalMotionRestrictionState.WearPPE, "감염 방지를 위한 개인보호장구를 착용하세요." },
        { SpinalMotionRestrictionState.PerformLogRoll, "통나무굴리기법을 시행하세요." },
        { SpinalMotionRestrictionState.PositionPatientOnSpineBoard, "긴척추고정판에 환자를 안전하게 위치시키세요." },
        { SpinalMotionRestrictionState.StatePaddingSpaceBetweenBoardAndBody, "\"긴척추고정판과 신체 사이 공간에 패드를 댄다.\"라고 말하세요." },
        { SpinalMotionRestrictionState.SecureTorsoAndLegsToBoard, "몸통과 다리를 고정하세요." },
        { SpinalMotionRestrictionState.ApplyHeadImmobilizer, "머리고정대를 적용하세요." },
        { SpinalMotionRestrictionState.SecureHands, "\"환자의 손을 고정한다.\"라고 말하세요." },
        { SpinalMotionRestrictionState.AssessPMSOfExtremities, "팔다리 PMS를 평가하세요." },
        { SpinalMotionRestrictionState.RecordOnMedicalChart, "의무기록지에 기록하세요." }
    };

    public static string GetMessage(SpinalMotionRestrictionState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}