using UnityEngine;

public class TestPositionPutObject : MonoBehaviour
{
    [Header("구체 오브젝트 프리팹")]
    public GameObject spherePrefab;

    private GameObject spawnedSphere;

    /// <summary>
    /// 외부에서 호출하는 구체 배치 메서드
    /// </summary>
    public void PlaceSphere(int markerId, Vector3 localOffset, float radius)
    {
        if (!OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData marker))
        {
            Debug.LogWarning($"❌ 마커 ID {markerId} 를 찾을 수 없습니다.");
            return;
        }

        Vector3 worldOffset = marker.rotation * localOffset;
        Vector3 targetWorldPos = marker.position + worldOffset;

        MainThreadDispatcher.Enqueue(() =>
        {
            if (spawnedSphere == null)
            {
                spawnedSphere = Instantiate(spherePrefab, targetWorldPos, Quaternion.identity);
                spawnedSphere.transform.localScale = Vector3.one * radius * 2f;
            }
            else
            {
                spawnedSphere.transform.position = targetWorldPos;
                spawnedSphere.transform.localScale = Vector3.one * radius * 2f;
            }

            Debug.Log($"🟢 구체 배치 완료 → 마커 {markerId} 기준 위치: {targetWorldPos:F3}, 반지름: {radius}m");
        });
    }

    /// <summary>
    /// 필요 시 제거
    /// </summary>
    public void ClearSphere()
    {
        if (spawnedSphere != null)
        {
            Destroy(spawnedSphere);
            spawnedSphere = null;
        }
    }
}
