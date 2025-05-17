using UnityEngine;

public class BreathValidator
{
    private IUIManager ui;

    public int breathCount = 0;
    public float breathFlow = 0.0f;

    public float requiredFlow = 3.0f;
    private int requiredCount = 2;

    public BreathValidator(IUIManager uiManager, string type)
    {
        if (type == "Infant")
        {
            requiredFlow = 10f;
            requiredCount = 2;
        }
        else
        {
            requiredFlow = 25f;
            requiredCount = 2;
        }
        ui = uiManager;
    }

    public bool TryAddBreath(float flow)
    {
        ui.SwitchToBreathUI();
        breathFlow = flow;
        ui.SetBreathForce(flow);

        if (flow >= requiredFlow)
        {
            breathCount++;
            ui.ShowCountText(true);
            ui.UpdateCountText($"인공호흡 : {breathCount}회");
            Debug.Log($"🌬 인공호흡 누적: {breathCount}/{requiredCount}");

            if (breathCount >= requiredCount)
            {
                return true;
            }
        }

        return false;
    }


    public void Reset()
    {
        breathCount = 0;
    }

    public int getBreathCount()
    {
        return breathCount;
    }

    public float getBreathFlow()
    {
        return breathFlow;
    }
}
