using System.Collections;
using UnityEngine;
using TMPro;

public class ShowMessageWithSound : MonoBehaviour
{
    public TextMeshProUGUI messageText;  // 연결할 TextMeshPro 오브젝트
    public AudioSource alertSound;       // 경고음을 재생할 AudioSource

    void Start()
    {
        // 시작 시 텍스트 숨기기
        messageText.gameObject.SetActive(false);
    }

    // 외부에서 호출할 함수
    public void ShowMessage(string text, float duration)
    {
        StartCoroutine(ShowMessageCoroutine(text, duration));
    }

    private IEnumerator ShowMessageCoroutine(string text, float duration)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

        if (alertSound != null)
        {
            alertSound.Play();  // 경고음 재생
        }

        yield return new WaitForSeconds(duration);

        messageText.gameObject.SetActive(false);
    }
}
