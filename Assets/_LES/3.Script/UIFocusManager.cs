using UnityEngine;
using UnityEngine.EventSystems; // EventSystem을 사용하기 위해 필수!

public class UIFocusManager : MonoBehaviour
{
    [Header("포커스 프레임 설정")]
    [Tooltip("Hierarchy에 만든 FocusFrame 오브젝트의 RectTransform을 연결")]
    [SerializeField]
    private RectTransform focusFrame;

    [Tooltip("UI의 루트 패널입니다. (예: InventoryPanels)")]
    [SerializeField]
    private RectTransform uiRoot;

    [Header("움직임 설정")]
    [Tooltip("프레임이 따라가는 속도")]
    [SerializeField]
    private float moveSpeed = 20f;
    
    [Tooltip("프레임의 크기가 변경되는 속도")]
    [SerializeField]
    private float sizeSpeed = 15f;
    
    [Tooltip("선택한 대상보다 얼마나 더 크게 표시할지 (패딩)")]
    [SerializeField]
    private Vector2 padding = new Vector2(10f, 10f);

    private RectTransform _currentTarget; // 현재 쫓아갈 대상
    
    private void Awake()
    {
        // 시작할 때 프레임을 숨김
        if (focusFrame != null)
        {
            focusFrame.gameObject.SetActive(false);
        }
    }

    // Update는 매 프레임마다 호출됩니다.
    private void Update()
    {
        // 1. EventSystem이 현재 선택 중인 게임 오브젝트를 가져옴
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        // 2. 선택된 오브젝트가 없거나, 우리 UI(uiRoot)의 자식이 아니면
        if (selectedObj == null || !selectedObj.transform.IsChildOf(uiRoot))
        {
            _currentTarget = null; // 대상을 null로 설정 (프레임을 숨김)
        }
        else
        {
            // 3. 선택된 오브젝트가 있으면 대상을 설정
            _currentTarget = selectedObj.GetComponent<RectTransform>();
        }

        // 4. 대상(Target)이 있을 경우
        if (_currentTarget != null)
        {
            // 4a. 프레임을 활성화
            focusFrame.gameObject.SetActive(true);

            // 4b. 부드럽게(Lerp) 프레임의 위치를 대상의 위치로 이동
            focusFrame.transform.position = Vector3.Lerp(
                focusFrame.transform.position, 
                _currentTarget.transform.position, 
                Time.unscaledDeltaTime * moveSpeed
            );

            // 4c. 부드럽게(Lerp) 프레임의 크기를 대상의 크기 + 패딩으로 변경
            focusFrame.sizeDelta = Vector2.Lerp(
                focusFrame.sizeDelta, 
                _currentTarget.sizeDelta + padding, 
                Time.unscaledDeltaTime * sizeSpeed
            );
        }
        // 5. 대상(Target)이 없을 경우
        else
        {
            // 5a. 프레임을 비활성화
            focusFrame.gameObject.SetActive(false);
        }
    }
}