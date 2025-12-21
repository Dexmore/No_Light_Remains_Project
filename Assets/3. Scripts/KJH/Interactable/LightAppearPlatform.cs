using UnityEngine;
public class LightAppearPlatform : Lanternable, ISavable
{
    public override bool isReady { get { return _isReady; } set { _isReady = value;} }
    public override bool isAuto => false;
    bool _isReady;
    [SerializeField] GameObject platform;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(false);
    }
    public override void Run()
    {
        _isReady = false;
        isComplete = true;
        platform?.SetActive(true);
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
        platform?.SetActive(true);
        Debug.Log("aa");
    }
    #endregion


}
