using UnityEngine;
using System.Collections.Generic;

public class EnhancementManager : SingletonBehaviour<EnhancementManager>
{
    protected override bool IsDontDestroy() => false;

    [System.Serializable]
    public struct MaterialRequirement
    {
        public ItemData item;
        public int count;
    }

    [System.Serializable]
    public struct LevelInfo
    {
        [Header("비용 설정 (100% 성공)")]
        public int goldCost; 
        public List<MaterialRequirement> requiredMaterials; 
    }

    // [수정] 'Common Enhancement Info' 삭제됨

    public enum EnhancementResult { Success, MaxLevel, Error }

    public EnhancementResult TryEnhance(string targetGearName, GearData targetGearData)
    {
        int currentLevel = DBManager.I.GetGearLevel(targetGearName);

        if (currentLevel >= 1) return EnhancementResult.MaxLevel;

        LevelInfo info;

        // [수정] 오직 기어별 전용 설정(Specific)만 확인합니다.
        if (targetGearData.specificEnhancementSettings != null && targetGearData.specificEnhancementSettings.Length > 0)
        {
            info = targetGearData.specificEnhancementSettings[0];
        }
        else
        {
            Debug.LogError($"[EnhancementManager] '{targetGearName}'의 강화 비용 설정(Specific Enhancement Settings)이 비어있습니다!");
            return EnhancementResult.Error;
        }

        if (!CheckCost(info)) return EnhancementResult.Error;

        ConsumeCost(info);

        DBManager.I.LevelUpGear(targetGearName);
        OnEnhanceSuccess(targetGearName);
        
        return EnhancementResult.Success;
    }

    private bool CheckCost(LevelInfo info)
    {
        if (DBManager.I.currData.gold < info.goldCost) return false;

        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;
                if (!DBManager.I.HasItem(mat.item.name, out int currentCount) || currentCount < mat.count)
                    return false;
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

    private void OnEnhanceSuccess(string name)
    {
        Debug.Log($"<color=green>[System] {name} 강화 완료</color>");
    }
}