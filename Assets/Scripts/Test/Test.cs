using System.Collections;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("오브젝트 활성 상태: " + gameObject.activeInHierarchy);
        StartCoroutine(TestCoroutine());
    }

    IEnumerator TestCoroutine()
    {
        while (true)
        {
            Debug.Log("코루틴 실행 중: " + Time.time);
            yield return new WaitForSecondsRealtime(1f); // Time.timeScale 영향 안 받음
        }
    }
}
