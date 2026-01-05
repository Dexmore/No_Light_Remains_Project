using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // [필수] New Input System 네임스페이스
using System.Collections.Generic;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        // 마우스가 연결되어 있지 않으면 무시
        if (Mouse.current == null) return;

        // [수정] 마우스 왼쪽 클릭 감지 (New Input System 방식)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 1. 이벤트 시스템 체크
            if (EventSystem.current == null)
            {
                Debug.LogError("❌ [ClickDebugger] EventSystem이 씬에 없습니다!");
                return;
            }

            // 2. 마우스 위치 가져오기 (New Input System 방식)
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 3. 레이캐스트 쏘기
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            // 4. 결과 출력
            if (results.Count > 0)
            {
                GameObject topObject = results[0].gameObject;
                Debug.Log($"🛑 [클릭 차단 범인] : {topObject.name} (부모: {topObject.transform.parent?.name})");

                for(int i = 1; i < results.Count; i++)
                {
                    Debug.Log($"   ㄴ 아래 깔림: {results[i].gameObject.name}");
                }
            }
            else
            {
                Debug.Log("💨 [허공 클릭] 마우스 아래에 'Raycast Target'이 켜진 UI가 없습니다.");
            }
        }
    }
}