using UnityEngine;
using System.Collections.Generic;

public class EnhancementManager : SingletonBehaviour<EnhancementManager>
{
    protected override bool IsDontDestroy() => false;

    public enum EnhancementResult { Success, MaxLevel, Error }

    [System.Serializable]
    public struct MaterialRequirement
    {
        public ItemData item;
        public int count;
    }

    [System.Serializable]
    public struct LevelInfo
    {
        [Header("강화 비용")]
        public int goldCost; 
        public List<MaterialRequirement> requiredMaterials; 
    }

    public EnhancementResult TryEnhance(string targetGearName, GearData targetGearData)
    {
        int currentLevel = DBManager.I.GetGearLevel(targetGearName);

        if (currentLevel >= 1) 
        {
            Debug.Log($"[Enhancement] 강화 실패: 이미 최대 레벨입니다.");
            return EnhancementResult.MaxLevel;
        }

        if (targetGearData.specificEnhancementSettings == null || targetGearData.specificEnhancementSettings.Length == 0)
        {
            Debug.LogError($"[Enhancement] '{targetGearName}'의 비용 설정이 없습니다.");
            return EnhancementResult.Error;
        }

        LevelInfo info = targetGearData.specificEnhancementSettings[0];

        // 비용 검사
        if (!CheckCost(info)) 
        {
            Debug.Log("[Enhancement] 강화 실패: 비용 부족 (Manager 검사)");
            return EnhancementResult.Error;
        }

        // 비용 소모 및 성공
        ConsumeCost(info);
        DBManager.I.LevelUpGear(targetGearName);
        AudioManager.I?.PlaySFX("UpgradeSuccess"); 
        Debug.Log($"[Enhancement] {targetGearName} 강화 성공!");
        
        return EnhancementResult.Success;
    }

    // [수정] UI와 똑같이 재료를 '합산'해서 검사하는 함수
    private bool CheckCost(LevelInfo info)
    {
        // 1. 골드 체크
        if (DBManager.I.currData.gold < info.goldCost) 
        {
            Debug.Log($"[Manager] 골드 부족. (보유: {DBManager.I.currData.gold})");
            return false;
        }

        // 2. 재료 체크 (리스트 전체 합산)
        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;

                string targetName = mat.item.name;
                int totalCount = 0;

                // DB를 직접 순회하며 중복된 아이템 개수까지 모두 더함
                if (DBManager.I.currData.itemDatas != null)
                {
                    foreach (var dbItem in DBManager.I.currData.itemDatas)
                    {
                        if (dbItem.Name == targetName)
                        {
                            totalCount += dbItem.count;
                        }
                    }
                }

                Debug.Log($"[Manager 검사] {targetName} | 합산된 보유량: {totalCount} | 필요: {mat.count}");

                if (totalCount < mat.count)
                {
                    Debug.Log($"[Manager] 재료 부족: {targetName}");
                    return false;
                }
            }
        }
        return true;
    }

    private void ConsumeCost(LevelInfo info)
    {
        DBManager.I.currData.gold -= info.goldCost;
        
        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;
                
                // 차감 시에는 기존 AddItem 함수 사용 (-개수)
                // (만약 DBManager가 중복 처리를 못한다면 여기서도 직접 빼줘야 하지만, 일단 실행해봅시다)
                DBManager.I.AddItem(mat.item.name, -mat.count);
            }
        }
    }
}