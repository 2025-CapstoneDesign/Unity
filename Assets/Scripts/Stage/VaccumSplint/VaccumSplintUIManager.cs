using UnityEngine;

public class VaccumSplintUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, VacuumSplintState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == VacuumSplintState.RecordOnMedicalChart);
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

    public void SetProgress(CPRState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}
