using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class ResultHistoryManager : MonoBehaviour
{
    string filePath;
    TrainingResultList allResults = new TrainingResultList();

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "result.json");
        LoadAllResults(); // 시작 시 기존 기록 불러오기
    }

    public void SaveNewResult(TrainingResult newResult)
    {
        allResults.results.Add(newResult); // 새 결과 추가
        string json = JsonUtility.ToJson(allResults, true);
        File.WriteAllText(filePath, json);
        Debug.Log("기록 저장 완료");
    }

    public void LoadAllResults()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            allResults = JsonUtility.FromJson<TrainingResultList>(json);
            Debug.Log("기록 불러오기 완료. 총 " + allResults.results.Count + "개");
        }
        else
        {
            allResults = new TrainingResultList(); // 처음 실행 시
        }
    }

    public List<TrainingResult> GetResults()
    {
        return allResults.results;
    }
}
