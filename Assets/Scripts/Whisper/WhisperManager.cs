using System.Collections;
using UnityEngine;

public class WhisperManager : MonoBehaviour
{
    public delegate void OnRandomInputGenerated(bool randomInput);
    public event OnRandomInputGenerated RandomInputGenerated;  // 랜덤 값 생성 시 호출될 이벤트

    private void Start()
    {
        // 1초마다 랜덤값을 발생시키는 코루틴 시작
        Debug.Log("Start");
        StartCoroutine(GenerateRandomInput());
    }

    // Change this method to public so it can be accessed by AEDManager
    public IEnumerator GenerateRandomInput()
    {
        while (true)
        {
            // 1초마다 랜덤 TRUE/FALSE 값을 생성
            bool randomInput = Random.value > 0.5f;  // 50% 확률로 TRUE 또는 FALSE 생성
            if (randomInput == false) {
                Debug.Log("false");
            }
            else {
                Debug.Log("true");
            }
            RandomInputGenerated?.Invoke(randomInput);  // 이벤트 호출

            yield return new WaitForSeconds(1f);  // 1초 간격으로 값 생성
        }
    }
}
