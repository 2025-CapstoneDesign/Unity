using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HistoryCardSpawner : MonoBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform gridParent;  // HistoryGrid에 연결
    public ResultHistoryManager historyManager; // 인스펙터에 연결하거나 GetComponent로

    IEnumerator Start()
    {
        if (historyManager == null)
            historyManager = FindObjectOfType<ResultHistoryManager>();

        // 데이터 로딩이 완료될 때까지 대기
        yield return new WaitUntil(() => historyManager.IsDataLoaded());

        List<TrainingResult> results = historyManager.GetResults();
        Debug.Log("📦 실제 불러온 기록 수: " + results.Count);

        foreach (var data in results)
        {
            GameObject card = Instantiate(cardPrefab, gridParent);
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
