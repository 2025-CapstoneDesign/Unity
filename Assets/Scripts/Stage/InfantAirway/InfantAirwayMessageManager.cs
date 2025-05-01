using System.Collections.Generic;

public static class InfantAirwayMessageManager
{
    private static readonly Dictionary<InfantAirwayState, string> messages = new Dictionary<InfantAirwayState, string>
    {
        { InfantAirwayState.EnsureSceneSafety, "수정" },
        { InfantAirwayState.WearPPE, "수정" },
        { InfantAirwayState.Call119AndRequestAED, "수정" },
        { InfantAirwayState.Perform5BackBlows, "수정" },
        { InfantAirwayState.Perform5ChestThrusts, "수정" },
        { InfantAirwayState.RepeatBackBlowsAndChestThrusts, "수정" },
        { InfantAirwayState.IfUnconsciousPlaceSupine, "수정" },
        { InfantAirwayState.Perform30ChestCompressions, "수정" },
        { InfantAirwayState.OpenAirwayAndCheckForObstruction, "수정" },
        { InfantAirwayState.Perform1RescueBreath, "수정" },
        { InfantAirwayState.ReopenAirwayAndPerform1RescueBreath, "수정" },
        { InfantAirwayState.Perform30To2CPRCycle, "수정" },
        { InfantAirwayState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(InfantAirwayState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}