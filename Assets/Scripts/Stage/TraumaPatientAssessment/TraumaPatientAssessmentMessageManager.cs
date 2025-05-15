using System.Collections.Generic;

public static class TraumaPatientAssessmentMessageManager
{
    private static readonly Dictionary<TraumaPatientAssessmentState, string> messages = new Dictionary<TraumaPatientAssessmentState, string>
    {
        { TraumaPatientAssessmentState.EnsureSceneSafety, "현장 안전을 확인하세요." },
        { TraumaPatientAssessmentState.WearPPE, "감염 방지를 위한 개인보호장구를 착용하세요." },
        { TraumaPatientAssessmentState.StabilizeHeadAndDelegateToAssistant, "머리를 안정화하고 보조요원에게 인계하세요." },
        { TraumaPatientAssessmentState.CheckConsciousness, "의식을 확인하세요." },
        { TraumaPatientAssessmentState.CheckAirwayForObstruction, "기도 개방 및 폐쇄 여부를 확인하세요." },
        { TraumaPatientAssessmentState.AssessBreathingAndPattern, "호흡과 양상을 평가하세요." },
        { TraumaPatientAssessmentState.CheckCirculatoryStatus, "순환 상태를 확인하세요." },
        { TraumaPatientAssessmentState.AssessLevelOfConsciousnessUsingAVPU, "의식 수준을 AVPU로 확인하세요." },
        { TraumaPatientAssessmentState.InspectHeadUsingDCAPBLSTIC, "머리를 DCAPBLSTIC으로 검사하세요." },
        { TraumaPatientAssessmentState.InspectNeckUsingDCAPBLSTICJVDTD, "목을 DCAPBLSTIC JVD TD로 검사하세요." },
        { TraumaPatientAssessmentState.ApplyCervicalCollar, "경추 고정대를 적용하세요." },
        { TraumaPatientAssessmentState.ExposeUpperBody, "상체를 노출하세요." },
        { TraumaPatientAssessmentState.InspectChestUsingDCAPBLSTICAndAuscultate, "가슴을 DCAPBLSTIC으로 검사하고 청진하세요." },
        { TraumaPatientAssessmentState.InspectAbdomenUsingDCAPBTLS, "복부를 DCAPBTLS로 검사하세요." },
        { TraumaPatientAssessmentState.ExposeLowerBody, "하체를 노출하세요." },
        { TraumaPatientAssessmentState.InspectPelvisUsingDCAPBLSTIC, "골반을 DCAPBLSTIC으로 검사하세요." },
        { TraumaPatientAssessmentState.InspectExtremitiesUsingDCAPBLSTICPMS, "팔다리를 DCAPBLSTIC PMS로 검사하세요." },
        { TraumaPatientAssessmentState.PerformLogRoll, "통나무굴리기를 실시하세요." },
        { TraumaPatientAssessmentState.InspectBackUsingDCAPBLSTIC, "등을 DCAPBLSTIC으로 검사하세요." },
        { TraumaPatientAssessmentState.RecordOnMedicalChart, "의무기록지에 기록하세요." }
    };

    public static string GetMessage(TraumaPatientAssessmentState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}
