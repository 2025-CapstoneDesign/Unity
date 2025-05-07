using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("????")]
    public string audioFolderPath = "Audip";
    public AudioSource voiceAudioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ???? ????
        }
        else
        {
            Destroy(gameObject);
        }
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