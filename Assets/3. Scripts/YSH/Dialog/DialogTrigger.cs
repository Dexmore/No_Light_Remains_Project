using UnityEngine;
public class DialogTrigger : MonoBehaviour, ISavable
{
    public int index;
    Collider2D collider2D;
    void Awake()
    {
        TryGetComponent(out collider2D);
        collider2D.enabled = true;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        isComplete = true;
        GameManager.I.onDialog.Invoke(index, transform);
        collider2D.enabled = false;
    }
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedState()
    {
        isComplete = true;
        collider2D.enabled = false;
    }
    #endregion
    
}
