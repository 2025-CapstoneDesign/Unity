using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 마커 데이터 클래스 - 위치와 회전 정보를 저장하고 평활화하는 기능
/// </summary>
public class MarkerData
{
    public Vector3 position;
    public Quaternion rotation;
    
    // 이전 위치와 회전 데이터를 저장하는 큐
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private Queue<Quaternion> rotationHistory = new Queue<Quaternion>();
    
    // 평활화 설정
    private int maxHistoryCount = 10;
    
    public MarkerData(Vector3 pos, Quaternion rot)
    {
        this.position = pos;
        this.rotation = rot;
        
        // 초기 위치와 회전으로 히스토리 초기화
        positionHistory.Enqueue(pos);
        rotationHistory.Enqueue(rot);
    }
    
    // 새 위치 데이터 추가 및 평활화
    public void UpdatePosition(Vector3 newPos, float smoothFactor, int historyLimit)
    {
        // 히스토리 크기 조정
        maxHistoryCount = historyLimit;
        
        // 새 위치 데이터 추가
        positionHistory.Enqueue(newPos);
        
        // 히스토리 크기 유지
        while (positionHistory.Count > maxHistoryCount)
            positionHistory.Dequeue();
        
        // 평활화 적용
        if (positionHistory.Count > 1)
        {
            Vector3 avgPos = Vector3.zero;
            foreach (var pos in positionHistory)
                avgPos += pos;
            
            avgPos /= positionHistory.Count;
            
            // 보간 적용 - smoothFactor가 1에 가까울수록 이전 평균치에 더 영향받음
            position = Vector3.Lerp(newPos, avgPos, smoothFactor);
        }
        else
        {
            position = newPos;
        }
    }
    
    // 새 회전 데이터 추가 및 평활화
    public void UpdateRotation(Quaternion newRot, float smoothFactor, int historyLimit)
    {
        // 히스토리 크기 조정
        maxHistoryCount = historyLimit;
        
        // 새 회전 데이터 추가
        rotationHistory.Enqueue(newRot);
        
        // 히스토리 크기 유지
        while (rotationHistory.Count > maxHistoryCount)
            rotationHistory.Dequeue();
        
        // 평활화 적용
        if (rotationHistory.Count > 1)
        {
            Quaternion avgRot = Quaternion.identity;
            int count = 0;
            
            foreach (var rot in rotationHistory)
            {
                if (count == 0)
                {
                    avgRot = rot;
                }
                else
                {
                    // 회전 보간 적용
                    avgRot = Quaternion.Slerp(avgRot, rot, 1.0f / (count + 1));
                }
                count++;
            }
            
            // 최종 보간 적용
            rotation = Quaternion.Slerp(newRot, avgRot, smoothFactor);
        }
        else
        {
            rotation = newRot;
        }
    }
}
