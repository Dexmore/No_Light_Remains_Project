using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// [수정] ISubmitHandler 제거
public class LanternSlotUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler
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
        if (functionIcon != null) _originalIconColor = functionIcon.color;

        // Button의 onClick이 엔터 키와 클릭을 모두 처리함
        _button?.onClick.AddListener(HandleInteraction);
    }

    // ... (SetData, ClearSlot, UpdateEquipVisual 함수는 기존 그대로 유지) ...
    public void SetData(LanternFunctionData data, LanternPanelController controller)
    {
        _myData = data;
        _controller = controller;
        if (functionIcon != null) { functionIcon.sprite = _myData.functionIcon; functionIcon.gameObject.SetActive(true); }
        if (_button != null) _button.interactable = true;
        UpdateEquipVisual();
        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;
        if (functionIcon != null) { functionIcon.sprite = null; functionIcon.gameObject.SetActive(false); }
        if (_button != null) _button.interactable = false;
        if (newIndicator != null) newIndicator.SetActive(false);
    }

    public void UpdateEquipVisual()
    {
        if (_myData == null || functionIcon == null) return;
        functionIcon.color = _myData.isEquipped ? _originalIconColor : (_originalIconColor * dimFactor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
            if (_button.interactable) _button.Select();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowFunctionDetails(_myData);
        }
    }

    // [삭제] OnSubmit 함수 전체 삭제
    // public void OnSubmit(BaseEventData eventData) { ... }

    private void HandleInteraction()
    {
        if (_myData == null || _controller == null) return;

        if (_myData.isNew)
        {
            _myData.isNew = false;
            if (newIndicator != null) newIndicator.SetActive(false);

            /////
            int find = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == _myData.name);
            if (find != -1)
            {
                CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[find];
                cd.isNew = false;
                DBManager.I.currData.lanternDatas[find] = cd;
            }
            /////
        }

        _controller.ToggleEquipFunction(_myData);
    }
}