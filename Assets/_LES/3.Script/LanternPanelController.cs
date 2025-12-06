using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using Unity.Collections;

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

    private void OnEnable()
    {
        // if (InventoryDataManager.Instance != null)
        // {
        //     InventoryDataManager.Instance.OnLanternsChanged += RefreshPanel;
        // }
    }

    private void OnDisable()
    {
        // if (InventoryDataManager.Instance != null)
        // {
        //     InventoryDataManager.Instance.OnLanternsChanged -= RefreshPanel;
        // }
    }

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
        //if (InventoryDataManager.Instance == null) return; // 매니저가 없으면 중단

        //////////
        List<LanternFunctionData> playerFunctions = new List<LanternFunctionData>();
        for (int i = 0; i < DBManager.I.currData.lanternDatas.Count; i++)
        {
            CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[i];
            int find = DBManager.I.itemDatabase.allLanterns.FindIndex(x => x.name == cd.Name);
            if (find == -1) continue;
            LanternFunctionData d = DBManager.I.itemDatabase.allLanterns[find];
            playerFunctions.Add(d);
        }
        //////////
        

        for (int i = 0; i < functionSlots.Count; i++)
        {
            if (i < playerFunctions.Count && playerFunctions[i] != null && !string.IsNullOrEmpty(playerFunctions[i].functionName))
            {
                functionSlots[i].SetData(playerFunctions[i], this);
            }
            else
            {
                functionSlots[i].ClearSlot();
            }
        }

        UpdateMainEquippedImage();
        SetupNavigation();

        LanternFunctionData firstAvailableFunc = playerFunctions.FirstOrDefault(f => f != null && !string.IsNullOrEmpty(f.functionName));
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

        // [수정] InventoryDataManager의 데이터를 사용



        for (int i = 0; i < DBManager.I.currData.lanternDatas.Count; i++)
        {
            CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[i];
            cd.isEquipped = false;
            DBManager.I.currData.lanternDatas[i] = cd;
        }



        if (!wasEquipped)
        {
            dataToToggle.isEquipped = true;
        }

        foreach (var slot in functionSlots)
        {
            slot.UpdateEquipVisual();
        }

        UpdateMainEquippedImage();
    }

    /// <summary>
    /// (2번 요청) 메인 장착부(빨간 원)의 이미지를 갱신합니다.
    /// </summary>
    private void UpdateMainEquippedImage()
    {
        //LanternFunctionData equippedFunction = InventoryDataManager.Instance.PlayerLanternFunctions.FirstOrDefault(f => f.isEquipped);
        CharacterData.LanternData cd = DBManager.I.currData.lanternDatas.FirstOrDefault(f => f.isEquipped);
        int find = DBManager.I.itemDatabase.allLanterns.FindIndex(x => x.functionName == cd.Name);
        if(find == -1) return;
        LanternFunctionData equippedFunction = DBManager.I.itemDatabase.allLanterns[find];

        if (equippedFunction != null)
        {
            equippedFunctionImage.sprite = equippedFunction.functionIcon;
            equippedFunctionImage.gameObject.SetActive(true);
        }
        else
        {
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


}