using System.Collections.Generic;

public static class TraumaPatientAssessmentMessageManager
{
    private static readonly Dictionary<TraumaPatientAssessmentState, string> messages = new Dictionary<TraumaPatientAssessmentState, string>
    {
        { TraumaPatientAssessmentState.EnsureSceneSafety, "현장의 안전을 먼저 확인하세요." },
        { TraumaPatientAssessmentState.WearPPE, "감염 방지를 위해 개인 보호 장비(PPE)를 착용하세요." },
        { TraumaPatientAssessmentState.StabilizeHeadAndDelegateToAssistant, "환자의 머리를 고정하고, 보조 요원에게 지시하세요." },
        { TraumaPatientAssessmentState.CheckConsciousness, "환자의 의식을 확인하세요." },
        { TraumaPatientAssessmentState.CheckAirwayForObstruction, "기도가 열려 있는지, 이물질은 없는지 확인하세요." },
        { TraumaPatientAssessmentState.AssessBreathingAndPattern, "호흡의 유무와 양상을 파악하세요." },
        { TraumaPatientAssessmentState.CheckCirculatoryStatus, "순환 상태를 확인하세요 (맥박, 피부색 등)." },
        { TraumaPatientAssessmentState.AssessLevelOfConsciousnessUsingAVPU, "AVPU 기준으로 의식 수준을 평가하세요." },
        { TraumaPatientAssessmentState.InspectHeadUsingDCAPBLSTIC, "머리 부위를 DCAP-BLS, TIC 기준으로 확인하세요." },
        { TraumaPatientAssessmentState.InspectNeckUsingDCAPBLSTICJVDTD, "목 부위를 DCAP-BLS, TIC, JVD, TD 기준으로 확인하세요." },
        { TraumaPatientAssessmentState.ApplyCervicalCollar, "적절한 크기의 경추 보호대를 착용하세요." },
        { TraumaPatientAssessmentState.ExposeUpperBody, "상체를 노출시켜 가슴 부위를 평가할 준비를 하세요." },
        { TraumaPatientAssessmentState.InspectChestUsingDCAPBLSTICAndAuscultate, "가슴 부위를 DCAP-BLS, TIC 기준으로 평가하고 폐음을 청진하세요." },
        { TraumaPatientAssessmentState.InspectAbdomenUsingDCAPBTLS, "복부를 DCAP-BTLS 기준으로 확인하세요." },
        { TraumaPatientAssessmentState.ExposeLowerBody, "하체를 노출시켜 평가할 준비를 하세요." },
        { TraumaPatientAssessmentState.InspectPelvisUsingDCAPBLSTIC, "골반 부위를 DCAP-BLS, TIC 기준으로 확인하세요." },
        { TraumaPatientAssessmentState.InspectExtremitiesUsingDCAPBLSTICPMS, "사지를 DCAP-BLS, TIC, PMS 기준으로 평가하세요." },
        { TraumaPatientAssessmentState.PerformLogRoll, "통나무 굴리기(log roll)를 시행하세요." },
        { TraumaPatientAssessmentState.InspectBackUsingDCAPBLSTIC, "등 부위를 DCAP-BLS, TIC 기준으로 확인하세요." },
        { TraumaPatientAssessmentState.RecordOnMedicalChart, "모든 내용을 의무 기록지에 기록하세요." }
    };

    public static string GetMessage(TraumaPatientAssessmentState state)
    {
        return messages.ContainsKey(state) ? messages[state] : "알 수 없는 상태입니다.";
    }
}
