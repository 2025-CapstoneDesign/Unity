using UnityEngine;

public class AEDSceneStart : MonoBehaviour
{
    public SceneStartGuide sceneStartGuide;

    void Awake()
    {
        var info = new SceneStartInfo("자동제세동기(AED) 훈련을 시작합니다.", null);
        sceneStartGuide.SetSceneInfo(info);

        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is NULL! AudioManager가 씬에 없거나 초기화되지 않았습니다.");
        }
        else
        {
            Debug.Log("AudioManager.Instance 접근 성공");
            AudioManager.Instance.PlayVoice("SceneStart/Voice_Stage1");
        }
    }

}