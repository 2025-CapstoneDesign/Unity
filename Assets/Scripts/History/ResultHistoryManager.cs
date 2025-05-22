using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;  // Task를 위해 추가
using UnityEngine.Networking;
using System.Security.Cryptography;

public class ResultHistoryManager : MonoBehaviour
{
    string filePath;
    TrainingResultList allResults = new TrainingResultList();
    private string apiUrl = "http://127.0.0.1:10055/api/protocol";

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "result.json");
        LoadAllResults(); // 시작 시 기존 기록 불러오기
    }

    // 디바이스 ID를 짧은 해시로 변환하는 메서드 추가
    private string GetShortHashString(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 5); // 20자로 제한
        }
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

    public async void SaveNewResult(TrainingResult newResult)
    {
        allResults.results.Add(newResult);
        string json = JsonUtility.ToJson(allResults, true);
        File.WriteAllText(filePath, json);
        Debug.Log("기록 저장 완료");

        await SendResultToServer(newResult);
    }

    private async Task SendResultToServer(TrainingResult result)
    {
        string originalId = SystemInfo.deviceUniqueIdentifier;
        string deviceId = GetShortHashString(originalId); // 짧은 해시로 변환
        // 직렬화 가능한 클래스로 변환
        var requestBody = new ProtocolData
        {
            userId = deviceId,
            protocolName = result.protocolName,
            date = result.date,
            duration = result.duration.ToString(),
            score = result.score.ToString()
        };

        string jsonData = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("서버에 데이터 전송 성공");
                }
                else
                {
                    Debug.LogError($"서버 전송 실패: {request.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 전송 중 오류 발생: {e.Message}");
            }
        }
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

[Serializable]
public class ProtocolData
{
    public string userId;
    public string protocolName;
    public string date;
    public string duration;
    public string score;
}
