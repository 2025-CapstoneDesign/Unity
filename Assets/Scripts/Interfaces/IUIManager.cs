using System.Collections;
using UnityEngine;

public interface IUIManager
{
    // UI 초기화
    void InitializeUI();
    
    // 메시지 표시
    void SetMessage(object state);
    
    // 완료 메시지 표시
    void ShowCompleteMessage();
    
    // UI 요소 표시/숨기기 제어
    void ShowCompressionUI(bool visible);
    void ShowBreathUI(bool visible);
    void ShowCountText(bool visible);
    
    // 값 업데이트
    void SetCompressionForce(float value);
    void SetBreathForce(float value);
    void UpdateCountText(string text);
    
    // 아이콘 표시 
    void ShowCheckIconPass(MonoBehaviour context);
    void ShowCheckIconFail(MonoBehaviour context);
    // 코루틴 시작 메서드 추가
    void StartHideCompressionUICoroutine(float seconds);
    void StartHideBreathUICoroutine(float seconds);
    // 지연된 UI 숨기기
    IEnumerator HideCompressionUIWithDelay(float seconds);
    IEnumerator HideBreathUIWithDelay(float seconds);
}