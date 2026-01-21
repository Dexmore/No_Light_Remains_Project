using UnityEngine;
using NaughtyAttributes;

public class Inventory : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private void Awake()
    {
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        // 초기 상태는 비활성화
        if (_canvasGroup != null) // 안전장치
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.transform.parent.gameObject.SetActive(false);
        }
    }

    [Button]
    // UI를 열 때 호출할 함수 (즉시 활성화)
    public void Open()
    {
        if (_canvasGroup == null) return; // 안전장치 추가

        _canvasGroup.transform.parent.gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
    }

    [Button]
    // UI를 닫을 때 호출할 함수 (즉시 비활성화)
    public void Close()
    {
        // 👇 [핵심 수정] 캔버스 그룹이 이미 파괴되었거나 없으면 아무것도 하지 말고 돌아가라!
        if (_canvasGroup == null) return; 

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        
        // 부모 오브젝트 접근 시에도 안전하게 체크
        if (_canvasGroup.transform.parent != null)
        {
            _canvasGroup.transform.parent.gameObject.SetActive(false);
        }
    }
}