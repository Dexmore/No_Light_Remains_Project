using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class RecordPanelController : MonoBehaviour, ITabContent
{
    [Header("슬롯 관리")]
    [SerializeField] private GameObject recordSlotPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("상세 내용(Detail) UI")]
    [SerializeField] private TextMeshProUGUI detailTitleText;
    [SerializeField] private TextMeshProUGUI detailContentText;
    
    [Header("내비게이션")]
    [SerializeField] private Selectable mainTabButton; 

    private List<RecordSlotUI> _spawnedSlots = new List<RecordSlotUI>();

    // [추가] 이벤트 구독
    private void OnEnable()
    {
        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnRecordsChanged += RefreshPanel;
        }
    }

    private void OnDisable()
    {
        if (InventoryDataManager.Instance != null)
        {
            InventoryDataManager.Instance.OnRecordsChanged -= RefreshPanel;
        }
    }

    public void OnShow()
    {
        RefreshPanel(); // 패널 새로고침
        
        // 한 프레임 기다린 후 첫 슬롯 선택 (포커스 문제 방지)
        StartCoroutine(SelectFirstSlot());
    }

    public void OnHide()
    {
        ClearAllSpawnedSlots();
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    // 이벤트 방송을 받거나 탭이 열릴 때 호출됩니다.
    private void RefreshPanel()
    {
        if (InventoryDataManager.Instance == null) return;

        ClearAllSpawnedSlots();
        

        //////////
        List<RecordData> playerRecords = new List<RecordData>();
        for(int i=0; i<DBManager.I.currData.recordDatas.Count; i++)
        {
            CharacterData.RecordData cd = DBManager.I.currData.recordDatas[i];
            int find = DBManager.I.itemDatabase.allRecords.FindIndex(x => x.recordTitle == cd.Name);
            if(find == -1) continue;
            RecordData d = DBManager.I.itemDatabase.allRecords[find];
            playerRecords.Add(d);
        }
        //////////
        Debug.Log(playerRecords.Count);
        
        if (playerRecords.Count == 0)
        {
            ShowRecordDetails(null);
            SetupSlotNavigation(); // [추가] 0개일 때도 내비게이션 설정
            return;
        }

        foreach (RecordData record in playerRecords)
        {
            GameObject slotGO = Instantiate(recordSlotPrefab, contentTransform);
            RecordSlotUI slotUI = slotGO.GetComponent<RecordSlotUI>();
            slotUI.SetData(record, this);
            _spawnedSlots.Add(slotUI);
        }
        
        SetupSlotNavigation();
        ShowRecordDetails(playerRecords[0]);
    }
    
    private IEnumerator SelectFirstSlot()
    {
        yield return new WaitForEndOfFrame();
        if (_spawnedSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(_spawnedSlots[0].gameObject);
        }
        else
        {
            // [추가] 활성화된 슬롯이 하나도 없으면 탭 버튼으로 포커스 이동
            mainTabButton?.Select();
        }
    }

    // (단, SetupSlotNavigation을 0개일 때도 처리하도록 수정)
    #region (수정 없는 함수들)
    public void ShowRecordDetails(RecordData data)
    {
        if (data != null)
        {
            detailTitleText.text = data.recordTitle;
            detailContentText.text = data.recordContent;
        }
        else
        {
            detailTitleText.text = "기록물 없음";
            detailContentText.text = "아직 습득한 기록물이 없습니다.";
        }
    }

    private void ClearAllSpawnedSlots()
    {
        foreach (RecordSlotUI slot in _spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();
    }
    
    private void SetupSlotNavigation()
    {
        if (_spawnedSlots.Count == 0)
        {
            // [추가] 슬롯이 0개일 때, 탭 버튼의 '아래'를 막음 (갈 곳이 없으므로)
            // (이 로직은 RecordPanelController가 아닌, TabGroup이 관리해야 더 좋지만 일단 여기에 둡니다.)
            return;
        }

        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue;
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = (i == 0) ? mainTabButton : _spawnedSlots[i - 1].GetComponent<Button>();
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? _spawnedSlots[0].GetComponent<Button>() : _spawnedSlots[i + 1].GetComponent<Button>();
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            button.navigation = nav;
        }
    }
    #endregion

    #region 테스트용 코드 (NaughtyAttributes)

    [Header("테스트용")]
    [SerializeField] private RecordData testRecordToAdd;

    [Button("Test: 기록물 추가")]
    private void TestAddRecord()
    {
        if (testRecordToAdd == null)
        {
            Debug.LogWarning("테스트할 기록물(.asset)을 인스펙터 필드에 할당해주세요!");
            return;
        }
        
        // 중앙 관리자의 'AddItem' 마스터 메서드를 호출
        InventoryDataManager.Instance.AddItem(testRecordToAdd);
    }

    #endregion
}