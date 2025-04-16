using System.Collections.Generic;
using UnityEngine;

public enum MarkerOrientation
{
    Floor,
    Wall
}

public static class MarkerOrientationMap
{
    public static Dictionary<int, MarkerOrientation> markerOrientationMap = new Dictionary<int, MarkerOrientation>
    {
        { 0, MarkerOrientation.Floor },
        { 1, MarkerOrientation.Wall }
    };
}
