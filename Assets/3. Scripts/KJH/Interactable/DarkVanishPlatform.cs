using UnityEngine;
using UnityEngine.Rendering.Universal;
public class DarkVanishPlatform : Lanternable, ISavable
{
    public override bool isReady { get { return _isReady; } set { _isReady = value;} }
    public override bool isAuto => false;
    public override ParticleSystem particle => ps;
    public override SpriteRenderer lightPoint => lp;
    bool _isReady;
    ParticleSystem ps;
    SpriteRenderer lp;
    Light2D light2;
    [SerializeField] GameObject platform;
    void Awake()
    {
        _isReady = true;
        isComplete = false;
        platform?.SetActive(true);
        ps = transform.GetComponentInChildren<ParticleSystem>(true);
        ps?.gameObject.SetActive(false);
        lp = transform.Find("LightPoint").GetComponent<SpriteRenderer>();
        lp?.gameObject.SetActive(false);
        light2 = transform.Find("Light(2)").GetComponent<Light2D>();
        light2?.gameObject.SetActive(false);
    }
    public override void PromptFill()
    {
        
    }
    public override void PromptCancel()
    {
        
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
        isComplete = true;
        platform?.SetActive(false);
    }
    #endregion




}
