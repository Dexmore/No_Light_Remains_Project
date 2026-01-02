using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorkbenchUI : MonoBehaviour
{
    [Header("1. 패널 및 기본 설정")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    [Header("2. 슬롯 리스트 (Pre-placed Slots)")]
    [SerializeField] private Transform slotContent; 

    [Header("3. 중앙 정보 (Info_Zone)")]
    [SerializeField] private Image targetGearImage;
    
    // [수정 요청 2] "[ 선택한 기어 ]" 부분의 텍스트 연결
    [SerializeField] private TextMeshProUGUI selectedGearTitleText; 

    // StatusZone 텍스트들 (기존 유지)
    [SerializeField] private TextMeshProUGUI[] currentStatusTexts; 
    
    // ReinforcementSpecsZone 텍스트들 (기존 유지)
    [SerializeField] private TextMeshProUGUI[] nextStatusTexts;

    [Header("4. 비용 및 확률")]
    [SerializeField] private TextMeshProUGUI[] costTexts; 
    [SerializeField] private TextMeshProUGUI successRateText; 
    [SerializeField] private TextMeshProUGUI destroyRateText; 
    [SerializeField] private Button enhanceButton;

    // 내부 변수
    private GearData _selectedGearData;
    private string _selectedGearName;
    private List<WorkbenchSlotUI> _preplacedSlots = new List<WorkbenchSlotUI>();

    private void Start()
    {
        if(closeButton != null) closeButton.onClick.AddListener(Close);
        if(enhanceButton != null) enhanceButton.onClick.AddListener(OnClickEnhance);
        if(panelRoot != null) panelRoot.SetActive(false);

        // [최초 1회] 배치된 슬롯들을 미리 리스트에 담아둡니다.
        // slotContent 아래에 있는 모든 WorkbenchSlotUI 컴포넌트를 찾습니다.
        if (slotContent != null)
        {
            // GetComponentsInChildren은 부모 포함일 수 있으므로 transform 루프 사용 권장
            foreach (Transform child in slotContent)
            {
                var slotScript = child.GetComponent<WorkbenchSlotUI>();
                if (slotScript != null)
                {
                    _preplacedSlots.Add(slotScript);
                    child.gameObject.SetActive(false); // 일단 다 꺼둡니다.
                }
            }
        }
    }

    public void Open()
    {
        if(panelRoot != null) panelRoot.SetActive(true);
        RefreshSlotList(); 
        ClearSelection();  
    }

    public void Close()
    {
        if(panelRoot != null) panelRoot.SetActive(false);
    }

    // 1. 슬롯 목록 갱신 (수정됨: 배치된 슬롯 재사용)
    private void RefreshSlotList()
    {
        if (DBManager.I == null || DBManager.I.currData.gearDatas == null) return;

        var userGears = DBManager.I.currData.gearDatas;
        
        // 슬롯 개수와 아이템 개수 중 더 작은 쪽만큼 반복
        int count = Mathf.Min(userGears.Count, _preplacedSlots.Count);

        // 1. 아이템이 있는 만큼 슬롯 활성화 및 세팅
        for (int i = 0; i < count; i++)
        {
            var userGear = userGears[i];
            GearData gearData = DBManager.I.itemDatabase.FindGearByName(userGear.Name);

            if (gearData != null)
            {
                _preplacedSlots[i].gameObject.SetActive(true);
                _preplacedSlots[i].Setup(gearData, userGear.Name, userGear.isEquipped, this);
            }
            else
            {
                // 데이터 못 찾으면 슬롯 끄기
                _preplacedSlots[i].gameObject.SetActive(false);
            }
        }

        // 2. 남은 슬롯들은 비활성화 (아이템이 3개면 4~11번 슬롯 끄기)
        for (int i = count; i < _preplacedSlots.Count; i++)
        {
            _preplacedSlots[i].gameObject.SetActive(false);
        }
    }

    public void SelectGear(string gearName, GearData data)
    {
        _selectedGearName = gearName;
        _selectedGearData = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_selectedGearData == null) return;

        // [수정 요청 2] 타이틀 텍스트 포맷 변경: "[ 이름 ]"
        if (selectedGearTitleText != null)
        {
            selectedGearTitleText.text = $"[ {_selectedGearData.localizedName} ]";
        }

        // 이미지 표시
        if (targetGearImage != null)
        {
            targetGearImage.sprite = _selectedGearData.gearIcon;
            targetGearImage.gameObject.SetActive(true);
        }

        // (기존 스탯 표시 로직 유지...)
        int currentLevel = 0; // TODO: DB연결 시 수정

        // StatusZone (이름, 레벨 등) - 필요 시 여기서 이름 표시는 제거하고 타이틀만 쓸 수도 있음
        if(currentStatusTexts.Length > 0) currentStatusTexts[0].text = _selectedGearData.localizedName;
        if(currentStatusTexts.Length > 1) currentStatusTexts[1].text = "Lv." + currentLevel;

        // 매니저 정보 가져오기
        if (EnhancementManager.I.GetLevelInfo(currentLevel, out var info))
        {
            if(successRateText != null) successRateText.text = $"{info.successRate}%";
            if(destroyRateText != null) destroyRateText.text = $"{info.destroyRate}%";

            if(costTexts.Length > 0) costTexts[0].text = $"{info.goldCost} G";
            
            if(costTexts.Length > 1)
            {
                if (info.requiredMaterials != null && info.requiredMaterials.Count > 0)
                {
                    var mat = info.requiredMaterials[0];
                    bool hasItem = DBManager.I.HasItem(mat.item.name, out int hasCount);
                    costTexts[1].text = $"{mat.item.localizedName} : {hasCount} / {mat.count}";
                    costTexts[1].color = (hasCount >= mat.count) ? Color.green : Color.red;
                }
                else
                {
                    costTexts[1].text = "재료 없음";
                    costTexts[1].color = Color.white;
                }
            }
            if(enhanceButton != null) enhanceButton.interactable = true;
        }
        else
        {
            if(successRateText != null) successRateText.text = "MAX";
            if(enhanceButton != null) enhanceButton.interactable = false;
        }
    }

    private void ClearSelection()
    {
        _selectedGearData = null;
        if(targetGearImage != null) targetGearImage.gameObject.SetActive(false);

        // [수정 요청 2] 초기화 시 텍스트 (원하는 대로 수정 가능)
        if (selectedGearTitleText != null) selectedGearTitleText.text = "[ 선택된 기어 없음 ]";
        
        foreach (var t in currentStatusTexts) t.text = "-";
        foreach (var t in nextStatusTexts) t.text = "-";
        foreach (var t in costTexts) t.text = "-";
        
        if(successRateText != null) successRateText.text = "";
        if(destroyRateText != null) destroyRateText.text = "";
        if(enhanceButton != null) enhanceButton.interactable = false;
    }

    private void OnClickEnhance()
    {
        if (_selectedGearData == null) return;
        var result = EnhancementManager.I.TryEnhance(_selectedGearName, _selectedGearData);

        switch (result)
        {
            case EnhancementManager.EnhancementResult.Success:
                FindObjectOfType<NotificationUI>()?.ShowMessage("강화 성공!");
                UpdateUI(); 
                break;
            case EnhancementManager.EnhancementResult.Fail:
                FindObjectOfType<NotificationUI>()?.ShowMessage("강화 실패...");
                UpdateUI(); 
                break;
            case EnhancementManager.EnhancementResult.Destroy:
                FindObjectOfType<NotificationUI>()?.ShowMessage("장비가 파괴되었습니다!");
                ClearSelection(); 
                RefreshSlotList(); // 슬롯 상태 갱신 (파괴된 템 슬롯 끄기)
                break;
            case EnhancementManager.EnhancementResult.Error:
                FindObjectOfType<NotificationUI>()?.ShowMessage("비용 부족 / 오류");
                break;
        }
    }
}