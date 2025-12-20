using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterIdle : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Idle;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        duration = Random.Range(durationRange.x, durationRange.y);


        await UniTask.Yield(token);
        float homeRadius = control.findRadius * 1.1f;
        float homeDistance = Vector2.Distance(control.startPosition, transform.position);
        float ratio = homeDistance / homeRadius;
        ratio = Mathf.Clamp(ratio - 0.1f, 0f, 1f);
        float returnChance = Mathf.Pow(ratio, 3);
        //Debug.Log($"homeDistance:{homeDistance} , W1 returnChanve:{returnChance}");
        if (Random.value < control.homeValue)
            if (Random.value < returnChance)
            {
                await UniTask.Delay(5, cancellationToken: token);
                control.ChangeState(MonsterControl.State.ReturnHome, true);
                return;
            }

        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.isDie) return;
        if(anim)
        {
            anim.Play("Idle");
        }
        await UniTask.Delay((int)(duration * 1000f), cancellationToken: token);
        control.ChangeNextState();
    }






}
