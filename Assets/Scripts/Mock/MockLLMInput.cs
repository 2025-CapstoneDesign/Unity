using System.Collections;
using UnityEngine;

public class MockLLMInput : MonoBehaviour
{
    [SerializeField] private AEDManager aedManager;
    [SerializeField] private float inputInterval = 10f; // 입력 주기 (초)

    void Start()
    {
        StartCoroutine(SendRandomInput());
    }

    private IEnumerator SendRandomInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(inputInterval);

            bool randomResult = Random.value > 0.5f;
            Debug.Log($"🧪 Mock 입력 전송: {(randomResult ? "성공" : "실패")}");

            aedManager.ReceiveInputResult(randomResult);
        }
    }
}
