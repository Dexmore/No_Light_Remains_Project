using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings; // 추가
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Project Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("게임 내 모든 데이터 등록")]
    public List<ItemData> allItems;
    public List<GearData> allGears;
    public List<LanternFunctionData> allLanterns;
    public List<RecordData> allRecords;

    // 게임 시작 시 혹은 매니저에서 이 함수를 한 번 호출해줘야 합니다.
    public void Initialize()
    {
        if (LocalizationSettings.InitializationOperation.IsDone)
        {
        // 이미 초기화가 끝난 상태라면 바로 실행
        OnLocalizationReady();
        }
        else
        {
            // 아직 초기화 중이라면 완료될 때까지 기다렸다가 실행
            LocalizationSettings.InitializationOperation.Completed += (op) =>
            {
                OnLocalizationReady();
            };
        }
    }

    private void OnLocalizationReady()
    {
        // 중복 등록 방지를 위해 한 번 해제 후 등록
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    
        RefreshAllData();
    }

    private void OnLocaleChanged(Locale locale)
    {
        Debug.Log($"언어 변경 감지: {locale.Identifier.Code}. 데이터를 갱신합니다.");
        RefreshAllData();
    }

    public void RefreshAllData()
    {
        // 모든 리스트의 데이터를 순회하며 LoadStrings() 호출
        // (각 데이터 클래스에 LoadStrings() 기능이 구현되어 있어야 합니다)
        allItems.ForEach(item => item.LoadStrings());
        allGears.ForEach(gear => gear.LoadStrings());
        allLanterns.ForEach(lantern => lantern.LoadStrings());
        allRecords.ForEach(record => record.LoadStrings());
    }

    // [수정] 정확성을 위해 에셋 파일 이름(.name)만으로 검색합니다.
    public ItemData FindItemByName(string name)
    {
        // 1. 에셋 이름과 정확히 일치하는지 확인
        return allItems.FirstOrDefault(item => item.name == name);
    }

    public GearData FindGearByName(string name)
    {
        return allGears.FirstOrDefault(gear => gear.name == name);
    }

    public LanternFunctionData FindLanternByName(string name)
    {
        return allLanterns.FirstOrDefault(lantern => lantern.name == name);
    }
    
    public RecordData FindRecordByName(string name)
    {
        return allRecords.FirstOrDefault(record => record.name == name);
    }
}