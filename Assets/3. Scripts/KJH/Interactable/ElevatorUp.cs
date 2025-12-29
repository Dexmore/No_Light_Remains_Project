using UnityEngine;
using DG.Tweening;
using System.Collections;
public class ElevatorUp : Interactable
{
    #region Interactable Complement
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    
    #endregion
    Transform platform;
    void Awake()
    {
        platform = transform.Find("Platform");
        platfomrInitPos = platform.position;
    }
    public override void Run()
    {
        sfx = AudioManager.I.PlaySFX("ElevatorUp");
        platform.DOLocalMoveY(15f,5f).SetEase(Ease.Linear);
        isReady = false;
        StartCoroutine(nameof(ReplayWait));
    }
    SFX sfx;
    void OnDisable()
    {
        sfx?.Despawn();
        sfx = null;
    }
    Vector3 platfomrInitPos;
    IEnumerator ReplayWait()
    {
        yield return YieldInstructionCache.WaitForSeconds(5f);
        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        //플레이어가 엘리베이터가 안 보일정도로 화면 밖으로 벗어났는지
        while(true)
        {
            if(Vector3.Distance(playerControl.transform.position, transform.position) > 40f)
            {
                platform.position = platfomrInitPos;
                isReady = true;
                break;
            }
            yield return YieldInstructionCache.WaitForSeconds(2f);
        }
    }


}
