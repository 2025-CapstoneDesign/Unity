using System;

public static class SensorEvents
{
    public static Action<string, float> OnSensorDataReceived; // 유량 센서, 압력 센서
    public static Action<float, float> OnGyroDataReceived;    // 자이로 센서 (roll, pitch)
}
