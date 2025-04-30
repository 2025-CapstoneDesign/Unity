using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StateExtensions
{
    public static string ToVoiceTag(this CPRState state)
    {
        return $"AED_{state}";
    }

    public static string ToVoiceTag(this InfantAirway state)
    {
        return $"InfantAirway_{state}";
    }

    public static string ToVoiceTag(this InfantCPR state)
    {
        return $"InfantCPR_{state}";
    }

    public static string ToVoiceTag(this SpinalMotionRestriction state)
    {
        return $"SpinialMotionRestriction_{state}";
    }

    public static string ToVoiceTag(this Suction state)
    {
        return $"Suction_{state}";
    }

    public static string ToVoiceTag(this TractionSplint state)
    {
        return $"TractionSplint_{state}";
    }

    public static string ToVoiceTag(this TraumaPatientAssessment state)
    {
        return $"TraumaPatientAssessment_{state}";
    }

    public static string ToVoiceTag(this VacuumSplint state)
    {
        return $"VacuumSplint_{state}";
    }
}

