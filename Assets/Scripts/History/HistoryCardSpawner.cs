using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HistoryCardSpawner : MonoBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform gridParent;  // HistoryGrid에 연결
    public ResultHistoryManager historyManager; // 인스펙터에 연결하거나 GetComponent로

    void Start()
    {
        // historyManager 연결 (직접 연결도 가능)
        if (historyManager == null)
            historyManager = FindObjectOfType<ResultHistoryManager>();

        List<TrainingResult> results = historyManager.GetResults();
        Debug.Log("불러온 기록 수: " + results.Count);

        foreach (var data in results)
        {
            Debug.Log("카드 생성");
            GameObject card = Instantiate(cardPrefab, gridParent);
           
            Debug.Log("카드 생성됨: " + card.name);


            TextMeshProUGUI titleText = card.transform.Find("StageName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = card.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = card.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();

            if (titleText == null) Debug.LogError("StageName 텍스트 못 찾음");
            if (scoreText == null) Debug.LogError("Score 텍스트 못 찾음");
            if (dateText == null) Debug.LogError("Date 텍스트 못 찾음");

            if (titleText != null) titleText.text = data.protocolName;
            if (scoreText != null) scoreText.text = $"점수: {data.score}점";
            if (dateText != null) dateText.text = data.date;
        }
    }
}
