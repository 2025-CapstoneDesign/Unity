using System.Collections.Generic;

public static class InfantAirwayMessageManager
{
    private static readonly Dictionary<InfantAirwayState, string> messages = new Dictionary<InfantAirwayState, string>
    {
        { InfantAirwayState.EnsureSceneSafety, "현장 안전을 확인하세요." },
        { InfantAirwayState.WearPPE, "감염 방지를 위한 개인보호장구를 착용하세요." },
        { InfantAirwayState.Call119AndRequestAED, "119 신고 및 AED를 요청하세요." },
        { InfantAirwayState.Perform5BackBlows, "등 두드리기를 5회 실시하세요." },
        { InfantAirwayState.Perform5ChestThrusts, "가슴압박을 5회 실시하세요." },
        { InfantAirwayState.RepeatBackBlowsAndChestThrusts, "등 두드리기와 가슴압박을 반복 실시하세요." },
        { InfantAirwayState.IfUnconsciousPlaceSupine, "영아를 바로 누운 자세로 놓으세요." },
        { InfantAirwayState.Perform30ChestCompressions, "가슴압박을 30회 실시하세요." },
        { InfantAirwayState.OpenAirwayAndCheckForObstruction, "기도 개방 및 이물질을 확인하세요." },
        { InfantAirwayState.Perform1RescueBreath, "인공호흡을 1회 실시하세요." },
        { InfantAirwayState.ReopenAirwayAndPerform1RescueBreath, "기도를 재개방 후 인공호흡을 1회 실시하세요." },
        { InfantAirwayState.Perform30To2CPRCycle, "가슴압박과 인공호흡을 실시하세요." },
        { InfantAirwayState.RecordOnMedicalChart, "의무기록지에 기록하세요." }
    };

    public static string GetMessage(InfantAirwayState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}