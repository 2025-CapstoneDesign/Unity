using UnityEngine;

public class TractionSplintUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, TractionSplintState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == TractionSplintState.RecordOnMedicalChart);
    }

    public void SetMessage(TractionSplintState state)
    {
        messageText.text = TractionSplintMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is TractionSplintState splintState)
        {
            SetMessage(splintState);
        }
        else
        {
            messageText.text = TractionSplintMessageManager.GetMessage((TractionSplintState)state);
            messageText.color = Color.white;
        }
    }

    public void SetProgress(TractionSplintState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}