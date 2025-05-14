using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("설정")]
    public string audioFolderPath = "Audio";
    public AudioSource voiceAudioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환시 유지
            
            // 씬 변경 이벤트에 리스너 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 씬이 로드될 때 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 재생 중인 모든 오디오 중지
        StopAllAudio();
    }

    // 모든 오디오 중지 메서드
    public void StopAllAudio()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
            Debug.Log("씬 전환: 모든 오디오 중지됨");
        }
    }

    void OnDestroy()
    {
        // 이벤트 리스너 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayVoice(string relativePath)
    {
        Debug.Log("PlayVoice()"); // ????? ?????

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

    public bool IsVoicePlaying()
    {
        return voiceAudioSource != null && voiceAudioSource.isPlaying;
    }
}