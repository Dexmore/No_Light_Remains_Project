using UnityEngine;

public class SconceLight : Lanternable, ISavable
{
    #region Lanternable Complement
    public override bool isReady { get { return _isReady; } set { _isReady = value; } }
    bool _isReady;
    public override bool isAuto => false;
    public override ParticleSystem particle => null;
    public override SpriteRenderer lightPoint => null;
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => true;
    int ISavable.ReplayWaitTimeSecond => rndSecond;
    int rndSecond;
    void Awake()
    {
        rndSecond = Random.Range(600, 10000);
    }
    public void SetCompletedState()
    {
        _isReady = false;
        isComplete = true;
    }
    #endregion
    public override void Run()
    {

    }
    public override void PromptFill()
    {

    }
    public override void PromptCancel()
    {

    }



}
