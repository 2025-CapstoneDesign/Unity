using UnityEngine;

public struct MarkerData
{
    public Vector3 position;
    public Quaternion rotation;

    public MarkerData(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}
