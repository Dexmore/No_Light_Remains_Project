using System.Threading;
using System.Linq;
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
        Transform target = control.memories.First().Key.transform;
        Vector3 direction = transform.position - target.position;
        direction.y = 0;
        direction.Normalize();
        if (direction.x < 0 && model.right.x < 0)
            model.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction.x > 0 && model.right.x > 0)
            model.localRotation = Quaternion.Euler(0f, 180f, 0f);

        float duration = 0.53f;
        if (type == 1)
        {
            if (control.isDie) return;
            anim.Play("HitSmall");
            duration = 0.53f;
            if (Random.value <= 0.5f)
            {
                rb.AddForce(3f * Random.Range(0.9f, 1.1f) * (direction + 0.2f * Vector3.up).normalized, ForceMode2D.Impulse);
            }
        }
        else if (type == 2)
        {
            if (control.isDie) return;
            anim.Play("HitLarge");
            if (control.data.Type != MonsterType.Large && control.data.Type != MonsterType.Boss)
                duration = 4.8f;
            else
                duration = 6.8f;

            await UniTask.Delay((int)(1000 * 0.15f), cancellationToken: token);
            rb.AddForce(6.6f * Random.Range(0.9f, 1.1f) * (direction + 0.8f * Vector3.up).normalized, ForceMode2D.Impulse);
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
