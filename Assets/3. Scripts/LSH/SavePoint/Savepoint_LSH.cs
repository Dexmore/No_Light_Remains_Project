using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint_LSH : MonoBehaviour
{
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vector2 pos2D = transform.position;
        string sceneName = SceneManager.GetActiveScene().name;
        SaveManager_LSH.Save(pos2D, sceneName);

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
