using UnityEngine;

public class PrintPositions : MonoBehaviour
{
    public GameObject targetObject; // 확인하고 싶은 오브젝트를 인스펙터에서 드래그해서 연결해줘!

    void Update()
    {
        if (targetObject != null)
        {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 objectPos = targetObject.transform.position;

            Debug.Log($"[카메라 위치] X:{cameraPos.x:F2}, Y:{cameraPos.y:F2}, Z:{cameraPos.z:F2}");
            Debug.Log($"[오브젝트 위치] X:{objectPos.x:F2}, Y:{objectPos.y:F2}, Z:{objectPos.z:F2}");
        }
        else
        {
            Debug.LogWarning("targetObject 가 연결되지 않았습니다.");
        }
    }
}
