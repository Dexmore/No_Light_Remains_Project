using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RecordPanelController : MonoBehaviour, ITabContent
{
    [Header("슬롯 관리")]
    [SerializeField] private GameObject recordSlotPrefab; // 기록물 슬롯 프리팹
    [SerializeField] private Transform contentTransform;  // 스크롤 뷰의 Content 오브젝트

    [Header("상세 내용(Detail) UI")]
    [SerializeField] private TextMeshProUGUI detailTitleText; // 우측 제목 텍스트
    [SerializeField] private TextMeshProUGUI detailContentText; // 우측 내용 텍스트

    // [가정] 게임 매니저 등에서 받아 온, 플레이어가 '습득한' 기록물 목록
    // 실제로는 이 리스트를 외부에서 채워줘야 합니다.
    private List<RecordData> _acquiredRecords = new List<RecordData>();

    // 현재 생성된 슬롯 UI 리스트
    private List<RecordSlotUI> _spawnedSlots = new List<RecordSlotUI>();

    private void Awake()
    {
        // [테스트용] 임시 데이터 생성
        CreateTestData();
    }

    /// <summary>
    /// 탭이 열릴 때 TabGroup에 의해 호출됩니다.
    /// </summary>
    public void OnShow()
    {
        Debug.Log("기록물 탭이 열렸습니다.");
        UpdateRecordList(); // 목록을 새로고침합니다.
    }

    public void OnHide()
    {
        Debug.Log("기록물 탭이 닫혔습니다.");
        
        // 생성된 슬롯 오브젝트들 삭제
        ClearAllSpawnedSlots();

        // 선택 해제
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    /// <summary>
    /// 습득한 기록물 목록을 기준으로 슬롯을 동적 생성합니다.
    /// </summary>
    private void UpdateRecordList()
    {
        ClearAllSpawnedSlots();
        
        // TODO: 실제로는 GameManager로부터 _acquiredRecords 리스트를 받아와야 함
        
        if (_acquiredRecords.Count == 0)
        {
            // 습득한 기록이 없으면 상세 정보창도 비움
            ShowRecordDetails(null);
            return;
        }

        // 습득한 기록만큼 슬롯 생성
        foreach (RecordData record in _acquiredRecords)
        {
            GameObject slotGO = Instantiate(recordSlotPrefab, contentTransform);
            RecordSlotUI slotUI = slotGO.GetComponent<RecordSlotUI>();
            slotUI.SetData(record, this); // [중요] 'this' (컨트롤러 자신)를 넘겨줌
            _spawnedSlots.Add(slotUI);
        }
        
        // 내비게이션 설정
        SetupSlotNavigation();
        
        // 탭이 열리면 항상 첫 번째 기록물의 내용을 자동으로 보여줌
        ShowRecordDetails(_acquiredRecords[0]);
        
        // EventSystem도 첫 번째 슬롯을 자동으로 선택
        if (_spawnedSlots.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(_spawnedSlots[0].gameObject);
        }
    }

    /// <summary>
    /// [공개] RecordSlotUI가 호출할 함수. 기록물 상세 내용을 오른쪽에 표시합니다.
    /// </summary>
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
            Destroy(slot.gameObject);
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
            nav.mode = Navigation.Mode.Explicit; // 수동 설정

            // 위쪽: (첫 슬롯) -> 맨 아래 슬롯 (루프) / (나머지) -> 바로 위 슬롯
            nav.selectOnUp = (i == 0) ? _spawnedSlots[_spawnedSlots.Count - 1].GetComponent<Button>() : _spawnedSlots[i - 1].GetComponent<Button>();
            
            // 아래쪽: (마지막 슬롯) -> 첫 슬롯 (루프) / (나머지) -> 바로 아래 슬롯
            nav.selectOnDown = (i == _spawnedSlots.Count - 1) ? _spawnedSlots[0].GetComponent<Button>() : _spawnedSlots[i + 1].GetComponent<Button>();
            
            // 좌/우 이동은 막음 (필요 시 우측 상세 패널의 스크롤바 등으로 연결 가능)
            nav.selectOnLeft = null;
            nav.selectOnRight = null;

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