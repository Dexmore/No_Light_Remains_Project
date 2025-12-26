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

    /// 패널 전체를 현재 데이터 기준으로 새로고침합니다.
    private void RefreshPanel()
    {
        //if (InventoryDataManager.Instance == null) return; // 매니저가 없으면 중단

        List<LanternFunctionData> playerFunctions = new List<LanternFunctionData>();
        for (int i = 0; i < DBManager.I.currData.lanternDatas.Count; i++)
        {
            CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[i];
            int find = DBManager.I.itemDatabase.allLanterns.FindIndex(x => x.name == cd.Name);
            if (find == -1) continue;
            LanternFunctionData d = Instantiate(DBManager.I.itemDatabase.allLanterns[find]);
            d.name = DBManager.I.itemDatabase.allLanterns[find].name;
            d.isEquipped = cd.isEquipped;
            d.isNew = cd.isNew;
            playerFunctions.Add(d);
        }
        
        for (int i = 0; i < functionSlots.Count; i++)
        {
            if (i < playerFunctions.Count && playerFunctions[i] != null && !string.IsNullOrEmpty(playerFunctions[i].functionName.GetLocalizedString()))
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

        LanternFunctionData firstAvailableFunc = playerFunctions.FirstOrDefault(f => f != null && !string.IsNullOrEmpty(f.functionName.GetLocalizedString()));
        ShowFunctionDetails(firstAvailableFunc);
    }

    // [공개] 슬롯에서 호출. 선택된 기능 정보를 오른쪽에 표시 (3번 요청)
    public void ShowFunctionDetails(LanternFunctionData data)
    {
        if (data != null)
        {
            if (detailPanelRoot != null) detailPanelRoot.SetActive(true);
            detailNameText.text = data.functionName.GetLocalizedString();
            detailDescriptionText.text = data.functionDescription.GetLocalizedString();
        }
        else
        {
            if (detailPanelRoot != null) detailPanelRoot.SetActive(false);
            detailNameText.text = "빛 이름";
            detailDescriptionText.text = "기능을 선택하세요.";
        }
    }
    
    /// [공개] 슬롯에서 호출. 기능 장착/해제 토글   
    public void ToggleEquipFunction(LanternFunctionData dataToToggle)
    {
        // -----------------------------------------------------------
        // [추가된 로직] 현재 장착 중인 랜턴이 "탈부착 불가"인지 확인
        // -----------------------------------------------------------
        
        // 1. DB에서 현재 장착된(isEquipped == true) 데이터 찾기
        var dbLanterns = DBManager.I.currData.lanternDatas;
        int equippedIndex = dbLanterns.FindIndex(x => x.isEquipped);

        if (equippedIndex != -1)
        {
            string equippedName = dbLanterns[equippedIndex].Name;
            
            // 2. 해당 랜턴의 원본 데이터(.asset)를 가져와서 설정 확인
            LanternFunctionData equippedAsset = DBManager.I.itemDatabase.allLanterns.Find(x => x.name == equippedName);
            
            // 3. 만약 현재 장착된 놈이 '고정형(isRemovable == false)'이라면?
            if (equippedAsset != null && !equippedAsset.isRemovable)
            {
                Debug.Log($"[Lantern] '{equippedAsset.name}'은 해제할 수 없는 기본 아이템 입니다.");
                // (필요하다면 여기에 "해제할 수 없습니다" 팝업 띄우는 코드 추가)
                return; 
            }
        }
        
        bool wasEquipped = dataToToggle.isEquipped;

        // 1. [DB 동기화] 모든 랜턴 장착 해제
        for (int i = 0; i < DBManager.I.currData.lanternDatas.Count; i++)
        {
            CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[i];
            cd.isEquipped = false; 
            DBManager.I.currData.lanternDatas[i] = cd;
        }

        // 2. [DB 동기화] 선택한 랜턴이 원래 꺼져있었다면 -> 켭니다.
        if (!wasEquipped)
        {
            dataToToggle.isEquipped = true;
            int findIndex = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == dataToToggle.name);
            if (findIndex != -1)
            {
                CharacterData.LanternData cd = DBManager.I.currData.lanternDatas[findIndex];
                cd.isEquipped = true;
                DBManager.I.currData.lanternDatas[findIndex] = cd;
            }
        }
        else
        {
            dataToToggle.isEquipped = false;
        }

        // 3. UI 시각적 갱신
        foreach (var slot in functionSlots)
        {
            if (slot.MyData != null)
            {
                if (slot.MyData != dataToToggle) slot.MyData.isEquipped = false;
                slot.UpdateEquipVisual();
            }
        }

        UpdateMainEquippedImage();
    }

    private void UpdateMainEquippedImage()
    {
        // 1. DB에서 현재 장착 중인(isEquipped == true) 랜턴 데이터를 찾습니다.
        CharacterData.LanternData cd = DBManager.I.currData.lanternDatas.FirstOrDefault(f => f.isEquipped);

        // 2. 장착된 데이터가 없다면 (이름이 비어있다면) -> 이미지를 끄고 종료
        if (string.IsNullOrEmpty(cd.Name))
        {
            if (equippedFunctionImage != null)
            {
                equippedFunctionImage.sprite = null;
                equippedFunctionImage.gameObject.SetActive(false);
            }
            return;
        }

        // 3. 아이템 데이터베이스에서 원본 데이터(.asset)를 찾는다.
        int find = DBManager.I.itemDatabase.allLanterns.FindIndex(x => x.name == cd.Name);
        
        if(find != -1)
        {
            // 4. 찾았으면 이미지 교체 및 활성화
            LanternFunctionData equippedFunction = DBManager.I.itemDatabase.allLanterns[find];
            if (equippedFunctionImage != null)
            {
                equippedFunctionImage.sprite = equippedFunction.functionIcon;
                equippedFunctionImage.gameObject.SetActive(true);
            }
        }
        else
        {
            // 5. DB에는 있는데 에셋을 못 찾은 경우 (에러 방지용으로 끄기)
            if (equippedFunctionImage != null)
            {
                equippedFunctionImage.sprite = null;
                equippedFunctionImage.gameObject.SetActive(false);
            }
        }
    }

    // 3칸 슬롯의 좌/우/위 내비게이션을 설정합니다. (1번 요청)
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