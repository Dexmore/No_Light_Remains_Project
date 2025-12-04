using UnityEngine;
using UnityEngine.EventSystems; 

public class UIFocusManager : MonoBehaviour
{
    [Header("포커스 프레임 설정")]
    [SerializeField] private RectTransform focusFrame;
    [SerializeField] private RectTransform uiRoot;

    [Header("움직임 설정")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float sizeSpeed = 15f;
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f);
    [Tooltip("셀렉터가 이 크기보다 작아지지 않도록 합니다.")]
    [SerializeField] private Vector2 minSize = new Vector2(100f, 100f);

    private RectTransform _currentTarget;
    
    private void Awake()
    {
        if (focusFrame != null) focusFrame.gameObject.SetActive(false);
    }

    // [추가] 매니저가 꺼질 때(인벤토리 닫힐 때) 프레임도 같이 숨김
    private void OnDisable()
    {
        if (focusFrame != null) focusFrame.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // [추가] UI 루트가 꺼져있으면 작동 중지
        if (uiRoot != null && !uiRoot.gameObject.activeInHierarchy)
        {
            if (focusFrame.gameObject.activeSelf) focusFrame.gameObject.SetActive(false);
            return;
        }

        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        // 선택된 오브젝트가 없거나, UI 자식이 아니거나, **비활성화된 상태라면** 타겟 해제
        if (selectedObj == null || !selectedObj.activeInHierarchy || !selectedObj.transform.IsChildOf(uiRoot))
        {
            _currentTarget = null;
        }
        else
        {
            _currentTarget = selectedObj.GetComponent<RectTransform>();
        }

        if (_currentTarget != null)
        {
            focusFrame.gameObject.SetActive(true);

            focusFrame.transform.position = Vector3.Lerp(
                focusFrame.transform.position, 
                _currentTarget.transform.position, 
                Time.unscaledDeltaTime * moveSpeed
            );

            Vector2 targetSize = _currentTarget.sizeDelta + padding;
            targetSize.x = Mathf.Max(targetSize.x, minSize.x);
            targetSize.y = Mathf.Max(targetSize.y, minSize.y);

            focusFrame.sizeDelta = Vector2.Lerp(
                focusFrame.sizeDelta, 
                targetSize, 
                Time.unscaledDeltaTime * sizeSpeed
            );
        }
        else
        {
            focusFrame.gameObject.SetActive(false);
        }
    }
}