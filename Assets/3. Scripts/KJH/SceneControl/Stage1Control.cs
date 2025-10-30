using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Stage101Control : MonoBehaviour
{
    [ReadOnlyInspector] [SerializeField] int remainMonsterCount;
    List<MonsterControl> remainMonsters = new List<MonsterControl>();
    public InteractablePortal[] nextPortal;
    void Start()
    {
        remainMonsters = FindObjectsByType<MonsterControl>(sortMode: FindObjectsSortMode.InstanceID).ToList();
    }
    IEnumerator CheckCount()
    {
        while(true)
        {
            yield return YieldInstructionCache.WaitForSeconds(1f);
            remainMonsters.Remove(null);
            remainMonsterCount = remainMonsters.Count;
            if(remainMonsterCount == 0)
            {
                
            }
        }
    }
    

}
