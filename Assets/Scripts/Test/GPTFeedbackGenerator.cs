using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GPTFeedbackGenerator : MonoBehaviour
{
    private string apiKey;
    private string apiUrl;
    private string model;
    [SerializeField] private float temperature = 0.7f;

    void Start()
    {
        LoadAPIConfig();
    }

    private void LoadAPIConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "APIConfig.json");
        if (File.Exists(configPath))
        {
            Debug.Log("챗지피티 연동 준비 완료~");
            string json = File.ReadAllText(configPath);
            var config = JsonUtility.FromJson<APIConfig>(json);
            apiKey = config.OpenAI.APIKey;
            apiUrl = config.OpenAI.APIUrl;
            model = config.OpenAI.Model;
        }
        else
        {
            Debug.LogError("API 설정 파일을 찾을 수 없습니다: " + configPath);
        }
    }

    [Serializable]
    private class APIConfig
    {
        public OpenAIConfig OpenAI;
    }

    [Serializable]
    private class OpenAIConfig
    {
        public string APIKey;
        public string APIUrl;
        public string Model;
    }

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
    }

    [Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    private class Choice
    {
        public Message message;
    }

    public IEnumerator GenerateFeedback(Dictionary<string, int> checkScore, String Stage, Action<string> onComplete)
    {
        string prompt = CreatePrompt(checkScore, Stage);
        yield return StartCoroutine(SendRequest(prompt, onComplete));
    }

    private IEnumerator SendRequest(string prompt, Action<string> onComplete)
    {
        ChatRequest requestBody = new ChatRequest
        {
            model = model,
            temperature = temperature,
            messages = new List<Message>
            {
                new Message
                {
                    role = "system",
                    content = "당신은 응급구조사 2급 자격증 평가위원입니다. 훈련자의 오류를 분석하고 구체적인 피드백을 제공해주세요."
                },
                new Message
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        // 💡 JsonUtility는 List<Message>를 배열로 인식 못하는 경우가 있어 수동 변환 필요
        jsonBody = jsonBody.Replace("\"messages\":{", "\"messages\":[{").Replace("}}", "}]}");

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(request.downloadHandler.text);
                    onComplete?.Invoke(response.choices[0].message.content);
                }
                catch (Exception e)
                {
                    Debug.LogError($"응답 처리 중 오류 발생: {e.Message}");
                    onComplete?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"API 요청 실패: {request.error}\n{request.downloadHandler.text}");
                onComplete?.Invoke(null);
            }
        }
    }

    private string CreatePrompt(Dictionary<string, int> checkScore, String Stage)
    {
        string prompt = $"당신은 응급구조사 2급 실기시험 평가관입니다. 아래는 한 훈련자가 {Stage} 훈련 중 범한 실수 목록입니다. 이 훈련자는 자격증 시험을 준비 중이며, 실전 상황에 가까운 피드백을 원합니다. 현실적인 개선 방향을 제시하고, 실제 시험에서 도움이 될 수 있는 조언을 **하나의 자연스러운 문단**으로 작성해주세요.\n\n";

        prompt += "📋 훈련 중 오류:\n";
        foreach (var item in checkScore)
        {
            prompt += $"- {item.Key}: {item.Value}회\n";
        }

        prompt += "\n✏️ 피드백 작성 가이드:\n";
        prompt += "- 전체적인 평가 (단, '점수' 언급은 하지 마세요)\n";
        prompt += "- 각 오류에 대한 **구체적인 개선 방법**을 포함해주세요 (예: 압박 깊이, 속도, 인공호흡 방법 등)\n";
        prompt += "- 자격시험 대비를 위한 실용적인 조언을 주세요\n";
        prompt += "- 막연한 격려보다는 훈련자가 어떤 연습을 해야 하는지 중점적으로 작성해주세요\n";
        prompt += "- **형식은 목록 없이 하나의 단락**으로 정리하고, **최대 5줄 이내**로 작성해주세요\n";

        return prompt;
    }

}
