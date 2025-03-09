using UnityEngine;
using System;
using System.Collections;

public abstract class Sensor
{
    public event Action<SensorType, double> OnDataSent;

    // 센서 데이터를 주기적으로 보내는 메서드
    public void StartSending(MonoBehaviour context)
    {
        context.StartCoroutine(SendDataCoroutine());
    }

    protected void NotifyData(SensorType type, double data)
    {
        OnDataSent?.Invoke(type, data);
    }

    protected abstract IEnumerator SendDataCoroutine();
}