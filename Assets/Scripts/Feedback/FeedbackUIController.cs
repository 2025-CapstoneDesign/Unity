using UnityEngine;
using UnityEngine.SceneManagement;

public class FeedbackUIController : MonoBehaviour
{
    public void OnRetryButtonPressed()
    {
        // GameManager의 sceneName으로 이동
        string retryScene = GameManager.Instance.sceneName;

        if (!string.IsNullOrEmpty(retryScene))
        {
            Debug.Log("재시도 씬 로드: " + retryScene);
            SceneManager.LoadScene(retryScene);
        }
        else
        {
            Debug.LogError("재시도할 씬 이름이 설정되지 않았습니다!");
        }
    }

    public void OnMenuButtonPressed()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
