using UnityEngine;

public class MarkerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab; // 마커로 쓸 오브젝트 (그라데이션도 OK)

    public void SpawnMarker(MarkerData data) //id로 가능 
    {
        if (markerPrefab == null)
        {
            Debug.LogWarning("❌ markerPrefab이 할당되지 않았습니다.");
            return;
        }

        GameObject marker = Instantiate(markerPrefab, data.position, data.rotation);
        // 필요 시 부모 설정
        // marker.transform.SetParent(someParent, worldPositionStays: true);
    }
}
