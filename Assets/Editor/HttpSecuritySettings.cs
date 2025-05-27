using UnityEditor;
using UnityEngine;

public class HttpSecuritySettings
{
    [InitializeOnLoadMethod]
    static void EnableHttpInEditor()
    {
#if UNITY_EDITOR
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        Debug.Log("HTTP 통신이 Unity 에디터에서 허용되도록 설정되었습니다.");
#endif
    }

    [MenuItem("Tools/Security/Allow HTTP Communications")]
    public static void EnableHttpManually()
    {
        PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
        Debug.Log("HTTP 통신이 Unity 에디터에서 허용되도록 설정되었습니다.");
    }
}