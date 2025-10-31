using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using NaughtyAttributes;

public class LanternPanelController : MonoBehaviour, ITabContent
{
    [Header("슬롯 설정 (파란색)")]
    [Tooltip("3개의 LanternSlotUI를 순서대로 등록")]
    [SerializeField] private List<LanternSlotUI> functionSlots; // 1번 요청
    
    [Header("장착부 (왼쪽 위 빨간색)")]
    [SerializeField] private Image equippedFunctionImage; // 2번 요청

    [Header("상세 정보 (오른쪽 빨간색)")]
    [SerializeField] private TextMeshProUGUI detailNameText; // 3번 요청
    [SerializeField] private TextMeshProUGUI detailDescriptionText; // 3번 요청
    [SerializeField] private GameObject detailPanelRoot; // 정보창 전체 (선택사항)

    [Header("내비게이션")]
    [Tooltip("슬롯에서 위로 갔을 때 선택될 탭 버튼 (예: '랜턴' 탭 버튼)")]
    [SerializeField] private Selectable mainTabButton;

    [Header("플레이어 랜턴 기능 (데이터)")]
    [SerializeField] private List<LanternFunctionData> _playerLanternFunctions;

    public void OnShow()
    {
        RefreshPanel();
        
        // 활성화된 첫 번째 슬롯을 찾아 선택
        LanternSlotUI firstInteractableSlot = functionSlots.FirstOrDefault(slot => slot.GetComponent<Button>().interactable);
        if (firstInteractableSlot != null)
        {
            firstInteractableSlot.GetComponent<Button>().Select();
        }
        else
        {
            mainTabButton?.Select();
        }
    }

    public void OnHide()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// 패널 전체를 현재 데이터 기준으로 새로고침합니다.
    /// </summary>
    private void RefreshPanel()
    {
        // 1. 3개의 슬롯에 데이터 할당
        for (int i = 0; i < functionSlots.Count; i++)
        {
            if (i < _playerLanternFunctions.Count && _playerLanternFunctions[i] != null && !string.IsNullOrEmpty(_playerLanternFunctions[i].functionName))
            {
                functionSlots[i].SetData(_playerLanternFunctions[i], this);
            }
            else
            {
                functionSlots[i].ClearSlot(); // 비활성화
            }
        }
        
        // 2. 장착된 기능 이미지 갱신 (2번 요청)
        UpdateMainEquippedImage();
        
        // 3. 내비게이션 설정 (1번 요청)
        SetupNavigation();

        // 4. 상세 정보창 갱신 (3번 요청)
        LanternFunctionData firstAvailableFunc = _playerLanternFunctions.FirstOrDefault(f => f != null && !string.IsNullOrEmpty(f.functionName));
        ShowFunctionDetails(firstAvailableFunc);
    }

    /// <summary>
    /// [공개] 슬롯에서 호출. 선택된 기능 정보를 오른쪽에 표시 (3번 요청)
    /// </summary>
    public void ShowFunctionDetails(LanternFunctionData data)
    {
        if (data != null)
        {
            if (detailPanelRoot != null) detailPanelRoot.SetActive(true);
            detailNameText.text = data.functionName;
            detailDescriptionText.text = data.functionDescription;
        }
        else
        {
            if (detailPanelRoot != null) detailPanelRoot.SetActive(false);
            detailNameText.text = "빛 이름";
            detailDescriptionText.text = "기능을 선택하세요.";
        }
    }

    /// <summary>
    /// [공개] 슬롯에서 호출. 기능 장착/해제 토글
    /// </summary>
    public void ToggleEquipFunction(LanternFunctionData dataToToggle)
    {
        bool wasEquipped = dataToToggle.isEquipped;

        // 1. 모든 기능의 장착 상태를 '해제'로 초기화
        foreach (var funcData in _playerLanternFunctions)
        {
            if (funcData != null) funcData.isEquipped = false;
        }
        
        // 2. 이전에 장착되지 않았던 아이템이라면, '장착' 상태로 변경
        // (랜턴은 기어와 달리 하나만 장착 가능 = 라디오 버튼)
        if (!wasEquipped)
        {
            dataToToggle.isEquipped = true;
        }
        // (이미 장착된 걸 또 누르면 1번 단계에서 해제만 되고 끝남)

        // 3. 모든 슬롯의 시각적 상태(어둡게) 갱신 (2번 요청)
        foreach (var slot in functionSlots)
        {
            slot.UpdateEquipVisual();
        }

        // 4. 메인 장착부 이미지 갱신 (2번 요청)
        UpdateMainEquippedImage();
    }

    /// <summary>
    /// (2번 요청) 메인 장착부(빨간 원)의 이미지를 갱신합니다.
    /// </summary>
    private void UpdateMainEquippedImage()
    {
        LanternFunctionData equippedFunction = _playerLanternFunctions.FirstOrDefault(f => f.isEquipped);

        if (equippedFunction != null)
        {
            equippedFunctionImage.sprite = equippedFunction.functionIcon;
            equippedFunctionImage.gameObject.SetActive(true);
        }
        else
        {
            // 장착된 게 없으면 이미지를 숨김
            equippedFunctionImage.sprite = null;
            equippedFunctionImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 3칸 슬롯의 좌/우/위 내비게이션을 설정합니다. (1번 요청)
    /// </summary>
    private void SetupNavigation()
    {
        // 1. 활성화된 슬롯 리스트 생성
        List<Button> interactableSlots = new List<Button>();
        foreach (var slot in functionSlots)
        {
            Button btn = slot.GetComponent<Button>();
            if (btn.interactable) interactableSlots.Add(btn);
        }

        if (interactableSlots.Count == 0) return;

        // 2. 활성화된 슬롯끼리만 연결
        for (int i = 0; i < interactableSlots.Count; i++)
        {
            Button currentButton = interactableSlots[i];
            Navigation nav = currentButton.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // 'Up'은 탭으로 탈출
            nav.selectOnUp = mainTabButton;
            nav.selectOnDown = null; // 아래는 막음

            // 'Left' (래핑)
            nav.selectOnLeft = interactableSlots[(i - 1 + interactableSlots.Count) % interactableSlots.Count];
            // 'Right' (래핑)
            nav.selectOnRight = interactableSlots[(i + 1) % interactableSlots.Count];

            currentButton.navigation = nav;
        }
    }
    
    #region 테스트용 코드

    // [추가] 인스펙터에서 테스트용으로 추가할 랜턴 기능을 미리 할당
    [Header("테스트용")]
    [SerializeField] private LanternFunctionData testLanternFunctionToAdd;

    [Button("Test: 랜턴 기능 추가")]
    private void TestAddLanternFunction()
    {
        if (testLanternFunctionToAdd == null)
        {
            Debug.LogWarning("테스트할 랜턴 기능을 인스펙터 필드에 할당해주세요!");
            return;
        }

        // 1. 데이터 리스트에 기능을 추가합니다.
        _playerLanternFunctions.Add(testLanternFunctionToAdd);

        // 2. UI를 새로고침합니다.
        OnShow(); 

        Debug.Log($"{testLanternFunctionToAdd.functionName} 랜턴 기능 추가 테스트 완료.");
    }

    #endregion
}