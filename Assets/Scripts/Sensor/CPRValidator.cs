using System.Collections.Generic;
using UnityEngine;

public class CPRValidator
{
    private List<float> compressionTimestamps = new();
    private float lastValidPressTime = -1f;

    private const float minPressure = 40f;
    private const float maxPressure = 100f;
    private const int requiredCount = 5;
    private const float minInterval = 0.1f; // 150bpm
    private const float maxInterval = 0.2f; // 60bpm

    public bool TryAddCompression(float value)
    {
        float now = Time.time;

        if (value < minPressure || value > maxPressure)
            return false;

        if (lastValidPressTime > 0f)
        {
            float interval = now - lastValidPressTime;
            if (interval < minInterval || interval > maxInterval)
                return false; // 템포 맞지 않음
        }

        lastValidPressTime = now;
        compressionTimestamps.Add(now);

        Debug.Log($"🫀 압박 기록됨: {compressionTimestamps.Count}회");

        return compressionTimestamps.Count >= requiredCount;
    }

    public void Reset()
    {
        compressionTimestamps.Clear();
        lastValidPressTime = -1f;
    }
}
