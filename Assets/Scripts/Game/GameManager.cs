using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string protocolName;
    public string duration;
    public int score;
    public string feedback;
    public string sceneName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("Instance is Null");
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 살아있게
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
