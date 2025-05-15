using UnityEngine;

public class InfantCPRSceneStart : MonoBehaviour
{
    public SceneStartGuide sceneStartGuide;

    void Start()
    {
        var info = new SceneStartInfo("영아 심폐소생술 훈련을 시작합니다.", null);
        sceneStartGuide.SetSceneInfo(info);

        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is NULL! AudioManager가 없거나 초기화되지 않았습니다.");
        }
        else
        {
            Debug.Log("AudioManager.Instance 사용 시작");
            AudioManager.Instance.PlayVoice("SceneStart/Voice_Stage5");
        }
    }
}