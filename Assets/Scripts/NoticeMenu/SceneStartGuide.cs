using System.Collections;
using UnityEngine;
using TMPro;

public class SceneStartGuide : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup guideMessageGroup;
    public TextMeshProUGUI guideText;
    public GameObject noticeCanvas;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float messageDisplayTime = 2f;

    [Header("Audio Settings")]
    public string voiceFolder = "Audio/VoiceResources"; // 상위 폴더만 설정

    // 씬별로 설정되는 정보
    private SceneStartInfo currentInfo;

    public void SetSceneInfo(SceneStartInfo info)
    {
        currentInfo = info;
    }

    void Start()
    {
        StartCoroutine(ShowGuideThenNotice());
    }

    IEnumerator ShowGuideThenNotice()
    {
        // 텍스트 설정
        guideText.text = currentInfo?.guideMessage ?? "훈련을 시작합니다.";

        // 음성 재생
        if (!string.IsNullOrEmpty(currentInfo?.guideAudioFileName))
        {
            string fullPath = $"{currentInfo.guideAudioFileName}";
            AudioManager.Instance.PlayVoice(fullPath);
        }

        // UI 표시
        guideMessageGroup.alpha = 0f;
        guideMessageGroup.gameObject.SetActive(true);
        noticeCanvas.SetActive(false);

        yield return StartCoroutine(FadeCanvasGroup(guideMessageGroup, 0f, 1f));
        yield return new WaitForSeconds(messageDisplayTime);
        yield return StartCoroutine(FadeCanvasGroup(guideMessageGroup, 1f, 0f));

        guideMessageGroup.gameObject.SetActive(false);
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
