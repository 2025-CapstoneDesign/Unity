using UnityEngine;
using TMPro; // 텍스트 쓸거면 필요

public class FeedbackManager : MonoBehaviour
{
    public TMP_Text protocolNameText;
    public TMP_Text durationText;
    public TMP_Text scoreText;
    public TMP_Text feedbackText;

    void Start()
    {
        protocolNameText.text = GameManager.Instance.protocolName;
        durationText.text = GameManager.Instance.duration;
        scoreText.text = "평가점수 - " +  GameManager.Instance.score.ToString() + "점";
        feedbackText.text = GameManager.Instance.feedback;


    }
}
