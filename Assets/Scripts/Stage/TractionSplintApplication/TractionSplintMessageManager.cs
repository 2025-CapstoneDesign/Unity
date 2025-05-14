using System.Collections.Generic;

public static class TractionSplintMessageManager
{
    private static readonly Dictionary<TractionSplintState, string> messages = new Dictionary<TractionSplintState, string>
    {
        { TractionSplintState.EnsureSceneSafety, "현장 안전을 확인하세요." },
        { TractionSplintState.WearPPE, "감염 방지를 위한 개인보호장구를 착용하세요." },
        { TractionSplintState.ExposeAndSupportFracture, "골절부위를 노출하고 지지한 후 보조요원에게 인계하세요." },
        { TractionSplintState.AssessDistalPulseMotorSensation, "손상된 다리 원위부의 맥박, 운동, 감각을 평가하세요." },
        { TractionSplintState.ApplyManualTractionAndDelegate, "통증을 확인하면서 두 손으로 당긴 후 보조요원에게 인계하세요." },
        { TractionSplintState.MeasureSplintLength, "견인부목 길이를 측정하세요." },
        { TractionSplintState.ApplyTractionSplint, "견인부목을 적용하세요." },
        { TractionSplintState.ApplyIschialStrap, "좌골끈을 적용하세요." },
        { TractionSplintState.ApplyAnkleHitch, "발목고정끈을 적용하세요." },
        { TractionSplintState.ConnectAndTightenAnkleTraction, "발목고정끈과 당김고리를 연결하여 당기세요." },
        { TractionSplintState.ApplySupportStraps, "발목고정끈의 당김장태와 모든 고정끈의 조임상태를 손으로 확인하세요." },
        { TractionSplintState.ReassessDistalPMS, "손상된 다리 원위부의 맥박, 운동, 감각을 재평가하세요." },
        { TractionSplintState.StateLogRollTransferToSpineBoard, "\"통나무굴리기법을 이용하여 긴척추고정판에 환자를 옮긴다.\"라고 말하세요." },
        { TractionSplintState.RecordOnMedicalChart, "의무기록지에 기록하세요." }
    };

    public static string GetMessage(TractionSplintState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}