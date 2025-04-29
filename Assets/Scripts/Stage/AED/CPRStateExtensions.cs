using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CPRStateExtensions
{
    public static string ToVoiceTag(this CPRState state)
    {
        return $"AED_{state}";
    }
}

