using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHit : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Hit;
    [HideInInspector] public int type;
    [HideInInspector] public MonsterControl.State prevState;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(cts.Token);
        
        float duration = 0.6f;
        if (type == 1)
        {
            if (control.isDie) return;
            anim.Play("Idle");
            duration = 0.6f;
        }
        else if(type == 2)
        {
            if (control.isDie) return;
            anim.Play("Idle");
            if (control.data.Type != MonsterType.Large && control.data.Type != MonsterType.Boss)
                duration = 6.8f;
            else
                duration = 2.4f;
        }
        //
        MonsterControl.State next = MonsterControl.State.Idle;
        float newCoolTime = 0f;
        if (prevState.ToString().Contains("Attack"))
        {
            float normalTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (normalTime < 0.22f)
            {
                if (Random.value < 0.5f)
                    next = prevState;
                else
                {
                    newCoolTime = Random.Range(0f, 0.2f) * control.stateDictionary[prevState].coolTime;
                    control.SetCoolTime(prevState, newCoolTime);
                }
            }
            else if (normalTime < 0.77f)
            {
                newCoolTime = Random.Range(0.6f, 1f) * control.stateDictionary[prevState].coolTime;
                control.SetCoolTime(prevState, newCoolTime);
            }
        }
        await UniTask.Delay((int)(1000 * duration), cancellationToken: token);
        if (next != MonsterControl.State.Idle)
        {
            control.ChangeState(next, true);
            return;
        }
        control.ChangeNextState();
    }






}
