using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // TextMeshPro 사용
public class TimerManager : MonoBehaviour
{

    public TextMeshProUGUI timerText; // UI에서 남은 시간 표시
    private float remainingTime;
    private bool isRunning;

    public void StartTimer(float duration)
    {
        Debug.Log("TimerManager 시작");
        remainingTime = duration;
        isRunning = true;
        StartCoroutine(RunTimer());
    }

    private IEnumerator RunTimer()
    {
        Debug.Log("타이머 시작");
        if (timerText == null)
        {
            Debug.LogError("타이머 UI가 설정되지 않았습니다.");
        }

        UpdateTimerUI();
        while (IsTimeUp() == false)
        {
            yield return new WaitForSecondsRealtime(1f); // 1초마다 실행 (시간 왜곡 없음)
            remainingTime -= 1f;
            UpdateTimerUI();
        }
        
        isRunning = false;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"남은 시간: {remainingTime:F1} 초";
        }
    }

    public bool IsTimeUp()
    {
        return remainingTime <= 0;
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }
}
