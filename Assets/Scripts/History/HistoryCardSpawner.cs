using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HistoryCardSpawner : MonoBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform gridParent;  // HistoryGrid에 연결

    [System.Serializable]
    public class HistoryData
    {
        public string title;
        public int score;
        public string date;
    }

    void Start()
    {
        List<HistoryData> histories = GetDummyData();
        Debug.Log(histories.Count);

        foreach (var data in histories)
        {
            Debug.Log(data.title);
            GameObject card = Instantiate(cardPrefab, gridParent);

            TextMeshProUGUI titleText = card.transform.Find("StageName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = card.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = card.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();

            // 디버깅 로그 (없으면 null인 상태!)
            if (titleText == null) Debug.LogError("StageName 텍스트 못 찾음");
            if (scoreText == null) Debug.LogError("Score 텍스트 못 찾음");
            if (dateText == null) Debug.LogError("Date 텍스트 못 찾음");

            // 값 넣기
            if (titleText != null) titleText.text = data.title;
            if (scoreText != null) scoreText.text = $"점수: {data.score}점";
            if (dateText != null) dateText.text = data.date;

        }
    }

    List<HistoryData> GetDummyData()
    {
        return new List<HistoryData>
        {
            new HistoryData{ title="자동제세동기", score=67, date="2025/04/02"},
            new HistoryData{ title="의심환자 평가", score=88, date="2025/04/01"},
            new HistoryData{ title="진공부목 적용", score=77, date="2025/03/24"},
            new HistoryData{ title="진공부목 적용", score=77, date="2025/03/24"},
            new HistoryData{ title="자동제세동기", score=43, date="2025/03/22"},
            new HistoryData{ title="진공부목 적용", score=75, date="2025/03/20"},
            new HistoryData{ title="의심환자 평가", score=91, date="2025/03/15"},
            new HistoryData{ title="진공부목 적용", score=72, date="2025/03/11"},
            new HistoryData{ title="의심환자 평가", score=87, date="2025/03/05"},
            new HistoryData{ title="진공부목 적용", score=79, date="2025/03/02"},
        };
    }
}
