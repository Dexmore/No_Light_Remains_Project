using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint_LSH : InteractableObject
{
    #region 상호작용 오브젝트는 InteractableObject로 상속하고 구현
    protected override void Start()
    {
        base.Start();
    }
    // OnColisionEnter2D 대신 아래 메서드로 변경했음.
    public override void Run()
    {
        base.Run();

        Vector2 pos2D = transform.position;
        
        string sceneName = SceneManager.GetActiveScene().name;

        //SaveManager_LSH.Save(pos2D, sceneName);

        // DBManager에 관련 기능이 있어서 변경했음.
        DBManager.I.currentCharData.sceneName = sceneName;
        DBManager.I.currentCharData.lastPosition = pos2D;
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
    #endregion
    
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
