using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StateExtensions
{
    public static string ToVoiceTag(this CPRState state)
    {
        return $"AED_{state}";
    }

    public static string ToVoiceTag(this InfantAirwayState state)
    {
        return $"InfantAirway_{state}";
    }

    public static string ToVoiceTag(this InfantCPRState state)
    {
        return $"InfantCPR_{state}";
    }

    public static string ToVoiceTag(this SpinalMotionRestrictionState state)
    {
        return $"SpinialMotionRestriction_{state}";
    }

    public static string ToVoiceTag(this SuctionState state)
    {
        return $"Suction_{state}";
    }

    public static string ToVoiceTag(this TractionSplintState state)
    {
        return $"TractionSplint_{state}";
    }

    public static string ToVoiceTag(this TraumaPatientAssessmentState state)
    {
        return $"TraumaPatientAssessment_{state}";
    }

    public static string ToVoiceTag(this VacuumSplintState state)
    {
        return $"VacuumSplint_{state}";
    }
}

