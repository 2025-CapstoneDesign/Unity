using System.Collections.Generic;
using UnityEngine;

public class CPRValidator
{
    private IUIManager ui;
    public List<float> compressionTimestamps = new();
    private float lastValidPressTime = -1f;

    public float minPressure = 40f;
    public float maxPressure = 100f;
    private int requiredCount = 5;
    private float minInterval = 0.1f;
    private float maxInterval = 0.2f;

    public float LastPressureValue { get; private set; }

    public CPRValidator(IUIManager uiManager, string type)
    {
        if (type == "Infant")
        {
            minPressure = 80f;
            maxPressure = 200f;
            requiredCount = 5;
            minInterval = 0.1f;
            maxInterval = 0.2f;
        }
        else
        {
            minPressure = 880f;
            maxPressure = 1100f;
            requiredCount = 5;
            minInterval = 0.1f;
            maxInterval = 0.2f;
        }
        ui = uiManager;
    }

    public bool TryAddCompression(float value)
{
    float now = Time.time;

    if (value < minPressure || value > maxPressure)
        return false;

    LastPressureValue = value;
    ui.SwitchToCompressionUI();

        if (lastValidPressTime > 0f)
    {
        float interval = now - lastValidPressTime;
        if (interval >= minInterval && interval <= maxInterval)
            ui.UpdateCountText($"템포 적절! {compressionTimestamps.Count + 1}회");
    }
    ui.ShowCountText(true);
    lastValidPressTime = now;
    compressionTimestamps.Add(now);

    ui.UpdateCountText($"가습압박 : {compressionTimestamps.Count}회");
    ui.SetCompressionForce(value);

    // ✅ 압박이 모두 끝났다면 UI를 숨기도록 처리
    if (compressionTimestamps.Count >= requiredCount)
    {
        Debug.Log($"💪 압박 완료");
        ui.ShowCountText(false);
        return true;
    }

    return false;
}


    public void Reset()
    {
        compressionTimestamps.Clear();
        lastValidPressTime = -1f;
    }

    public int GetCompressionCount()
    {
        return compressionTimestamps.Count;
    }

    public float GetLastPressureValue()
    {
        return LastPressureValue;
    }

    public float GetLastValidPressTime()
    {
        return lastValidPressTime;
    }

    public float GetMinPressure()
    {
        return minPressure;
    }

    public float GetMaxPressure()
    {
        return maxPressure;
    }

    public float GetMinInterval()
    {
        return minInterval;
    }

    public float GetMaxInterval()
    {
        return maxInterval;
    }

    public int GetRequiredCount()
    {
        return requiredCount;
    }
}
