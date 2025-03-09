using System;
using UnityEngine;

public class InfantCPRManager
{
    private SensorManager sensorManager;
    private int compressionCount = 0;
    private const int requiredCompressions = 5;

    public bool IsInfantCPRPassed { get; private set; } = false;

    // 생성자에서 SensorManager를 주입받고 즉시 실행
    public InfantCPRManager(SensorManager manager)
    {
        sensorManager = manager ?? throw new ArgumentNullException(nameof(manager)); // Null 방지
        Debug.Log("InfantCPRManager 초기화");
        Initialize();
        if (sensorManager != null)
        {
            Debug.Log("센서 데이터 이벤트 구독 완료");
        }
        else
        {
            Debug.LogError("SensorManager가 null입니다!");
        }
    }

    // 초기화 메서드
    private void Initialize()
    {
        compressionCount = 0;
        IsInfantCPRPassed = false;

        if (sensorManager != null)
        {
            // 센서 데이터 이벤트 구독
            sensorManager.OnSensorDataReceived += HandleSensorData;
            Debug.Log("센서 데이터 구독");
        }
    }

    // CPR 로직 실행 중단
    public void StopCPR()
    {
        if (sensorManager != null)
        {
            sensorManager.OnSensorDataReceived -= HandleSensorData;
            Debug.Log("CPR 로직 중단");
        }
    }

    // 센서 데이터를 처리하는 메서드
    private void HandleSensorData(SensorType sensorType, double data)
    {
        Debug.Log("핸들러 센서 데이터 호출되고있습니다.");
    
        if (IsInfantCPRPassed)
        {
            Debug.Log("이미 통과!");
            return;
        }

        if (sensorType == SensorType.CompressionSensor)
        {
            Debug.Log($"CPR Compression Strength: {data} kg");
            if (data >= 7 && data <= 16)
            {
                compressionCount++;
                Debug.Log($"Valid compression count: {compressionCount}");

                if (compressionCount >= requiredCompressions)
                {
                    Debug.Log("5회가 넘었습니다!");
                    StopCPR();
                    IsInfantCPRPassed = true; // CPR 통과
                }
            }
            else
            {
                Debug.Log("압박 강도가 너무 약해요!");
            }
        }
        else
        {
            Debug.Log("압박 센서가 없습니다.");
        }
    }
}
