using System.Collections;
using UnityEngine;
public class RandomGear : DropItem
{
    public Sprite[] sprites;
    [Space(40)]
    [SerializeField] RandomTable[] randomGears;
    [System.Serializable]
    public struct RandomTable
    {
        public GearData gear;
        public int weight;
    }
    public bool isGuaranteed;
    SpriteRenderer sr;
    protected override void OnEnable()
    {
        gold = 0;
        itemData = null;
        gearData = null;
        lanternData = null;
        recordData = null;
        base.OnEnable();
    }
    IEnumerator Start()
    {
        gold = 0;
        itemData = null;
        gearData = null;
        lanternData = null;
        recordData = null;
        if (randomGears.Length == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }
        bool success = false;
        // 기존 50회 루프 고수
        for (int i = 0; i < 50; i++)
        {
            int totalWeight = 0;
            foreach (var element in randomGears)
            {
                totalWeight += element.weight;
            }
            int randomInt = Random.Range(0, totalWeight);
            int partialWeight = 0;
            int findIndex = -1;
            for (int k = 0; k < randomGears.Length; k++)
            {
                partialWeight += randomGears[k].weight;
                if (partialWeight >= randomInt)
                {
                    findIndex = k;
                    break;
                }
            }
            if (findIndex == -1) continue;
            GearData candidate = randomGears[findIndex].gear;
            bool alreadyHas;
            DBManager.I.HasGear(candidate.name, out alreadyHas);
            if (alreadyHas)
            {
                // 이미 가지고 있을 때: 확정 드롭 모드면 다시 루프(continue), 아니면 종료
                if (isGuaranteed)
                {
                    continue; 
                }
                else
                {
                    gameObject.SetActive(false);
                    yield break; // 확정이 아니면 그냥 사라짐
                }
            }
            gearData = candidate;
            success = true;
            break; // 루프 탈출
        }
        if (!success || gearData == null)
        {
            gameObject.SetActive(false);
            yield break;
        }
        if (TryGetComponent(out sr))
        {
            switch (gearData.name)
            {
                case "001_LastStandGear": sr.sprite = sprites[0]; break;
                case "002_CounterGear": sr.sprite = sprites[1]; break;
                case "003_FatalBlowGear": sr.sprite = sprites[2]; break;
                case "004_GlitchGear": sr.sprite = sprites[3]; break;
                case "005_RestorationGear": sr.sprite = sprites[4]; break;
                case "006_SuperNovaGear": sr.sprite = sprites[5]; break;
                case "007_QuickHealGear": sr.sprite = sprites[3]; break;
                case "008_ExpansionGear": sr.sprite = sprites[6]; break;
                case "009_ParryGear": sr.sprite = sprites[3]; break;
            }
        }
    }
}