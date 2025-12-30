using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterJump : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Jump;
    // 낭떠러지 체크용
    Vector2 rayOrigin;
    Vector2 rayDirection;
    float rayLength;
    Ray2D checkRay;
    RaycastHit2D CheckRayHit;
    public override async UniTask Enter(CancellationToken token)
    {
        //Debug.Log($"{transform.name} : {control.state}");
        await UniTask.Yield(token);
        Activate(token).Forget();
    }
    public async UniTask Activate(CancellationToken token)
    {
        if (control.isDie) return;
        if (!control.isGround)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        rayOrigin = transform.position + 1.3f * control.width * model.right + 0.2f * control.height * Vector3.up;
        rayDirection = Vector3.down;
        rayLength = 0.9f * control.jumpLength + 0.1f * control.height;
        checkRay.origin = rayOrigin;
        checkRay.direction = rayDirection;
        CheckRayHit = Physics2D.Raycast(checkRay.origin, checkRay.direction, rayLength, control.groundLayer);
        if (CheckRayHit.collider == null)
        {
            await UniTask.Yield(token);
            control.ChangeNextState();
            return;
        }
        if(control.state == MonsterControl.State.Die) return;
        anim.Play("Idle");
        rb.AddForce(Vector2.up * (control.jumpLength + 1.85f) * 175f);
        float startTime = Time.time;
        await UniTask.Delay(1000, cancellationToken: token);
        await UniTask.WaitUntil(() => control.isGround, cancellationToken: token);
        control.ChangeNextState();
    }






}
