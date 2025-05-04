using UnityEngine;

public class SpinialMotionRestrictionUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, SpinalMotionRestrictionState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == SpinalMotionRestrictionState.RecordOnMedicalChart);
    }

    public void SetMessage(SpinalMotionRestrictionState state)
    {
        messageText.text = SpinalMotionRestrictionMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is SpinalMotionRestrictionState spinalState)
        {
            SetMessage(spinalState);
        }
        else
        {
            messageText.text = SpinalMotionRestrictionMessageManager.GetMessage((SpinalMotionRestrictionState)state);
            messageText.color = Color.white;
        }
    }

    public void SetProgress(SpinalMotionRestrictionState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}