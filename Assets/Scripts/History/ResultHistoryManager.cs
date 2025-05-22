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
    private static string filePath = Path.Combine(Application.dataPath, "Scripts", "History", "local_history.json");
    private static TrainingResultList allResults = new TrainingResultList();
    private string apiUrl = "http://192.168.1.129:10055/api/protocol";
    private string getResultsUrl = "http://192.168.1.129:10055/api/protocol/results";

    private bool isInitialized = false;
    private bool dataLoaded = false; // 데이터 로딩 완료 플래그 추가

    async void Start()
    {
        // readonly 제거했으므로 경로 변경 가능
        LoadAllResults(); // 로컬 데이터 먼저 로드
        
        try 
        {
            var serverResults = await GetResultsFromServer();
            if (serverResults != null && serverResults.Any())
            {
                allResults.results = new List<TrainingResult>(serverResults);
                Debug.Log($"서버에서 {serverResults.Count}개의 기록을 가져왔습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"서버 데이터 동기화 실패: {e.Message}");
        }
        finally
        {
            dataLoaded = true; // 데이터 로딩 완료 표시
            isInitialized = true;
        }
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

    public static async void SaveNewResult(TrainingResult newResult)
    {
        // 디렉토리 존재 확인 및 생성
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 파일이 존재하면 먼저 읽어오기
        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            allResults = JsonUtility.FromJson<TrainingResultList>(existingJson);
        }

        allResults.results.Add(newResult);
        string json = JsonUtility.ToJson(allResults, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"기록 저장 완료: {filePath}");

        // 서버 전송은 인스턴스가 있는 경우에만
        var instance = FindObjectOfType<ResultHistoryManager>();
        if (instance != null)
        {
            await instance.SendResultToServer(newResult);
        }
        else
        {
            Debug.LogWarning("ResultHistoryManager 인스턴스가 없어 서버 전송은 스킵됩니다.");
        }
    }

    private async Task SendResultToServer(TrainingResult result)
    {
        string deviceId = GetShortHashString(SystemInfo.deviceUniqueIdentifier);
        
        var requestBody = new ProtocolData
        {
            userId = deviceId,
            protocolName = result.protocol_name,  // TrainingResult의 protocol_name을 protocolName으로 변환
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

    private async Task<List<TrainingResult>> GetResultsFromServer()
    {
        string deviceId = GetShortHashString(SystemInfo.deviceUniqueIdentifier);

        using (UnityWebRequest request = UnityWebRequest.Get($"{getResultsUrl}?userId={deviceId}"))
        {
            try
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"서버에서 데이터 조회 성공: {jsonResponse}");
                    
                    // 서버 응답 구조에 맞게 래퍼 클래스 사용
                    ServerResponse response = JsonUtility.FromJson<ServerResponse>(jsonResponse);
                    return response?.data ?? new List<TrainingResult>();
                }
                
                Debug.LogError($"서버 조회 실패: {request.error}");
                return new List<TrainingResult>();
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 조회 중 오류 발생: {e.Message}");
                return new List<TrainingResult>();
            }
        }
    }

    // 동기 방식의 GetResults 메서드 수정
    public List<TrainingResult> GetResults()
    {
        if (!dataLoaded) return new List<TrainingResult>();

        return allResults.results
            .OrderByDescending(r =>
            {
                DateTime date;
                if (DateTime.TryParse(r.date, out date))
                    return date;
                return DateTime.MinValue;
            })
            .Take(10)
            .ToList();
    }

    // 비동기 방식의 메서드명 변경
    public async Task<List<TrainingResult>> GetResultsAsync()
    {
        var serverResults = await GetResultsFromServer();
        
        if (serverResults != null && serverResults.Any())
        {
            var results = serverResults
                .OrderByDescending(r =>
                {
                    DateTime date;
                    if (DateTime.TryParse(r.date, out date))
                        return date;
                    else
                        return DateTime.MinValue;
                })
                .Take(10)
                .ToList();

            // 서버에서 받은 결과를 로컬 데이터에 동기화
            allResults.results = new List<TrainingResult>(results);
            string json = JsonUtility.ToJson(allResults, true);
            File.WriteAllText(filePath, json);
            
            return results;
        }
        
        return GetResults(); // 서버 조회 실패 시 로컬 데이터 반환
    }

    public bool IsDataLoaded() // 데이터 로딩 상태 확인 메서드 추가
    {
        return dataLoaded;
    }
}

[Serializable]
public class ProtocolData
{
    public string userId;
    public string protocolName;  // 서버로 전송할 때는 protocolName 사용
    public string date;
    public string duration;
    public string score;
}

[Serializable]
public class ServerResponse
{
    public List<TrainingResult> data;
    public string status;
}
