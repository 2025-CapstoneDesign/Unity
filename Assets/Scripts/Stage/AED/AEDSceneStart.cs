using UnityEngine;

public class AEDSceneStart : MonoBehaviour
{
    public SceneStartGuide guide;

    void Awake()
    {
        var info = new SceneStartInfo(
            "자동제세동기 훈련을 시작하겠습니다.",
            "Voice_Stage1"
        );

        guide.SetSceneInfo(info);
    }
}
