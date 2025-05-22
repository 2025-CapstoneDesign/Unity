using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrainingResult
{
    public string protocol_name;  // 서버의 필드명과 일치시킴
    public string protocolName   // 프로퍼티 제거하고 실제 데이터는 protocol_name 사용
    {
        get => protocol_name;
        set => protocol_name = value;
    }
    
    public string date;
    public string duration;
    public int score;
    public string feedback;
}

// List를 JSON으로 저장하려면 이걸 따로 감싸줘야 해
[System.Serializable]
public class TrainingResultList
{
    public List<TrainingResult> results = new List<TrainingResult>();
}
