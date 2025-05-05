using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("설정")]
    public string audioFolderPath = "Audio/VoiceResources";
    public AudioSource voiceAudioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 선택 사항
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlayVoice(string relativePath)
    {
        Debug.Log("PlayVoice() 실행 시작"); // 최상단에 넣어보기
        Debug.Log($"[AudioManager] PlayVoice() 호출됨: {relativePath}");
        string fullPath = $"{audioFolderPath}/{relativePath}";
        Debug.Log($"[AudioManager] Trying to load: {fullPath}");

        AudioClip clip = Resources.Load<AudioClip>(fullPath);

        if (clip == null)
        {
            Debug.LogError($"AudioClip not found at: {fullPath}");
            return;
        }

        voiceAudioSource.clip = clip;
        voiceAudioSource.Play();
    }


}
