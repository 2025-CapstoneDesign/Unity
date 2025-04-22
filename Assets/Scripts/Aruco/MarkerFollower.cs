using UnityEngine;

public class MarkerFollower : MonoBehaviour
{
    public int markerId = 0;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public int updateInterval = 2; // 💡 2프레임마다 실행
    public float offsetDistance = 0.02f; // 마커로부터의 거리
    public float wallThreshold = 0.5f; // 벽 마커 판단 임계값

    private Renderer objectRenderer;
    private int frameCounter = 0;
    private bool isInitialized = false;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

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
            
            // 마커의 위쪽 벡터와 월드의 위쪽 벡터 사이의 각도를 계산
            float upDot = Vector3.Dot(markerUp, Vector3.up);
            
            // 벽 마커인지 판단 (위쪽 벡터가 수직에 가까우면 바닥 마커, 수평에 가까우면 벽 마커)
            bool isWall = Mathf.Abs(upDot) < wallThreshold;
            
            // 마커 방향에 따라 적절한 오프셋 방향 결정
            Vector3 offsetDirection = isWall ? markerForward : markerUp;
            
            // 오프셋 적용
            Vector3 targetPosition = data.position + offsetDirection * offsetDistance;

            // 회전도 마커 방향에 따라 조정
            Quaternion targetRotation = isWall ? 
                Quaternion.LookRotation(markerForward, Vector3.up) : 
                Quaternion.LookRotation(markerForward, markerUp);

            if (!isInitialized)
            {
                // 초기 위치 설정 시에는 보간 없이 바로 이동
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isInitialized = true;
                lastPosition = targetPosition;
                lastRotation = targetRotation;
            }
            else
            {
                // 위치가 크게 변했는지 확인
                float positionChange = Vector3.Distance(lastPosition, targetPosition);
                float rotationChange = Quaternion.Angle(lastRotation, targetRotation);

                if (positionChange > 0.1f || rotationChange > 30f)
                {
                    // 위치나 회전이 크게 변했다면 바로 이동
                    transform.position = targetPosition;
                    transform.rotation = targetRotation;
                }
                else
                {
                    // 작은 변화에 대해서만 보간 적용
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
                }

                lastPosition = targetPosition;
                lastRotation = targetRotation;
            }

            if (objectRenderer != null && !objectRenderer.enabled)
                objectRenderer.enabled = true;
        }
        else
        {
            if (objectRenderer != null && objectRenderer.enabled)
                objectRenderer.enabled = false;
            isInitialized = false;
        }
    }
}
