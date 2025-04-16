using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorManager : MonoBehaviour
{
    public event System.Action<SensorType, double> OnSensorDataReceived; // 이벤트 정의

    private List<Sensor> sensors;

    void Awake()
    {
        sensors = new List<Sensor>
        {
            new FakeCompressionSensor(),
            new FakeFlowSensor(),
            new FakeGyroSensor()
        };

        Debug.Log("SensorManager: Sensors initialized in Awake.");
    }

    void OnEnable()
    {
        foreach (var sensor in sensors)
        {
            sensor.OnDataSent += HandleSensorData; // 이벤트 연결
            sensor.StartSending(this);            // 데이터 전송 시작
        }

        Debug.Log("SensorManager: Sensors started.");
    }

    void OnDisable()
    {
        foreach (var sensor in sensors)
        {
            sensor.OnDataSent -= HandleSensorData; // 이벤트 해제
        }

        Debug.Log("SensorManager: Sensors stopped.");
    }

    private void HandleSensorData(SensorType type, double data) // 메서드 이름 변경
    {
        // Debug.Log($"SensorManager: Received data - Type: {type}, Data: {data}");
        // 이벤트 호출
        OnSensorDataReceived?.Invoke(type, data);
    }
}
