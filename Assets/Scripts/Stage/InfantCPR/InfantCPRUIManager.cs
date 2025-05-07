using UnityEngine;

public class InfantCPRUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, InfantCPRState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == InfantCPRState.RecordOnMedicalChart);
    }

    public void SetMessage(InfantCPRState state)
    {
        messageText.text = InfantCPRMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is InfantCPRState infantCPRState)
        {
            SetMessage(infantCPRState);
        }
        else
        {
            messageText.text = "알 수 없는 상태입니다";
            messageText.color = Color.white;
        }
    }

    public void SetProgress(InfantCPRState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}