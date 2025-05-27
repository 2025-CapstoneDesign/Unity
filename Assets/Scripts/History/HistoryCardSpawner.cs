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
        Debug.Log("🎬 HistoryCardSpawner Start");
        
        if (historyManager == null)
        {
            Debug.Log("🔍 Looking for ResultHistoryManager");
            historyManager = FindObjectOfType<ResultHistoryManager>();
            if (historyManager == null)
            {
                Debug.LogError("❌ ResultHistoryManager not found!");
                yield break;
            }
        }

        Debug.Log("⏳ Waiting for results to load...");
        yield return new WaitUntil(() => historyManager.isLoaded);

        List<TrainingResult> results = historyManager.GetResults();
        Debug.Log($"📦 Loaded {results.Count} results");

        if (cardPrefab == null)
        {
            Debug.LogError("❌ Card prefab is not assigned!");
            yield break;
        }

        if (gridParent == null)
        {
            Debug.LogError("❌ Grid parent is not assigned!");
            yield break;
        }

        foreach (var data in results)
        {
            Debug.Log($"🎴 Creating card for: {data.protocol_name}");
            GameObject card = Instantiate(cardPrefab, gridParent);
            
            TextMeshProUGUI titleText = card.transform.Find("StageName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = card.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = card.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();

            if (titleText == null) Debug.LogError($"❌ StageName text component not found on card {card.name}");
            if (scoreText == null) Debug.LogError($"❌ Score text component not found on card {card.name}");
            if (dateText == null) Debug.LogError($"❌ Date text component not found on card {card.name}");

            if (titleText != null) titleText.text = data.protocol_name;
            if (scoreText != null) scoreText.text = data.score.ToString();
            if (dateText != null) dateText.text = data.date;

            Debug.Log($"✅ Card created successfully: {data.protocol_name} - {data.date} - Score: {data.score}");
        }
    }

}
