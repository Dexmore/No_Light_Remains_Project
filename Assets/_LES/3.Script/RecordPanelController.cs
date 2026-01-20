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
    [SerializeField] private Image detailContentImage;
    [SerializeField] private TextMeshProUGUI detailContentText;

    [Header("내비게이션")]
    [SerializeField] private Selectable mainTabButton;

    private List<RecordSlotUI> _spawnedSlots = new List<RecordSlotUI>();

    // [수정] 이벤트 구독 제거 (DBManager는 UI 갱신 이벤트를 따로 안 보내므로 OnShow에서 처리)
    private void OnEnable() { }
    private void OnDisable() { }

    public void OnShow()
    {
        RefreshPanel(); // 열릴 때마다 데이터 새로고침
        StartCoroutine(SelectFirstSlot());
    }

    public void OnHide()
    {
        ClearAllSpawnedSlots();
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void RefreshPanel()
    {
        // 1. 기존 슬롯 싹 지우기
        ClearAllSpawnedSlots();

        if (DBManager.I == null) return;

        // 2. DBManager에 저장된 기록물 데이터 리스트 가져오기
        var savedRecords = DBManager.I.currData.recordDatas;

        if (savedRecords == null || savedRecords.Count == 0)
        {
            ShowRecordDetails(null);
            SetupSlotNavigation(); 
            return;
        }

        // [핵심 수정] -------------------------------------------------------
        // 원본 DB 리스트를 건드리지 않기 위해 '복사본 리스트'를 만듭니다.
        List<CharacterData.RecordData> displayList = new List<CharacterData.RecordData>(savedRecords);
        
        // 리스트를 뒤집습니다. (가장 마지막에 추가된 데이터가 0번 인덱스로 옴 -> 최신순)
        displayList.Reverse();
        // -------------------------------------------------------------------

        // 3. 뒤집힌 리스트(displayList)를 하나씩 순회하며 슬롯 생성
        bool hasAnyRecord = false;

        foreach (var savedData in displayList)
        {
            // 이름으로 원본 에셋 찾기 (ItemDatabase에서 검색)
            int findIndex = DBManager.I.itemDatabase.allRecords.FindIndex(x => x.name == savedData.Name);
            
            if (findIndex != -1)
            {
                // 원본 에셋 찾음
                RecordData originalAsset = DBManager.I.itemDatabase.allRecords[findIndex];
                
                // 런타임 복제본 생성
                RecordData runtimeRecord = Instantiate(originalAsset);
                runtimeRecord.name = originalAsset.name; 
                runtimeRecord.isNew = savedData.isNew;   
                
                // 번역 로드
                runtimeRecord.LoadStrings(); 

                // 슬롯 프리팹 생성 및 데이터 주입
                GameObject slotGO = Instantiate(recordSlotPrefab, contentTransform);
                RecordSlotUI slotUI = slotGO.GetComponent<RecordSlotUI>();
                slotUI.SetData(runtimeRecord, this);
                
                _spawnedSlots.Add(slotUI);
                hasAnyRecord = true;
            }
            else
            {
                Debug.LogWarning($"[RecordPanel] DB에는 '{savedData.Name}'가 있는데 ItemDatabase에서 못 찾았습니다.");
            }
        }

        SetupSlotNavigation();

        // 4. 첫 번째 아이템 정보 표시 (최신 아이템이 선택됨)
        if (hasAnyRecord && _spawnedSlots.Count > 0)
        {
             // 첫 번째 슬롯(가장 최신 기록물)의 정보를 갱신하기 위해 강제 선택 처리
             // (Coroutine에서 SelectFirstSlot이 호출되므로 여기선 놔둬도 무방하나 로직 흐름상 유지)
        }
        else
        {
            ShowRecordDetails(null);
        }
    }

    private IEnumerator SelectFirstSlot()
    {
        yield return new WaitForEndOfFrame();
        if (_spawnedSlots.Count > 0)
        {
            // 첫 번째 슬롯을 선택하면 OnSelect가 호출되면서 상세 정보도 갱신됨
            EventSystem.current.SetSelectedGameObject(_spawnedSlots[0].gameObject);
            _spawnedSlots[0].OnSelect(null); // 강제 호출로 정보 갱신 보장
        }
        else
        {
            mainTabButton?.Select();
        }
    }

    // --- (이하 코드는 기존과 동일) ---
    public void ShowRecordDetails(RecordData data)
    {
        if (data != null)
        {
            // 아직 번역 로딩이 안 끝났을 수도 있으므로 체크
            if (string.IsNullOrEmpty(data.localizedName))
            {
                detailTitleText.text = "로딩 중...";
                detailContentText.text = "데이터를 불러오는 중입니다.";
                detailContentImage.gameObject.SetActive(false);
            }
            else
            {
                detailTitleText.text = data.localizedName;
                detailContentText.text = data.localizedDescription;
                if(data.sprite == null)
                {
                    detailContentImage.gameObject.SetActive(false);
                }
                else
                {
                    detailContentImage.gameObject.SetActive(true);
                    detailContentImage.sprite = data.sprite;
                    detailContentImage.preserveAspect = true;
                }
            }
        }
        else
        {
            if(SettingManager.I.setting.locale == 0) //영어
            {
                detailContentText.text = "No Record";
                detailContentText.text = "No Record Text";
            }
            else if(SettingManager.I.setting.locale == 1) //한글
            {
                detailTitleText.text = "기록물 없음";
                detailContentText.text = "아직 습득한 기록물이 없습니다.";
            }
            //else if(SettingManager.I.setting.locale == 2)//중국어

            detailContentImage.gameObject.SetActive(false);                
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
        if (_spawnedSlots.Count == 0) return;

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
}