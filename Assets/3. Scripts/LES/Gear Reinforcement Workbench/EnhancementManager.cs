using UnityEngine;
using System.Collections.Generic;

// [통합됨] 별도의 Settings 파일 없이 매니저가 직접 데이터를 관리합니다.
public class EnhancementManager : SingletonBehaviour<EnhancementManager>
{
    protected override bool IsDontDestroy() => false; // 씬 이동 시 유지 필요 여부에 따라 변경

    // ==================================================================================
    // [내부 구조체 정의] 기존 ScriptableObject의 내용을 여기로 가져왔습니다.
    // ==================================================================================
    [System.Serializable]
    public struct MaterialRequirement
    {
        public ItemData item; // 필요한 재료 아이템 (ItemData SO 연결)
        public int count;     // 필요 개수
    }

    [System.Serializable]
    public struct LevelInfo
    {
        [Header("단계 식별 (예: 0 -> 1)")]
        public string label; 

        [Header("확률 설정 (0~100%)")]
        [Range(0, 100)] public float successRate;  // 성공 확률
        [Range(0, 100)] public float destroyRate;  // 실패 시 파괴 확률
        
        [Header("비용 설정")]
        public int goldCost; // 필요 골드
        public List<MaterialRequirement> requiredMaterials; // 필요 재료 리스트
    }

    // ==================================================================================
    // [매니저 설정] 인스펙터에서 이 리스트를 채우시면 됩니다.
    // ==================================================================================
    [Space(20)]
    [Header("강화 단계별 데이터 테이블")]
    [Tooltip("Index 0 = 0강에서 1강 갈 때의 정보")]
    [SerializeField] private List<LevelInfo> enhancementLevels;

    // 강화 결과 상태 열거형
    public enum EnhancementResult { Success, Fail, Destroy, Error }

    // ==================================================================================
    // [핵심 로직]
    // ==================================================================================

    /// <summary>
    /// 강화 시도 함수
    /// </summary>
    public EnhancementResult TryEnhance(string targetGearName, GearData targetGearData)
    {
        // 1. 현재 장비의 레벨 확인 (DB 연결 전 임시 처리)
        // int currentLevel = DBManager.I.GetGearLevel(targetGearName); 
        int currentLevel = 0; // [테스트용] 무조건 0강으로 가정

        // 2. 최대 레벨 도달 여부 확인
        if (currentLevel >= enhancementLevels.Count)
        {
            Debug.LogWarning("이미 최대 레벨입니다.");
            return EnhancementResult.Error;
        }

        // 3. 해당 단계의 강화 정보 가져오기
        LevelInfo currentInfo = enhancementLevels[currentLevel];

        // 4. 비용 검사 (골드 & 재료)
        if (!CheckCost(currentInfo))
        {
            return EnhancementResult.Error; // 재료 부족 알림은 UI에서 처리 권장
        }

        // 5. 비용 소모 (실제 차감)
        ConsumeCost(currentInfo);

        // 6. 확률 계산 (RNG)
        float randomVal = Random.Range(0f, 100f);
        
        // --- [성공] ---
        if (randomVal <= currentInfo.successRate)
        {
            OnEnhanceSuccess(targetGearName, currentLevel);
            return EnhancementResult.Success;
        }
        // --- [실패 or 파괴] ---
        else
        {
            // 실패 그룹 내에서 다시 파괴 확률 계산
            float destroyRandom = Random.Range(0f, 100f);
            
            if (destroyRandom <= currentInfo.destroyRate)
            {
                OnEnhanceDestroy(targetGearName);
                return EnhancementResult.Destroy;
            }
            else
            {
                OnEnhanceFail(targetGearName);
                return EnhancementResult.Fail;
            }
        }
    }

    // ----- 내부 헬퍼 함수들 -----

    private bool CheckCost(LevelInfo info)
    {
        // 1. 골드 확인
        if (DBManager.I.currData.gold < info.goldCost)
        {
            Debug.Log($"골드 부족: 보유 {DBManager.I.currData.gold} / 필요 {info.goldCost}");
            return false;
        }

        // 2. 재료 확인
        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;
                
                // DBManager의 HasItem 활용 (보유량 체크)
                if (!DBManager.I.HasItem(mat.item.name, out int currentCount) || currentCount < mat.count)
                {
                    Debug.Log($"재료 부족: {mat.item.name} (보유:{currentCount} / 필요:{mat.count})");
                    return false;
                }
            }
        }
        return true;
    }

    private void ConsumeCost(LevelInfo info)
    {
        // 1. 골드 차감
        DBManager.I.currData.gold -= info.goldCost;

        // 2. 재료 차감
        if (info.requiredMaterials != null)
        {
            foreach (var mat in info.requiredMaterials)
            {
                if (mat.item == null) continue;
                // AddItem에 음수를 넣어 차감
                DBManager.I.AddItem(mat.item.name, -mat.count);
            }
        }
    }

    private void OnEnhanceSuccess(string name, int currentLevel)
    {
        Debug.Log($"<color=green>[강화 성공]</color> {name}: {currentLevel} -> {currentLevel + 1}");
        // TODO: DBManager 데이터 업데이트 로직 (팀원 협의 후 주석 해제)
        /*
        var gearList = DBManager.I.currData.gearDatas;
        int idx = gearList.FindIndex(x => x.Name == name);
        if (idx != -1) {
            var data = gearList[idx];
            data.level++;
            gearList[idx] = data;
        }
        */
    }

    private void OnEnhanceFail(string name)
    {
        Debug.Log($"<color=orange>[강화 실패]</color> 등급 유지: {name}");
    }

    private void OnEnhanceDestroy(string name)
    {
        Debug.Log($"<color=red>[장비 파괴]</color> {name} 삭제됨");
        // TODO: DBManager 아이템 삭제 로직 (팀원 협의 후 주석 해제)
        /*
        int idx = DBManager.I.currData.gearDatas.FindIndex(x => x.Name == name);
        if (idx != -1) {
            DBManager.I.currData.gearDatas.RemoveAt(idx);
        }
        */
    }

    /// <summary>
    /// UI에서 보여줄 '다음 레벨 정보'를 반환하는 함수
    /// </summary>
    public bool GetLevelInfo(int currentLevel, out LevelInfo info)
    {
        if (currentLevel >= 0 && currentLevel < enhancementLevels.Count)
        {
            info = enhancementLevels[currentLevel];
            return true;
        }
        info = default;
        return false;
    }
}