using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 검색 기능을 위해 필요

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Project Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("게임 내 모든 데이터 등록")]
    public List<ItemData> allItems;
    public List<GearData> allGears;
    public List<LanternFunctionData> allLanterns;
    public List<RecordData> allRecords; // (기록물도 추가)

    // 이름으로 ItemData 원본을 찾습니다.
    public ItemData FindItemByName(string name)
    {
        return allItems.FirstOrDefault(item => item.name == name || item.itemName == name);
    }

    public GearData FindGearByName(string name)
    {
        return allGears.FirstOrDefault(gear => gear.name == name || gear.gearName == name);
    }

    public LanternFunctionData FindLanternByName(string name)
    {
        return allLanterns.FirstOrDefault(lantern => lantern.name == name || lantern.functionName == name);
    }
    
    public RecordData FindRecordByName(string name)
    {
        return allRecords.FirstOrDefault(record => record.name == name || record.recordTitle == name);
    }
}