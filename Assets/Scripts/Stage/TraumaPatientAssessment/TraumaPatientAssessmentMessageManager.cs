using System.Collections.Generic;

public static class TraumaPatientAssessmentMessageManager
{
    private static readonly Dictionary<TraumaPatientAssessmentState, string> messages = new Dictionary<TraumaPatientAssessmentState, string>
    {
        { TraumaPatientAssessmentState.EnsureSceneSafety, "수정" },
        { TraumaPatientAssessmentState.WearPPE, "수정" },
        { TraumaPatientAssessmentState.StabilizeHeadAndDelegateToAssistant, "수정" },
        { TraumaPatientAssessmentState.CheckConsciousness, "수정" },
        { TraumaPatientAssessmentState.CheckAirwayForObstruction, "수정" },
        { TraumaPatientAssessmentState.AssessBreathingAndPattern, "수정" },
        { TraumaPatientAssessmentState.CheckCirculatoryStatus, "수정" },
        { TraumaPatientAssessmentState.AssessLevelOfConsciousnessUsingAVPU, "수정" },
        { TraumaPatientAssessmentState.InspectHeadUsingDCAPBLSTIC, "수정" },
        { TraumaPatientAssessmentState.InspectNeckUsingDCAPBLSTICJVDTD, "수정" },
        { TraumaPatientAssessmentState.ApplyCervicalCollar, "수정" },
        { TraumaPatientAssessmentState.ExposeUpperBody, "수정" },
        { TraumaPatientAssessmentState.InspectChestUsingDCAPBLSTICAndAuscultate, "수정" },
        { TraumaPatientAssessmentState.InspectAbdomenUsingDCAPBTLS, "수정" },
        { TraumaPatientAssessmentState.ExposeLowerBody, "수정" },
        { TraumaPatientAssessmentState.InspectPelvisUsingDCAPBLSTIC, "수정" },
        { TraumaPatientAssessmentState.InspectExtremitiesUsingDCAPBLSTICPMS, "수정" },
        { TraumaPatientAssessmentState.PerformLogRoll, "수정" },
        { TraumaPatientAssessmentState.InspectBackUsingDCAPBLSTIC, "수정" },
        { TraumaPatientAssessmentState.RecordOnMedicalChart, "수정" }
    };

    public static string GetMessage(TraumaPatientAssessmentState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}