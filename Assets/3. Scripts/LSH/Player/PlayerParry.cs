using UnityEngine;

public class PlayerParry : IPlayerState
{
    private readonly PlayerControl ctx;
    private readonly PlayerStateMachine fsm;
    public PlayerParry(PlayerControl ctx, PlayerStateMachine fsm) { this.ctx = ctx; this.fsm = fsm; }
    private const float duration = 0.6f;   // 총 길이
    private const float parryTime = 0.247f;   // 패링 시간
    private float _elapsedTime;
    SFX sfxWait;
    public void Enter()
    {
        flag1 = false;
        isSuccess = false;
        GameManager.I.onParry += ParrySuccessHandler;
        _elapsedTime = 0f;
        sfxWait?.Despawn();
        sfxWait = null;
        ctx.animator.Play("Player_ParryWait");
        sfxWait = AudioManager.I.PlaySFX("ParryWait");
        ctx.Parred = true;
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                adjustedTime2 = duration;
                adjustedTime1 = parryTime * 1.1f + 0.1f;
                break;
            case 1:
                adjustedTime2 = duration * 1.025f + 0.02f;
                adjustedTime1 = parryTime * 0.9f + 0.05f;
                break;
            case 2:
                adjustedTime2 = duration * 1.05f + 0.04f;
                adjustedTime1 = parryTime * 0.85f;
                break;
        }
        //Gear 기어 (격퇴의 기어) 009_ParryGear
        bool outValue = false;
        if (DBManager.I.HasGear("009_ParryGear", out outValue))
        {
            if (outValue)
            {
                int level = DBManager.I.GetGearLevel("009_ParryGear");
                if (level == 0)
                {
                    adjustedTime1 *= 1.28f;
                    adjustedTime1 += 0.05f;
                }
                else if (level == 1)
                {
                    adjustedTime1 *= 1.58f;
                    adjustedTime1 += 0.09f;
                }
            }
        }
    }
    public void Exit()
    {
        ctx.Parred = false;
        GameManager.I.onParry -= ParrySuccessHandler;
        flag1 = false;
        sfxWait?.Despawn();
        sfxWait = null;
    }
    float addTime = 0.4f;
    [HideInInspector] public int lastSuccesCount;
    float lastSuccesTime;
    void ParrySuccessHandler(HitData hitData)
    {
        isSuccess = true;
        if (Time.time - lastSuccesTime < 1.8f)
        {
            if (lastSuccesCount % 2 == 0)
            {
                ctx.animator.Play("Player_Parry2");
            }
            if (lastSuccesCount % 2 == 1)
            {
                ctx.animator.Play("Player_Parry");
            }
            lastSuccesCount++;
            if (lastSuccesCount == 4)
            {
                DBManager.I.SteamAchievement("ACH_PARRY_COMBO_4");
            }
        }
        else
        {
            lastSuccesCount = 0;
            ctx.animator.Play("Player_Parry");
        }
        sfxWait?.Despawn();
        sfxWait = null;
        lastSuccesTime = Time.time;
        //Gear 기어 (반격의 기어) 002_CounterGear
        bool outValue = false;
        if (DBManager.I.HasGear("002_CounterGear", out outValue))
        {
            if (outValue)
            {
                Transform target = hitData.attacker;
                float monsterDamage = hitData.damage;
                // 기본 데미지 20f
                // 반사데미지 0.1f * damage

                int level = DBManager.I.GetGearLevel("002_CounterGear");
                float gearDamage = 0;
                if(level == 0)
                {
                    gearDamage = 10f + 0.1f * monsterDamage;
                }
                else if(level == 1)
                {
                    gearDamage = 15f + 0.12f * monsterDamage;
                }
                Vector2 hitPoint = 0.25f * hitData.hitPoint + 0.75f * (target.transform.position + 1.3f * Vector3.up);
                GameManager.I.onHit.Invoke
                (
                    new HitData
                    (
                        "CounterGear",
                        ctx.transform,
                        target,
                        gearDamage,
                        hitPoint,
                        null,
                        HitData.StaggerType.None,
                        HitData.AttackType.CounterGear
                    )
                );
            }
        }
        GameManager.I.ach_parryCount++;
        // if (GameManager.I.ach_parryCount == 1)
        // {
        //     DBManager.I.SteamAchievement("ACH_PARRY_FIRST");
        // }
        // if (GameManager.I.ach_parryCount >= 50)
        // {
        //     DBManager.I.SteamAchievement("ACH_PARRY_COUNT_50");
        //     GameManager.I.ach_parryCount = 0;
        // }
    }
    bool isSuccess;
    private float adjustedTime1;
    private float adjustedTime2;
    bool flag1 = false;
    public void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > adjustedTime1)
        {
            ctx.Parred = false;
            if (!flag1 && !isSuccess)
            {
                //Debug.Log("Fail");
                flag1 = true;
            }
        }
        if (_elapsedTime > adjustedTime2)
        {
            if (isSuccess)
                fsm.ChangeState(ctx.idle);
        }
        if (_elapsedTime > adjustedTime2 + addTime)
        {
            fsm.ChangeState(ctx.idle);
        }
        if (!flag1 && isSuccess)
        {
            //Debug.Log("Success");
            flag1 = true;
        }
    }
    public void UpdatePhysics()
    {

    }
}
