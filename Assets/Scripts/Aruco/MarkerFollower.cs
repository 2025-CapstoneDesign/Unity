using UnityEngine;

public class MarkerFollower : MonoBehaviour
{
    public int markerId = 0;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public int updateInterval = 10; // 💡 10프레임마다 실행

    private Renderer objectRenderer;
    private int frameCounter = 0;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogWarning("[SmoothMarkerFollower] Renderer 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            objectRenderer.enabled = false;
        }
    }

    void Update()
    {
        frameCounter++;

        if (frameCounter < updateInterval) return;

        // 💡 10프레임마다 한 번만 아래 실행
        frameCounter = 0;

        if (OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData data))
        {
            transform.position = Vector3.Lerp(transform.position, data.position, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, data.rotation, Time.deltaTime * rotateSpeed);

            if (objectRenderer != null && !objectRenderer.enabled)
                objectRenderer.enabled = true;
        }
        else
        {
            if (objectRenderer != null && objectRenderer.enabled)
                objectRenderer.enabled = false;
        }
    }
}
