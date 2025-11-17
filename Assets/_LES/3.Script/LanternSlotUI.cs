using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanternSlotUI : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image functionIcon;
    [SerializeField] private GameObject newIndicator;

    [Header("비장착 상태 밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.5f;

    private LanternFunctionData _myData;
    private LanternPanelController _controller;
    private Button _button;
    private Color _originalIconColor;
    
    public LanternFunctionData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (functionIcon != null)
        {
            _originalIconColor = functionIcon.color;
        }
        
        //OnSubmit(Enter)과 OnClick(마우스)이 모두 HandleInteraction을 호출하도록
        _button?.onClick.AddListener(HandleInteraction);
    }

    public void SetData(LanternFunctionData data, LanternPanelController controller)
    {
        _myData = data;
        _controller = controller;

        if (functionIcon != null)
        {
            functionIcon.sprite = _myData.functionIcon;
            functionIcon.gameObject.SetActive(true);
        }
        if (_button != null) _button.interactable = true;
        
        UpdateEquipVisual();

        //"New" 표시 설정
        if (newIndicator != null)
        {
            newIndicator.SetActive(_myData.isNew);
        }
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;

        if (functionIcon != null)
        {
            functionIcon.sprite = null;
            functionIcon.gameObject.SetActive(false);
        }
        if (_button != null) _button.interactable = false;
        
        //빈 슬롯이면 "New" 표시 끄기
        if (newIndicator != null)
        {
            newIndicator.SetActive(false);
        }
    }
    
    //(UpdateEquipVisual, OnSelect 함수는 기존과 동일)
    #region (수정 없는 함수들)
    public void UpdateEquipVisual()
    {
        if (_myData == null || functionIcon == null) return;
        functionIcon.color = _myData.isEquipped ? _originalIconColor : (_originalIconColor * dimFactor);
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }
    #endregion
    
    //'Submit'(Enter/Space) 시 호출
    public void OnSubmit(BaseEventData eventData)
    {
        HandleInteraction();
    }

    //클릭 또는 Enter/Space (Submit) 시 호출되는 공통 함수
    private void HandleInteraction()
    {
        if (_myData == null || _controller == null) return;
        
        //아이템을 장착/해제(상호작용)하면 "New" 표시를 끕니다.
        if (_myData.isNew)
        {
            _myData.isNew = false;
            if (newIndicator != null)
            {
                newIndicator.SetActive(false);
            }
        }
        
        //컨트롤러에게 장착/해제 시도 알림
        _controller.ToggleEquipFunction(_myData);
    }
}