public class SceneStartInfo
{
    public string guideMessage;
    public string guideAudioFileName;  // <-- 폴더 제외, 순수 파일명만

    public SceneStartInfo(string message, string audioFileName)
    {
        guideMessage = message;
        guideAudioFileName = audioFileName;
    }
}
