using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AEDUIManager : BaseUIManager
{
    [SerializeField] private TextMeshProUGUI cycleText; // Inspector에서 할당
    public void UpdateTimerUI(TimerManager timerManager, CPRState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == CPRState.Completed);
    }

    public void SetMessage(CPRState state)
    {
        messageText.text = AdapterMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is CPRState cprState)
        {
            SetMessage(cprState);
        }
        else
        {
            messageText.text = AdapterMessageManager.GetMessage(state);
            messageText.color = Color.white;
        }
    }

    public void SetCycleCount(int cycle)
    {
        if (cycleText != null)
        {
            cycleText.text = $"현재 주기: {cycle}/5";
        }
    }

    public void ShowCycleText(bool isShow)
    {
        if (cycleText != null)
        {
            cycleText.gameObject.SetActive(isShow);
        }
    }

    public void SetProgress(CPRState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}
