using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class MonsterHeal : MonsterState
{
    public override MonsterControl.State mapping => MonsterControl.State.Heal;
    public Vector2 durationRange;
    float duration;
    public override async UniTask Enter(CancellationToken token)
    {
        await UniTask.Yield(token);
        Activate(token).Forget();
        duration = Random.Range(durationRange.x, durationRange.y);
        if (control.data.Type == MonsterType.Large || control.data.Type == MonsterType.Boss)
        {
            bossHUD = FindAnyObjectByType<BossHUD>();
        }
    }
    public override void Exit()
    {
        base.Exit();
        particle?.Despawn();
        particle = null;
    }
    Particle particle;
    BossHUD bossHUD;

    public async UniTask Activate(CancellationToken token)
    {
        await UniTask.Yield(token);

        anim.Play("Heal");
        await UniTask.Delay((int)(1000f * (0.3f * duration)), cancellationToken: token);
        particle = ParticleManager.I.PlayParticle("DarkCharge", transform.position + 0.5f * control.height * Vector3.up, Quaternion.identity);
        particle.transform.localScale = 0.3f * Vector3.one;

        float _startTime = Time.time;
        while (!token.IsCancellationRequested && (Time.time - _startTime) < 0.7f * duration)
        {
            float ratio = (Time.time - _startTime) / (0.7f * duration);
            await UniTask.Delay(50, cancellationToken: token);
            ratio = Mathf.Pow(ratio, 1.8f);
            ratio = Mathf.Clamp01(ratio);
            control.currHealth += (0.11f * control.data.HP + 450f) * (0.5f + 2f * ratio) * 0.015f;
            if (bossHUD)
                bossHUD.Refresh();
        }

        await UniTask.Yield(token);

        particle?.Despawn();
        particle = null;
        control.ChangeNextState();
    }



}
