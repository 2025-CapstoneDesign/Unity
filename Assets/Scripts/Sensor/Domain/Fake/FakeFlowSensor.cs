using System.Collections;

public class FakeFlowSensor : Sensor
{
    protected override IEnumerator SendDataCoroutine()
    {
        while (true)
        {
            float data = UnityEngine.Random.Range(0.1f, 2.0f); // 0.1 ~ 2.0 범위의 데이터 생성
            NotifyData(SensorType.FlowSensor, data);
            yield return new UnityEngine.WaitForSeconds(0.5f); // 0.5초 간격
        }
    }
}