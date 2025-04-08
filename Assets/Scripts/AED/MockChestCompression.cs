using UnityEngine;

public class MockChestCompression : MonoBehaviour
{
    public CompressionLevelUI compressionUI; // 강도 UI에 연결
    public float updateInterval = 0.5f;       // 몇 초마다 값 갱신할지
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            float randomStrength = Random.Range(0f, 100f); // 0~100 사이 무작위
            compressionUI.SetStrength(randomStrength);     // UI에 전달
            Debug.Log($"[Mock] 강도: {randomStrength:F1}");

            timer = 0f;
        }
    }
}
