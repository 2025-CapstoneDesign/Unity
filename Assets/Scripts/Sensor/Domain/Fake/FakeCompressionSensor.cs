using System.Collections;
using UnityEngine;

public class FakeCompressionSensor : Sensor
{
    protected override IEnumerator SendDataCoroutine()
    {
        while (true)
        {
            float data = UnityEngine.Random.Range(20.0f, 60.0f); // 임의 데이터 생성
            // Debug.Log($"FakeCompressionSensor: Notifying data {data}");
            NotifyData(SensorType.CompressionSensor, data);
            yield return new UnityEngine.WaitForSeconds(0.5f);
        }
    }
}