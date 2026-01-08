using UnityEngine;
using System.Collections.Generic;

public class EnhancementManager : SingletonBehaviour<EnhancementManager>
{
    protected override bool IsDontDestroy() => false;

    // [수정] NotEnoughResources 추가
    public enum EnhancementResult { Success, MaxLevel, Error, NotEnoughResources }

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
            // [수정] Error 대신 NotEnoughResources 반환 (튜토리얼 로직과 일치시킴)
            return EnhancementResult.NotEnoughResources;
        }

        // 비용 소모 및 성공
        ConsumeCost(info);
        DBManager.I.LevelUpGear(targetGearName);
        AudioManager.I?.PlaySFX("UpgradeSuccess"); 
        Debug.Log($"[Enhancement] {targetGearName} 강화 성공!");
        
        return EnhancementResult.Success;
    }

    // (아래 CheckCost, ConsumeCost 함수는 기존과 동일하게 유지)
    private bool CheckCost(LevelInfo info)
    {
        // ... (기존 로직 유지) ...
        if (DBManager.I.currData.gold < info.goldCost) return false;

        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;
                string targetName = mat.item.name;
                int totalCount = 0;
                if (DBManager.I.currData.itemDatas != null)
                {
                    foreach (var dbItem in DBManager.I.currData.itemDatas)
                        if (dbItem.Name == targetName) totalCount += dbItem.count;
                }
                if (totalCount < mat.count) return false;
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
                DBManager.I.AddItem(mat.item.name, -mat.count);
            }
        }
    }
}