using System.Collections;
using UnityEngine;
public class DropRandomGear : DropItem
{
    [Space(40)]
    [SerializeField] RandomGear[] randomGears;
    [System.Serializable]
    public struct RandomGear
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
        }
    }







}
