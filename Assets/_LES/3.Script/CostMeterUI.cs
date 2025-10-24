using UnityEngine;
using System.Collections.Generic;

// [추가] 인스펙터에서 'On/Off' 오브젝트 쌍을 관리할 클래스
[System.Serializable]
public class CostPip
{
    [Tooltip("Pip의 최상위 부모 오브젝트 (최대치에 따라 켜고 끌 때 사용)")]
    public GameObject pipParent;
    
    [Tooltip("활성화(On) 상태일 때 켤 오브젝트 (예: 0_On)")]
    public GameObject onStateObject;
    
    [Tooltip("비활성화(Off) 상태일 때 켤 오브젝트 (예: 0_Off)")]
    public GameObject offStateObject;
}

public class CostMeterUI : MonoBehaviour
{
    // [수정] List<GameObject> 대신 새로운 List<CostPip>을 사용
    [SerializeField]
    private List<CostPip> costPips;

    /// <summary>
    /// 표시할 최대 코스트 개수를 설정합니다. (배경 0_Off를 켭니다)
    /// </summary>
    /// <param name="max">표시할 최대 코스트 (예: 6)</param>
    public void SetMaxCost(int max)
    {
        for (int i = 0; i < costPips.Count; i++)
        {
            if (costPips[i].pipParent != null)
            {
                // i가 max(6)보다 작으면 Pip 자체를 켭니다 (배경이 보임).
                costPips[i].pipParent.SetActive(i < max);
            }
        }
    }

    /// <summary>
    /// 현재 활성화된 코스트를 설정합니다. (0_On을 켭니다)
    /// </summary>
    /// <param name="amount">활성화할 코스트 (예: 3)</param>
    public void SetCost(int amount)
    {
        for (int i = 0; i < costPips.Count; i++)
        {
            // Pip이 꺼져있으면(최대치 밖) 무시
            if (!costPips[i].pipParent.activeSelf) continue;

            if (i < amount)
            {
                // 0_On 켜기, 0_Off 끄기
                costPips[i].onStateObject.SetActive(true);
                costPips[i].offStateObject.SetActive(false);
            }
            else
            {
                // 0_On 끄기, 0_Off 켜기
                costPips[i].onStateObject.SetActive(false);
                costPips[i].offStateObject.SetActive(true);
            }
        }
    }

    // (선택사항) 인스펙터에 연결된 Pip의 총 개수를 반환
    public int GetTotalPipCount()
    {
        return costPips.Count;
    }
}