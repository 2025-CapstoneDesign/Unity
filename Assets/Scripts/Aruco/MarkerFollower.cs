using UnityEngine;

public class MarkerFollower : MonoBehaviour
{
    public int markerId = 0;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public int updateInterval = 2; // 💡 2프레임마다 실행

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
        frameCounter = 0;

        if (OptimizedArUcoMarkerDetection.markerMap.TryGetValue(markerId, out MarkerData data))
        {
            Vector3 markerUp = data.rotation * Vector3.up;
            Vector3 markerForward = data.rotation * Vector3.forward;
            bool isWall = Mathf.Abs(Vector3.Dot(markerUp, Vector3.up)) < 0.5f;
            float offsetDistance = 0.02f;

            Vector3 offsetPosition = isWall
                ? data.position + markerForward * offsetDistance
                : data.position + markerUp * offsetDistance;

            transform.position = offsetPosition;
            transform.rotation = data.rotation;

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
