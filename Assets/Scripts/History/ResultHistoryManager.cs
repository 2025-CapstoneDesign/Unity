using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ResultHistoryManager : MonoBehaviour
{
    string filePath;
    TrainingResultList allResults = new TrainingResultList();

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "result.json");
        LoadAllResults(); // 시작 시 기존 기록 불러오기
    }

    void Awake()
    {
        if (FindObjectsOfType<ResultHistoryManager>().Length > 1)
        {
            Destroy(gameObject); // 이미 있다면 제거
            return;
        }

        DontDestroyOnLoad(gameObject);
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
        return allResults.results
            .OrderByDescending(r =>
            {
                DateTime date;
                if (DateTime.TryParse(r.date, out date))
                    return date;
                else
                    return DateTime.MinValue; // 날짜 파싱 실패 시 가장 오래된 걸로 취급
            })
            .Take(10)
            .ToList();
    }
}
