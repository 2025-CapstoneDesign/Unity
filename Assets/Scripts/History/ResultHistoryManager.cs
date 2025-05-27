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
    private string apiUrl = "http://192.168.1.129:10055/api/protocol";
    public bool isLoaded { get; private set; } = false;

    void Awake()
    {
#if UNITY_WSA && !UNITY_EDITOR
        // UWP에서 HTTP 통신 허용 설정
        UnityEngine.WSA.Application.InvokeOnUIThread(() =>
        {
            try
            {
                Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                filter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.InvalidName);
            }
            catch (Exception e)
            {
                Debug.LogError($"HTTP 설정 중 오류 발생: {e.Message}");
            }
        }, false);
#endif

        if (FindObjectsOfType<ResultHistoryManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // 초기화 및 데이터 로드
        filePath = Path.Combine(Application.dataPath, "Scripts", "History", "local_history.json");
        Debug.Log($"📂 History file path: {filePath}");
        LoadAllResults();
    }

    void Start()
    {
        // Start는 비워둡니다
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
        try
        {
            string originalId = SystemInfo.deviceUniqueIdentifier;
            string deviceId = GetShortHashString(originalId);
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
                
#if UNITY_WSA && !UNITY_EDITOR
                // UWP/HoloLens에서 인증서 검증 무시
                request.certificateHandler = new BypassCertificate();
#endif

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
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 전송 중 오류 발생: {e.Message}");
        }
    }

    public void LoadAllResults()
    {
        Debug.Log($"🔍 Checking file exists: {File.Exists(filePath)}");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"📄 Loaded JSON content: {json}");
            allResults = JsonUtility.FromJson<TrainingResultList>(json);
            Debug.Log($"📊 Loaded results count: {allResults.results.Count}");
            foreach (var result in allResults.results)
            {
                Debug.Log($"📝 Result: {result.protocol_name} - {result.date} - Score: {result.score}");
            }
        }
        else
        {
            Debug.LogWarning("❌ History file not found. Creating new TrainingResultList");
            allResults = new TrainingResultList(); // 처음 실행 시
        }
        isLoaded = true;
    }

    public List<TrainingResult> GetResults()
    {
        if (allResults == null || allResults.results == null)
        {
            Debug.LogWarning("❌ allResults or results list is null");
            return new List<TrainingResult>();
        }

        Debug.Log($"🔄 Sorting {allResults.results.Count} results");
        var results = allResults.results
            .OrderByDescending(r =>
            {
                DateTime date;
                if (DateTime.TryParse(r.date, out date))
                {
                    Debug.Log($"📅 Parsed date {r.date} successfully");
                    return date;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to parse date: {r.date}");
                    return DateTime.MinValue;
                }
            })
            .Take(10)
            .ToList();
            
        Debug.Log($"📋 GetResults returning {results.Count} items");
        return results;
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

// 인증서 검증을 무시하기 위한 핸들러
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
