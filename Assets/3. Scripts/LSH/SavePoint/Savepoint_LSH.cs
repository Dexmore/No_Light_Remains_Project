using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint_LSH : Interactable
{
    public override bool isReady {get; set;}
    public override bool isAuto => false;
    public override Type type => Type.Normal;
    public override void Run()
    {

        Vector2 pos2D = transform.position;
        string sceneName = SceneManager.GetActiveScene().name;
        DBManager.I.currData.sceneName = sceneName;
        DBManager.I.currData.lastPos = pos2D;
        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        if(playerControl) playerControl.currHealth = DBManager.I.currData.maxHealth;
        DBManager.I.currData.currHealth = DBManager.I.currData.maxHealth;
        DBManager.I.currData.currPotionCount = DBManager.I.currData.maxPotionCount;
        HUDBinder hUDBinder = FindAnyObjectByType<HUDBinder>();
        hUDBinder?.Refresh(1f);
        _ = GameManager.I.SaveAllMonsterAndObject();
        System.DateTime now = System.DateTime.Now;
        string datePart = now.ToString("yyyy.MM.dd");
        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
        DBManager.I.currData.lastTime = $"{datePart}-{secondsOfDay}";
        DBManager.I.Save();
        
        Debug.Log($"[SavePoint] Saved at {pos2D} in scene {sceneName}");

        if (s_current != null && s_current != this)
        {
            s_current.SetInactive();
        }

        s_current = this;

        if (!_activatedOnce)
        {
            // 처음 밟았을 때만 Activate → Idle
            PlayActivateOnce();
            _activatedOnce = true;
        }
    }
    [Header("Components")]
    [SerializeField] private Animator animator;
    [Header("Animator Params")]
    [SerializeField] private string activateTriggerName = "Activate";
    [SerializeField] private string deactivateTriggerName = "Deactivate";

    // 마지막으로 활성화된 세이브포인트
    private static SavePoint_LSH s_current;
    private bool _activatedOnce = false;

    void Awake()
    {
        isReady = true;
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    private void PlayActivateOnce() // 활성화될때 애니메이션 재생
    {
        if (!animator) return;

        animator.ResetTrigger(deactivateTriggerName);
        animator.SetTrigger(activateTriggerName);
    }
    public void SetInactive()
    {
        if (!animator) return;

        animator.ResetTrigger(activateTriggerName);
        animator.SetTrigger(deactivateTriggerName);
    }



}
