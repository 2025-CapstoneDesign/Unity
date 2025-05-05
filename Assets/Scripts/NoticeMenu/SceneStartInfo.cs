using UnityEngine;

public class SceneStartInfo
{
    public string guideMessage;
    public AudioClip guideAudio;

    public SceneStartInfo(string message, AudioClip audio)
    {
        guideMessage = message;
        guideAudio = audio;
    }
}
