using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;
public class SconceLight : Lanternable, ISavable
{
    #region Lanternable Complement
    public override bool isReady { get { return _isReady; } set { _isReady = value; } }
    bool _isReady;
    public override bool isAuto => false;
    public override ParticleSystem particle => _particle;
    public override SpriteRenderer lightPoint => _lightPoint;
    #endregion
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => canReplay;
    int ISavable.ReplayWaitTimeSecond => replayWaitTimeSecond;
    public void SetCompletedImmediately()
    {
        _isReady = false;
        isComplete = true;
        _lightPoint.gameObject.SetActive(true);
        _particle.gameObject.SetActive(true);
        _light2D.gameObject.SetActive(true);
        _particle.Play();
    }
    #endregion
    bool firstIsComplete;
    ParticleSystem _particle;
    SpriteRenderer _lightPoint;
    GameObject _light2D;
    void Awake()
    {
        base.fillSpeed = 2.2f;
        replayWaitTimeSecond = Random.Range(86400, 864000);
        _lightPoint = transform.Find("LightPoint").GetComponent<SpriteRenderer>();
        _particle = transform.GetComponentInChildren<ParticleSystem>(true);
        _light2D = transform.GetComponentInChildren<Light2D>().gameObject;
        if (firstIsComplete)
        {
            _isReady = false;
            isComplete = true;
            _lightPoint.gameObject.SetActive(true);
            _particle.gameObject.SetActive(true);
            _light2D.gameObject.SetActive(true);
            _particle.Play();
        }
        else
        {
            _isReady = true;
            isComplete = false;
            _lightPoint.gameObject.SetActive(false);
            _particle.gameObject.SetActive(false);
            _light2D.gameObject.SetActive(false);
            _particle.Stop();
        }
    }
    public override async void Run()
    {
        if (!isReady) return;
        DBManager.I.currData.ach12count++;
        if (DBManager.I.currData.ach12count >= 20)
        {
            DBManager.I.SteamAchievement("ACH_LUMENTECH");
        }
        AudioManager.I.PlaySFX("UIClick2");
        SetCompletedImmediately();
        await Task.Delay(200);
        while (!_lightPoint.gameObject.activeSelf)
        {
            _lightPoint.gameObject.SetActive(true);
            await Task.Delay(200);
        }
    }
    public override void PromptFill()
    {
        _lightPoint.gameObject.SetActive(true);
    }
    public override async void PromptCancel()
    {
        _lightPoint.gameObject.SetActive(false);
    }
    [Header("한번만 할수있는지or씬이동시 반복가능한지 여부")]
    [SerializeField] bool canReplay;
    int replayWaitTimeSecond;



}
