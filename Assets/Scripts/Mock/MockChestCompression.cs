using UnityEngine;

public class MockChestCompression : MonoBehaviour
{
    public CompressionLevelUI compressionUI; // 강도 UI에 연결
    public AEDManager aedManager;            // AEDManager 참조
    public float updateInterval = 0.5f;      // 값 갱신 주기
    public float strengthThreshold = 30f;    // 유효한 압박 기준

    private float timer = 0f;
    private int compressionCount = 0;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            float randomStrength = Random.Range(0f, 100f); // 0~100 무작위
            compressionUI.SetStrength(randomStrength);     // UI에 전달
            Debug.Log($"[Mock] 강도: {randomStrength:F1}");

            // ✅ 일정 강도 이상이면 카운트 증가
            if (randomStrength >= strengthThreshold)
            {
                compressionCount++;
                aedManager.UpdateCompressionCount(compressionCount);
                Debug.Log($"[Mock] 유효 압박 횟수: {compressionCount}회");
            }

            timer = 0f;
        }
    }
}
