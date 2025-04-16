using System.Collections;

public class FakeGyroSensor : Sensor
{
    protected override IEnumerator SendDataCoroutine()
    {
        while (true)
        {
            float data = UnityEngine.Random.Range(-90.0f, 90.0f); // -90.0 ~ 90.0 범위의 데이터 생성
            NotifyData(SensorType.GyroSensor, data);
            yield return new UnityEngine.WaitForSeconds(0.5f); // 0.5초 간격
        }
    }
}