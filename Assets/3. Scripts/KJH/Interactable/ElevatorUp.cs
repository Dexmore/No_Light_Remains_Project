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
    GameObject collision;
    void Awake()
    {
        platform = transform.Find("Platform");
        platfomrInitPos = platform.position;
        collision = transform.Find("Collision").gameObject;
        collision.SetActive(false);
    }
    public override void Run()
    {
        collision.SetActive(true);
        sfx = AudioManager.I.PlaySFX("ElevatorUp");
        tween = platform.DOLocalMoveY(15f,5f).SetEase(Ease.Linear).Play().SetLink(gameObject);
        isReady = false;
        StartCoroutine(nameof(ReplayWait));
    }
    Tween tween;
    SFX sfx;
    void OnDisable()
    {
        sfx?.Despawn();
        sfx = null;
        tween.Kill();
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
