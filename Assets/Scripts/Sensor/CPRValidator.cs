using System.Collections.Generic;
using UnityEngine;

public class CPRValidator
{
    public List<float> compressionTimestamps = new();
    private float lastValidPressTime = -1f;

    private const float minPressure = 40f;
    private const float maxPressure = 100f;
    private const int requiredCount = 5;
    private const float minInterval = 0.1f;
    private const float maxInterval = 0.2f;

    // 압력값을 외부에서 가져갈 수 있도록 (예: AEDManager → UI)
    public float LastPressureValue { get; private set; }

    public bool TryAddCompression(float value)
    {
        Debug.Log("TryAddCompression 호출됨");
        float now = Time.time;

        if (value < minPressure || value > maxPressure)
        {
            Debug.Log("❌ 압력 범위 초과");
            return false;
        }

        LastPressureValue = value;

        if (lastValidPressTime > 0f)
        {
            float interval = now - lastValidPressTime;

            if (interval < minInterval)
                Debug.Log($"⚠️ 너무 빠릅니다! 간격: {interval:F2}s");
            else if (interval > maxInterval)
                Debug.Log($"⚠️ 너무 느립니다! 간격: {interval:F2}s");
            else
                Debug.Log($"✅ 템포 적절! 간격: {interval:F2}s");
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
