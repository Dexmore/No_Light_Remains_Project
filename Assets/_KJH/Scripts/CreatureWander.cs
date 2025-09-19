using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CreatureWander : CreatureAbility
{
    public override CreatureControl.State mapping => CreatureControl.State.Wander;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Init(CancellationToken token)
    {
        //Debug.Log($"{transform.name} : {control.state}");
        await UniTask.Yield(cts.Token);
        duration = Random.Range(durationRange.x, durationRange.y);
        Activate(token).Forget();
    }
    public override async UniTask Activate(CancellationToken token)
    {
        float startTime = Time.time;
        Vector2 direction = Vector2.zero;
        if (Random.value <= 0.5f)
        {
            direction = Vector2.right;
            transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }
        else
        {
            direction = Vector2.left;
            transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
        }
        while (Time.time - startTime < duration)
        {
            rb.AddForce(direction * 3f * control.data.MoveSpeed);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cts.Token);
        }
        control.GoNextState();
    }
    public override void UnInit()
    {
        base.UnInit();
    }






}
