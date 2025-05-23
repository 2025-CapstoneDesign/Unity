using System;
using UnityEngine;

public class MarkerDistanceValidate : MonoBehaviour
{
    [Serializable]
    public class DistanceData
    {
        public bool IsVerified;
        public Vector3[] positionHistory1 = new Vector3[10];
        public Vector3[] positionHistory2 = new Vector3[10];
        public int historyIndex = 0;
        public bool hasFullHistory = false;
        public Vector3 lastPosition1;
        public Vector3 lastPosition2;
    }

    public bool IsVerified { get; private set; } = false;

    private int markerId1;
    private int markerId2;
    private float minDistance;
    private float maxDistance;
    private Action onVerifiedCallback;
    private DistanceData distanceData = new DistanceData();

    private bool isActive = false;

    public void BeginValidation(int markerId1, int markerId2, float minDistance, float maxDistance, Action onSuccess)
    {
        this.markerId1 = markerId1;
        this.markerId2 = markerId2;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
        this.onVerifiedCallback = onSuccess;

        IsVerified = false;
        isActive = true;
        
        // 히스토리 초기화
        distanceData = new DistanceData();
    }

    public void StopValidation()
    {
        isActive = false;
    }

    void Update()
    {
        if (!isActive || IsVerified)
            return;

        var map = OptimizedArUcoMarkerDetection.markerMap;

        if (map.TryGetValue(markerId1, out MarkerData marker1) && map.TryGetValue(markerId2, out MarkerData marker2))
        {
            // 위치 히스토리 업데이트
            distanceData.positionHistory1[distanceData.historyIndex] = marker1.position;
            distanceData.positionHistory2[distanceData.historyIndex] = marker2.position;
            
            distanceData.historyIndex = (distanceData.historyIndex + 1) % distanceData.positionHistory1.Length;
            if (distanceData.historyIndex == 0)
                distanceData.hasFullHistory = true;

            // 안정화된 위치 계산
            Vector3 stablePos1 = GetStablePosition(distanceData.positionHistory1);
            Vector3 stablePos2 = GetStablePosition(distanceData.positionHistory2);

            // 급격한 변화 감지
            float positionChange1 = Vector3.Distance(distanceData.lastPosition1, marker1.position);
            float positionChange2 = Vector3.Distance(distanceData.lastPosition2, marker2.position);

            Vector3 pos1, pos2;

            if (positionChange1 > 0.1f || positionChange2 > 0.1f)
            {
                // 급격한 변화 시 천천히 보간
                pos1 = Vector3.Lerp(stablePos1, marker1.position, Time.deltaTime * 3f);
                pos2 = Vector3.Lerp(stablePos2, marker2.position, Time.deltaTime * 3f);
            }
            else
            {
                // 작은 변화는 더 빠르게 보간
                pos1 = Vector3.Lerp(stablePos1, marker1.position, Time.deltaTime * 8f);
                pos2 = Vector3.Lerp(stablePos2, marker2.position, Time.deltaTime * 8f);
            }

            // 현재 상태 저장
            distanceData.lastPosition1 = pos1;
            distanceData.lastPosition2 = pos2;

            float distance = Vector3.Distance(pos1, pos2);

            if (distance >= minDistance && distance <= maxDistance)
            {
                IsVerified = true;
                isActive = false;
                onVerifiedCallback?.Invoke();
            }
        }
    }

    private Vector3 GetStablePosition(Vector3[] history)
    {
        if (!distanceData.hasFullHistory)
            return history[0];

        Vector3 avgPosition = Vector3.zero;
        int count = distanceData.hasFullHistory ? history.Length : distanceData.historyIndex;

        for (int i = 0; i < count; i++)
            avgPosition += history[i];

        return avgPosition / count;
    }
}
