using UnityEngine;

public class BreathValidator
{
    private IUIManager ui;

    public int breathCount = 0;
    public float breathFlow = 0.0f;

    public const float requiredFlow = 3.0f;
    private const int requiredCount = 2;

    public BreathValidator(IUIManager uiManager)
    {
        ui = uiManager;
    }

    public bool TryAddBreath(float flow)
{
    ui.ShowBreathUI(true);
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
            ui.StartHideBreathUICoroutine(3f); // ✅ 2초 후 UI 자동 숨김
          
            return true;
        }
    }

    return false;
}


    public void Reset()
    {
        breathCount = 0;
    }
}
