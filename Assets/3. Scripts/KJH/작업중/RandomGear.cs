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
    protected override void OnEnable()
    {
        gold = 0;
        itemData = null;
        gearData = null;
        lanternData = null;
        recordData = null;
        base.OnEnable();
    }
    SpriteRenderer sr;
    
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
        if (gearData == null)
        {
            int totalWeight = 0;
            foreach (var element in randomGears)
            {
                totalWeight += element.weight;
            }
            int randomInt = Random.Range(0, totalWeight);
            int partialWeight = 0;
            int find = -1;
            for (int k = 0; k < randomGears.Length; k++)
            {
                partialWeight += randomGears[k].weight;
                if (partialWeight >= randomInt)
                {
                    find = k;
                    break;
                }
            }
            if (find == -1)
            {
                gameObject.SetActive(false);
                yield break;
            }
            gearData = randomGears[find].gear;
            bool outValue;
            if (DBManager.I.HasGear(gearData.name, out outValue))
            {
                gameObject.SetActive(false);
                yield break;
            }
            if(TryGetComponent(out sr))
            {
                switch(gearData.name)
                {
                    case "001_LastStandGear":
                    sr.sprite = sprites[0];
                    break;
                    case "002_CounterGear":
                    sr.sprite = sprites[1];
                    break;
                    case "003_FatalBlowGear":
                    sr.sprite = sprites[2];
                    break;
                    case "004_GlitchGear":
                    sr.sprite = sprites[3];
                    break;
                    case "005_RestorationGear":
                    sr.sprite = sprites[4];
                    break;
                    case "006_SuperNovaGear":
                    sr.sprite = sprites[5];
                    break;
                    case "007_QuickHealGear":
                    sr.sprite = sprites[3];
                    break;
                    case "008_ExpansionGear":
                    sr.sprite = sprites[6];
                    break;
                    case "009_ParryGear":
                    sr.sprite = sprites[3];
                    break;
                }
            }
        }
    }







}
