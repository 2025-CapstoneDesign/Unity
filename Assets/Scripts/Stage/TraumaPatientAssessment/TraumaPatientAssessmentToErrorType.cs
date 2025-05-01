using System;
using System.Collections.Generic;

public static class TraumaPatientAssessmentToErrorType
{
    private static readonly Dictionary<TraumaPatientAssessmentState, string> stateToLabel = new Dictionary<TraumaPatientAssessmentState, string>
    {
        { TraumaPatientAssessmentState.EnsureSceneSafety,    "PASS" },
        { TraumaPatientAssessmentState.WearPPE,              "PASS" },
        { TraumaPatientAssessmentState.StabilizeHeadAndDelegateToAssistant, "PASS" },
        { TraumaPatientAssessmentState.CheckConsciousness,  "PASS" },
        { TraumaPatientAssessmentState.CheckAirwayForObstruction, "PASS" },
        { TraumaPatientAssessmentState.AssessBreathingAndPattern, "PASS" },
        { TraumaPatientAssessmentState.CheckCirculatoryStatus, "PASS" },
        { TraumaPatientAssessmentState.AssessLevelOfConsciousnessUsingAVPU, "PASS" },
        { TraumaPatientAssessmentState.InspectHeadUsingDCAPBLSTIC, "PASS" },
        { TraumaPatientAssessmentState.InspectNeckUsingDCAPBLSTICJVDTD, "PASS" },
        { TraumaPatientAssessmentState.ApplyCervicalCollar, "PASS" },
        { TraumaPatientAssessmentState.ExposeUpperBody, "PASS" },
        { TraumaPatientAssessmentState.InspectChestUsingDCAPBLSTICAndAuscultate, "PASS" },
        { TraumaPatientAssessmentState.InspectAbdomenUsingDCAPBTLS, "PASS" },
        { TraumaPatientAssessmentState.ExposeLowerBody, "PASS" },
        { TraumaPatientAssessmentState.InspectPelvisUsingDCAPBLSTIC, "PASS" },
        { TraumaPatientAssessmentState.InspectExtremitiesUsingDCAPBLSTICPMS, "PASS" },
        { TraumaPatientAssessmentState.PerformLogRoll, "PASS" },
        { TraumaPatientAssessmentState.InspectBackUsingDCAPBLSTIC, "PASS" },
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