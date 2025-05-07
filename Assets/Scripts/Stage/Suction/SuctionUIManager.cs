using UnityEngine;

public class SuctionUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, SuctionState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == SuctionState.RecordOnMedicalChart);
    }

    public void SetMessage(SuctionState state)
    {
        messageText.text = SuctionMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is SuctionState suctionState)
        {
            SetMessage(suctionState);
        }
        else
        {
            messageText.text = "알 수 없는 상태입니다";
            messageText.color = Color.white;
        }
    }

    public void SetProgress(SuctionState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}