using System.Collections.Generic;
using UnityEngine;

public class CPRValidator
{
    public List<float> compressionTimestamps = new();
    private float lastValidPressTime = -1f;

    private const float minPressure = 40f;
    private const float maxPressure = 100f;
    private const int requiredCount = 5;
    private const float minInterval = 0.1f; // 150bpm
    private const float maxInterval = 0.2f; // 60bpm

    public bool TryAddCompression(float value)
    {
        Debug.Log("TryAddCompression 호출됨");
        float now = Time.time;

        // 압력 값 검증
        if (value < minPressure || value > maxPressure)
        {
            Debug.Log("❌ 압력 범위 초과");
            return false;
        }

        // 템포 확인 (누적은 하되, 안내 메시지 출력용)
        if (lastValidPressTime > 0f)
        {
            float interval = now - lastValidPressTime;

            if (interval < minInterval)
            {
                Debug.Log($"⚠️ 너무 빠릅니다! 간격: {interval:F2}s");
            }
            else if (interval > maxInterval)
            {
                Debug.Log($"⚠️ 너무 느립니다! 간격: {interval:F2}s");
            }
            else
            {
                Debug.Log($"✅ 템포 적절! 간격: {interval:F2}s");
            }
        }

        // 무조건 누적
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
