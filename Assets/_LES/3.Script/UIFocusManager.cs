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

    // [추가] 셀렉터의 최소 크기 설정
    [Tooltip("셀렉터가 이 크기보다 작아지지 않도록 합니다.")]
    [SerializeField] private Vector2 minSize = new Vector2(100f, 100f);

    private RectTransform _currentTarget;
    
    private void Awake()
    {
        if (focusFrame != null) focusFrame.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (selectedObj == null || !selectedObj.transform.IsChildOf(uiRoot))
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

            // 위치 이동
            focusFrame.transform.position = Vector3.Lerp(
                focusFrame.transform.position, 
                _currentTarget.transform.position, 
                Time.unscaledDeltaTime * moveSpeed
            );

            // [수정] 목표 크기 계산 시 최소 크기(minSize) 적용
            Vector2 targetSize = _currentTarget.sizeDelta + padding;
            targetSize.x = Mathf.Max(targetSize.x, minSize.x);
            targetSize.y = Mathf.Max(targetSize.y, minSize.y);

            // 크기 변경
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