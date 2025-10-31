using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // [추가] FirstOrDefault를 사용하기 위해
using System.Collections;
using TMPro;

public class RecordPanelController : MonoBehaviour, ITabContent
{
    [Header("슬롯 관리")]
    [SerializeField] private GameObject recordSlotPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("상세 내용(Detail) UI")]
    [SerializeField] private TextMeshProUGUI detailTitleText;
    [SerializeField] private TextMeshProUGUI detailContentText;
    
    // [추가] 탭으로 돌아가기 위한 버튼
    [Header("내비게이션")]
    [Tooltip("슬롯에서 위로 갔을 때 선택될 탭 버튼 (예: '기록물' 탭 버튼)")]
    [SerializeField] private Selectable mainTabButton; 

    // [가정] 습득한 기록물 목록 (테스트용)
    private List<RecordData> _acquiredRecords = new List<RecordData>();

    private List<RecordSlotUI> _spawnedSlots = new List<RecordSlotUI>();

    private void Awake()
    {
        CreateTestData();
    }

    public void OnShow()
    {
        UpdateRecordList();
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

    private void UpdateRecordList()
    {
        ClearAllSpawnedSlots();
        
        if (_acquiredRecords.Count == 0)
        {
            ShowRecordDetails(null);
            return;
        }

        foreach (RecordData record in _acquiredRecords)
        {
            GameObject slotGO = Instantiate(recordSlotPrefab, contentTransform);
            RecordSlotUI slotUI = slotGO.GetComponent<RecordSlotUI>();
            slotUI.SetData(record, this);
            _spawnedSlots.Add(slotUI);
        }
        
        // 내비게이션 설정
        SetupSlotNavigation();
        
        // 첫 번째 기록물 내용 표시
        ShowRecordDetails(_acquiredRecords[0]);
        
        // [수정] 한 프레임 기다린 후 첫 슬롯 선택 (포커스 문제 방지)
        StartCoroutine(SelectFirstSlot());
    }
    
    // [추가] 첫 슬롯 선택용 코루틴
    private IEnumerator SelectFirstSlot()
    {
        yield return new WaitForEndOfFrame(); // 레이아웃 계산 대기
        if (_spawnedSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(_spawnedSlots[0].gameObject);
        }
    }

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

    /// <summary>
    /// [수정된] 내비게이션 설정 함수
    /// </summary>
    private void SetupSlotNavigation()
    {
        if (_spawnedSlots.Count == 0) return;

        for (int i = 0; i < _spawnedSlots.Count; i++)
        {
            Button button = _spawnedSlots[i].GetComponent<Button>();
            if (button == null) continue;

            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // [수정] 위쪽: (첫 슬롯) -> '메인 탭 버튼' / (나머지) -> 바로 위 슬롯
            nav.selectOnUp = (i == 0) ? mainTabButton : _spawnedSlots[i - 1].GetComponent<Button>();
            
            // 아래쪽: (마지막 슬롯) -> 첫 슬롯 (루프) / (나머지) -> 바로 아래 슬롯
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? _spawnedSlots[0].GetComponent<Button>() : _spawnedSlots[i + 1].GetComponent<Button>();
            
            nav.selectOnLeft = null;
            nav.selectOnRight = null; // (필요 시 우측 상세 패널의 스크롤바 등으로 연결 가능)

            button.navigation = nav;
        }
    }

    // --- 테스트용 임시 데이터 ---
    private void CreateTestData()
    {
        _acquiredRecords.Add(new RecordData("첫 번째 기록", "이것은 게임의 첫 번째 기록물입니다. 내용은..."));
        _acquiredRecords.Add(new RecordData("랜턴에 대하여", "랜턴은 어둠을 밝히는 중요한 도구입니다."));
        _acquiredRecords.Add(new RecordData("괴물의 약점", "그들은 빛을 두려워하는 것 같습니다."));
    }
}