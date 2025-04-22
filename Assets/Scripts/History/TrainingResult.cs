using System.Collections.Generic;

[System.Serializable]
public class TrainingResult
{
    public string protocolName;
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
