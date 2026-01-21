using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GearSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [Header("슬롯 UI 요소")]
    [SerializeField] private Image gearIcon;
    [SerializeField] private Image slotBackground;
    [SerializeField] private GameObject newIndicator;

    [Header("밝기 및 애니메이션 설정")]
    [Tooltip("선택되지 않았을 때의 어두운 정도 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.4f;
    [SerializeField] private float animationSpeed = 15f;
    // 기존 변수 선언 부분에 추가
    [Header("선택 효과")]
    [SerializeField] private GameObject selectionOutline; // 빛나는 테두리 이미지 연결 

    private GearData _myData;
    private GearPanelController _controller;
    private Button _button;

    private Color _originalIconColor;
    private Color _targetColor;
    private bool _isFocused = false;

    public GearData MyData { get { return _myData; } }

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (gearIcon != null)
        {
            _originalIconColor = gearIcon.color;
            // 초기화
            RecalculateTargetColor();
            gearIcon.color = _targetColor;
        }

        _button?.onClick.AddListener(HandleInteraction);

        // [추가] 시작할 때 아웃라인은 무조건 끕니다!
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }

    private void Update()
    {
        if (gearIcon != null)
        {
            gearIcon.color = Color.Lerp(gearIcon.color, _targetColor, Time.unscaledDeltaTime * animationSpeed);
        }
    }

    private void RecalculateTargetColor()
    {
        if (gearIcon == null) return;

        // 1. RGB (밝기) 계산
        Color tempColor;

        if (_isFocused)
        {
            // 포커스 상태: 밝게
            tempColor = _originalIconColor;
        }
        else
        {
            if (_myData != null && _myData.isEquipped)
            {
                // 장착 상태 (포커스 X): 밝게 유지 (식별 위해)
                tempColor = _originalIconColor;
            }
            else
            {
                // 미장착 상태 (포커스 X): 어둡게 (선택 안됨 표시)
                // *주의: dimFactor를 곱하면 Alpha도 같이 줄어드므로, 아래에서 Alpha를 다시 덮어써야 함
                tempColor = _originalIconColor * dimFactor;
            }
        }

        // 2. [핵심] Alpha (투명도) 강제 고정
        // 상태 상관없이 항상 225/255 (약 0.88)
        tempColor.a = 225f / 255f;

        _targetColor = tempColor;
    }

    public void SetData(GearData data, GearPanelController controller)
    {
        _myData = data;
        _controller = controller;

        if (gearIcon != null)
        {
            gearIcon.sprite = _myData.gearIcon;
            gearIcon.gameObject.SetActive(true);
        }
        if (_button != null) _button.interactable = true;
        if (newIndicator != null) newIndicator.SetActive(_myData.isNew);

        _isFocused = (EventSystem.current.currentSelectedGameObject == gameObject);
        RecalculateTargetColor();

        if (gearIcon != null) gearIcon.color = _targetColor;
    }

    public void ClearSlot()
    {
        _myData = null;
        _controller = null;
        _isFocused = false;

        if (gearIcon != null) { gearIcon.sprite = null; gearIcon.gameObject.SetActive(false); }
        if (_button != null) _button.interactable = false;
        if (newIndicator != null) newIndicator.SetActive(false);

        RecalculateTargetColor();
        if (gearIcon != null) gearIcon.color = _targetColor;
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }

    public void UpdateEquipVisual()
    {
        RecalculateTargetColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_myData != null && _controller != null)
        {
            _controller.ShowSelectedGearDetails(_myData);
            if (_button.interactable) _button.Select();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isFocused = true;
        RecalculateTargetColor();

        // [추가] 아웃라인 켜기
        if (selectionOutline != null) selectionOutline.SetActive(true);

        if (_myData != null && _controller != null)
        {
            _controller.ShowSelectedGearDetails(_myData);
        }
    }

    // OnDeselect 함수 수정
    public void OnDeselect(BaseEventData eventData)
    {
        _isFocused = false;
        RecalculateTargetColor();

        // [추가] 아웃라인 끄기
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }

    private void HandleInteraction()
    {
        if (_myData != null && _controller != null)
        {
            if (_myData.isNew)
            {
                _myData.isNew = false;
                if (newIndicator != null) newIndicator.SetActive(false);

                int find = DBManager.I.currData.gearDatas.FindIndex(x => x.Name == _myData.name);
                if (find != -1)
                {
                    CharacterData.GearData cd = DBManager.I.currData.gearDatas[find];
                    cd.isNew = false;
                    DBManager.I.currData.gearDatas[find] = cd;
                }
            }

            _controller.ToggleEquipGear(_myData);
        }
    }
}