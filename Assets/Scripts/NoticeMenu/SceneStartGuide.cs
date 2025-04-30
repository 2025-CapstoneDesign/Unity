using System.Collections;
using UnityEngine;
using TMPro; // TMP 전용 네임스페이스 추가!

public class SceneStartGuide : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup guideMessageGroup; // 간단한 메시지용 패널
    public TextMeshProUGUI guideText;     // 메시지를 보여줄 TMP 텍스트
    public GameObject noticeCanvas;       // 본격 안내 UI

    [Header("Settings")]
    public string messageText = "훈련을 시작합니다."; // 인스펙터에서 입력할 메시지
    public float fadeDuration = 1f;
    public float messageDisplayTime = 2f;

    void Start()
    {
        StartCoroutine(ShowGuideThenNotice());
    }

    IEnumerator ShowGuideThenNotice()
    {
        // 초기 설정
        guideText.text = messageText;
        guideMessageGroup.alpha = 0f;
        guideMessageGroup.gameObject.SetActive(true);
        noticeCanvas.SetActive(false);

        // 1. 페이드 인
        yield return StartCoroutine(FadeCanvasGroup(guideMessageGroup, 0f, 1f));

        // 2. 유지 시간
        yield return new WaitForSeconds(messageDisplayTime);

        // 3. 페이드 아웃
        yield return StartCoroutine(FadeCanvasGroup(guideMessageGroup, 1f, 0f));
        guideMessageGroup.gameObject.SetActive(false);

        // 4. 본격 UI 켜기
        noticeCanvas.SetActive(true);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(from, to, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        cg.alpha = to;
    }
}
