using System;
using System.Collections.Generic;

public static class TraumaPatientAssessmentToErrorType
{
    private static readonly Dictionary<TraumaPatientAssessmentState, string> stateToLabel = new Dictionary<TraumaPatientAssessmentState, string>
    {
        { TraumaPatientAssessmentState.EnsureSceneSafety,    "현장 안전 확인" },
        { TraumaPatientAssessmentState.WearPPE,              "보호장비 착용" },
        { TraumaPatientAssessmentState.StabilizeHeadAndDelegateToAssistant, "PASS" },
        { TraumaPatientAssessmentState.CheckConsciousness,  "의식 확인" },
        { TraumaPatientAssessmentState.CheckAirwayForObstruction, "PASS" },
        { TraumaPatientAssessmentState.AssessBreathingAndPattern, "PASS" },
        { TraumaPatientAssessmentState.CheckCirculatoryStatus, "PASS" },
        { TraumaPatientAssessmentState.AssessLevelOfConsciousnessUsingAVPU, "AVPU 확인" },
        { TraumaPatientAssessmentState.InspectHeadUsingDCAPBLSTIC, "머리의 DCAP, BLS, TIC 확인" },
        { TraumaPatientAssessmentState.InspectNeckUsingDCAPBLSTICJVDTD, "목의 DCAP, BLS, TIC 및 JVD, TD 확인" },
        { TraumaPatientAssessmentState.ApplyCervicalCollar, "PASS" },
        { TraumaPatientAssessmentState.ExposeUpperBody, "상의 제거" },
        { TraumaPatientAssessmentState.InspectChestUsingDCAPBLSTICAndAuscultate, "가슴의 DCAP, BLS, TIC확인" },
        { TraumaPatientAssessmentState.InspectAbdomenUsingDCAPBTLS, "외상환자평가의 복부 확인" },
        { TraumaPatientAssessmentState.ExposeLowerBody, "하의 제거" },
        { TraumaPatientAssessmentState.InspectPelvisUsingDCAPBLSTIC, "외상환자평가의 골반 확인" },
        { TraumaPatientAssessmentState.InspectExtremitiesUsingDCAPBLSTICPMS, "외상환자평가의 사지 PMS 확인" },
        { TraumaPatientAssessmentState.PerformLogRoll, "PASS" },
        { TraumaPatientAssessmentState.InspectBackUsingDCAPBLSTIC, "외상환자평가의 등 확인" },
        { TraumaPatientAssessmentState.RecordOnMedicalChart, "PASS" }
    };

    public static string GetLabel(TraumaPatientAssessmentState enumName)
    {
        if (stateToLabel.TryGetValue(enumName, out string label))
        {
            return label;
        }
        return "PASS";
    }
}