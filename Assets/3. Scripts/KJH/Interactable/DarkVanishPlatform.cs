using UnityEngine;
public class DarkVanishPlatform : Lanternable, ISavable
{
    public override bool isReady { get { return _isReady; } set { _isReady = value;} }
    public override bool isAuto => false;
    bool _isReady;
    [SerializeField] GameObject platform;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(true);
    }
    public override void Run()
    {
        _isReady = false;
        isComplete = true;
        platform?.SetActive(false);
    }
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => false;
    int ISavable.ReplayWaitTimeSecond => 0;
    public void SetCompletedState()
    {
        _isReady = false;
        platform?.SetActive(false);
    }
    #endregion




}
