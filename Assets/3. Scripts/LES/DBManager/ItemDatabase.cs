using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Project Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("게임 내 모든 데이터 등록")]
    public List<ItemData> allItems;
    public List<GearData> allGears;
    public List<LanternFunctionData> allLanterns;
    public List<RecordData> allRecords;

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