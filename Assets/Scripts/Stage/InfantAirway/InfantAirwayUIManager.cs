using UnityEngine;

public class InfantAirwayUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, InfantAirwayState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == InfantAirwayState.RecordOnMedicalChart);
    }

    public void SetMessage(InfantAirwayState state)
    {
        messageText.text = AdapterMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is InfantAirwayState infantAirwayState)
        {
            SetMessage(infantAirwayState);
        }
        else
        {
            messageText.text = AdapterMessageManager.GetMessage(state);
            messageText.color = Color.white;
        }
    }

    public void SetProgress(InfantAirwayState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}