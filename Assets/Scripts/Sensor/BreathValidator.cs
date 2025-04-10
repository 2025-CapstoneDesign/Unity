using UnityEngine;

public class BreathValidator
{
    private int breathCount = 0;

    private const float requiredFlow = 3.0f;    // 성공 기준 유량
    private const int requiredCount = 2;        // 2회 인공호흡

    public bool TryAddBreath(float flow)
    {
        if (flow >= requiredFlow)
        {
            breathCount++;
            Debug.Log($"🌬 인공호흡 누적: {breathCount}/{requiredCount}");

            if (breathCount >= requiredCount)
                return true;
        }

        return false;
    }

    public void Reset()
    {
        breathCount = 0;
    }
}
