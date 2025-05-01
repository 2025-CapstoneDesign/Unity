using UnityEngine;

public class TraumaPatientAssessmentUIManager : BaseUIManager
{
    public void UpdateTimerUI(TimerManager timerManager, TraumaPatientAssessmentState currentState)
    {
        UpdateTimerUICommon(timerManager, currentState == TraumaPatientAssessmentState.RecordOnMedicalChart);
    }

    public void SetMessage(TraumaPatientAssessmentState state)
    {
        messageText.text = AdapterMessageManager.GetMessage(state);
        messageText.color = Color.white;
    }

    public override void SetMessage(object state)
    {
        if (state is TraumaPatientAssessmentState traumaState)
        {
            SetMessage(traumaState);
        }
        else
        {
            messageText.text = AdapterMessageManager.GetMessage(state);
            messageText.color = Color.white;
        }
    }

    public void SetProgress(TraumaPatientAssessmentState state, int totalSteps)
    {
        base.SetProgress((float)(int)state / totalSteps);
    }
}
