using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{

    [Header("Player HP")]
    public float maxHealth = 1000;
    public float currHealth;

    [Header("Light Resource")]
    public float maxBattery = 100;
    public float currBattery;

    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Input (use bound actions)")]
    public InputActionAsset inputActionAsset;
    private InputAction lanternAction;
    private InputAction jumpAction;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Transform childTR;
    [HideInInspector] public AttackRange attackRange;
    [HideInInspector] public PlayerStateMachine fsm;
    // States
    [HideInInspector] public PlayerIdle idle;
    [HideInInspector] public PlayerRun run;
    [HideInInspector] public PlayerJump jump;
    [HideInInspector] public PlayerFall fall;
    [HideInInspector] public PlayerAttack attack;
    [HideInInspector] public PlayerAttackCombo attackCombo;
    [HideInInspector] public PlayerAttackCombo2 attackCombo2;
    [HideInInspector] public PlayerDash dash;
    [HideInInspector] public PlayerParry parry;
    [HideInInspector] public PlayerHit hit;
    [HideInInspector] public PlayerDie die;
    [HideInInspector] public PlayerUsePotion usePotion;
    [HideInInspector] public PlayerOpenInventory openInventory;
    [HideInInspector] public PlayerStop stop;
    // [HideInInspector] public PlayerJumpAttack jumpAttack;
    [HideInInspector] public HUDBinder hUDBinder;

    // === Ground 체크 ===
    [Header("Ground Sensor (정교 판정)")]
    public LayerMask groundLayer;
    CapsuleCollider2D capsuleCollider2D;
    [HideInInspector] public float height;
    [HideInInspector] public float width;
    // private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];
    [HideInInspector] public Dictionary<Collider2D, Vector2> collisions = new Dictionary<Collider2D, Vector2>();
    // Runtime
    [Tooltip("특정 플랫폼을 통과할 수 있는지 확인 여부")] public bool fallThroughPlatform;

    [Header("Parry 성공 시 나오는 FX 및 TimeFreeze 효과")]
    [SerializeField] private string parryParticle = "ParrySuccess";
    [SerializeField] private float parryHitStopDuration = 0.08f;
    [SerializeField, Range(0f, 1f)] private float parryHitStopTimeScale = 0.05f;
    private float _hitStopEndRealtime = -1f;
    [SerializeField] private float baseTimeScale = 1f;
    private Coroutine _parryHitStopCo;
    private float _defaultFixedDeltaTime;
    [HideInInspector] public bool Grounded { get { return _Grounded; } set { _Grounded = value; } }
    [ReadOnlyInspector][SerializeField] private bool _Grounded;
    [HideInInspector] public bool Parred { get { return _Parred; } set { _Parred = value; } }
    [ReadOnlyInspector][SerializeField] private bool _Parred;
    [HideInInspector] public bool Avoided { get { return _Avoided; } set { _Avoided = value; } }
    [ReadOnlyInspector][SerializeField] private bool _Avoided;
    [HideInInspector] public bool Dead { get { return _Dead; } set { _Dead = value; } }
    [ReadOnlyInspector][SerializeField] private bool _Dead;

    void Awake()
    {
        TryGetComponent(out rb);
        childTR = transform.GetChild(0);
        animator = GetComponentInChildren<Animator>(true);
        capsuleCollider2D = GetComponentInChildren<CapsuleCollider2D>(true);
        attackRange = GetComponentInChildren<AttackRange>(true);
        height = capsuleCollider2D.size.y;
        width = capsuleCollider2D.size.x;
        PlayerLight = GetComponentInChildren<PlayerLight>(true);
        fsm = new PlayerStateMachine();
        idle = new PlayerIdle(this, fsm);
        run = new PlayerRun(this, fsm);
        jump = new PlayerJump(this, fsm);
        fall = new PlayerFall(this, fsm);
        attack = new PlayerAttack(this, fsm);
        attackCombo = new PlayerAttackCombo(this, fsm);
        attackCombo2 = new PlayerAttackCombo2(this, fsm);
        parry = new PlayerParry(this, fsm);
        dash = new PlayerDash(this, fsm);
        hit = new PlayerHit(this, fsm);
        die = new PlayerDie(this, fsm);
        usePotion = new PlayerUsePotion(this, fsm);
        openInventory = new PlayerOpenInventory(this, fsm);
        stop = new PlayerStop(this, fsm);
        InitMatInfo();
        sfxFootStep = GetComponentInChildren<AudioSource>();
        hUDBinder = FindAnyObjectByType<HUDBinder>();

        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }
    void Start()
    {
        GameObject light0 = PlayerLight.transform.GetChild(0).gameObject;
        GameObject light1 = PlayerLight.transform.GetChild(1).gameObject;
        GameObject light2 = PlayerLight.transform.GetChild(2).gameObject;
        CharacterData characterData = DBManager.I.currData;
        if (characterData.sceneName == "" && characterData.maxHealth == 0)
        {
            // 테스트 플레이어 시작 영역 (정상적인 로비-캐릭터 선택씬을 거친 게임흐름과 무관함)
            DBManager.I.CloseLoginUI();
            CharacterData newData = new CharacterData();
            newData.gold = 0;
            newData.death = 0;
            newData.sceneName = "Stage0";
            newData.lastPos = Vector2.zero;
            newData.maxHealth = maxHealth;
            newData.maxBattery = maxBattery;
            newData.currHealth = currHealth;
            newData.currBattery = currBattery;
            newData.mpc = 3;
            newData.cpc = 3;
            newData.mgc = 3;
            newData.difficulty = 0;
            newData.seed = Random.Range(1, 9999);
            
            newData.itemDatas = new List<CharacterData.ItemData>();
            newData.gearDatas = new List<CharacterData.GearData>();
            newData.lanternDatas = new List<CharacterData.LanternData>();
            newData.recordDatas = new List<CharacterData.RecordData>();
            newData.sceneDatas = new List<CharacterData.SData>();
            newData.pds = new List<CharacterData.ProgressData>();
            newData.ks = new List<CharacterData.KillCount>();

            newData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            newData.lastPos = transform.position;
            System.DateTime now = System.DateTime.Now;
            string datePart = now.ToString("yyyy.MM.dd");
            int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
            newData.lastTime = $"{datePart}-{secondsOfDay}";
            DBManager.I.currData = newData;
            // DBManager.I.savedData = newData; (이줄 제거)
            // 깊은 복사
            string json = JsonUtility.ToJson(newData);
            DBManager.I.savedData = JsonUtility.FromJson<CharacterData>(json);
            light0.SetActive(false);
            light1.SetActive(false);
            light2.SetActive(true);
            // 신규캐릭터 시작 아이템
            DBManager.I.AddLantern("BasicLantern");
            int find = DBManager.I.currData.lanternDatas.FindIndex(x => x.Name == "BasicLantern");
            CharacterData.LanternData lanternData = DBManager.I.currData.lanternDatas[find];
            lanternData.isEquipped = true;
            lanternData.isNew = false;
            DBManager.I.currData.lanternDatas[find] = lanternData;
            startPosition = transform.position;
        }
        else
        {
            maxHealth = DBManager.I.currData.maxHealth;
            maxBattery = DBManager.I.currData.maxBattery;
            currHealth = DBManager.I.currData.currHealth;
            currBattery = DBManager.I.currData.currBattery;
            light0.SetActive(GameManager.I.isLanternOn);
            light1.SetActive(GameManager.I.isLanternOn);
            light2.SetActive(!GameManager.I.isLanternOn);
            Random.InitState(DBManager.I.currData.seed);
        }
        inventoryUI = FindAnyObjectByType<Inventory>();
        StartCoroutine(nameof(DecreaseBattery));

        //Gear 기어 (초신성 기어) 006_SuperNovaGear
        bool outValue1 = false;
        if (DBManager.I.HasGear("006_SuperNovaGear", out outValue1))
        {
            if (outValue1)
            {
                GameManager.I.isSuperNovaGearEquip = true;
            }
            else
            {
                GameManager.I.isSuperNovaGearEquip = false;
            }
        }
        else
        {
            GameManager.I.isSuperNovaGearEquip = false;
        }

    }

    void OnEnable()
    {
        fsm.ChangeState(idle);
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed += DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += DashCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled += DashCancel;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled += DashCancel;
        inputActionAsset.FindActionMap("Player").FindAction("Jump").canceled += JumpCancel;
        // jumpAction = inputActionAsset.FindActionMap("Player").FindAction("Jump");
        lanternAction = inputActionAsset.FindActionMap("Player").FindAction("Lantern");
        lanternAction.performed += LanternInput;
        GameManager.I.onHit += HitHandler;
    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").performed -= DashInput;
        inputActionAsset.FindActionMap("Player").FindAction("LeftDash").canceled -= DashCancel;
        inputActionAsset.FindActionMap("Player").FindAction("RightDash").canceled -= DashCancel;
        inputActionAsset.FindActionMap("Player").FindAction("Jump").canceled -= JumpCancel;
        lanternAction.performed -= LanternInput;
        // jumpAction = null;
        GameManager.I.onHit -= HitHandler;
        fsm.OnDisable();
    }

    void Update()
    {
        fsm.Update();
        CheckGroundedPrecise();
        FixBugPosition();
        //CheckPlatformFallThrough();
    }
    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        //if ((collision.collider.gameObject.layer & groundLayer) != 0)
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            if (!collisions.ContainsKey(collision.collider))
                collisions.Add(collision.collider, collision.contacts[0].point);
            else
                collisions[collision.collider] = collision.contacts[0].point;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            if (collisions.ContainsKey(collision.collider))
                collisions.Remove(collision.collider);
    }
    Ray2D groundRay = new Ray2D();
    RaycastHit2D groundRayHit;
    float groundCheckTime;
    void CheckGroundedPrecise()
    {
        //_Grounded = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.1f * capsuleCollider2D.size.y)
                {
                    _Grounded = true;
                    return;
                }

        if (Time.time - groundCheckTime > Random.Range(0.1f, 0.3f))
        {
            groundCheckTime = Time.time;
            groundRay.origin = (Vector2)transform.position + 0.08f * Vector2.up;
            groundRay.direction = Vector2.down;
            groundRayHit = Physics2D.Raycast(groundRay.origin, groundRay.direction, 0.1f, groundLayer);
            if (groundRayHit)
            {
                _Grounded = true;
            }
            else
            {
                _Grounded = false;
            }
        }
    }

    #region Dash
    int leftDashInputCount = 0;
    int rightDashInputCount = 0;
    void DashInput(InputAction.CallbackContext callback)
    {
        if (!_Grounded) return;
        if (fsm.currentState == dash) return;
        if (callback.action.name == "LeftDash")
        {
            if (leftDashInputCount == 0)
            {
                leftDashInputCount = 1;
                if (rightDashInputCount != 0) rightDashInputCount = 0;
                StopCoroutine(nameof(DashRelease));
                StartCoroutine(nameof(DashRelease));
            }
            else if (leftDashInputCount == 2)
            {
                if (rightDashInputCount != 0) rightDashInputCount = 0;
                dash.isLeft = true;
                isDash = true;
                StopCoroutine(nameof(Dash));
                StartCoroutine(nameof(Dash));
            }
        }
        else if (callback.action.name == "RightDash")
        {
            if (rightDashInputCount == 0)
            {
                rightDashInputCount = 1;
                if (leftDashInputCount != 0) leftDashInputCount = 0;
                StopCoroutine(nameof(DashRelease));
                StartCoroutine(nameof(DashRelease));
            }
            else if (rightDashInputCount == 2)
            {
                if (leftDashInputCount != 0) leftDashInputCount = 0;
                dash.isLeft = false;
                isDash = true;
                StopCoroutine(nameof(Dash));
                StartCoroutine(nameof(Dash));
            }
        }
    }
    [ReadOnlyInspector] public bool isDash;
    void DashCancel(InputAction.CallbackContext callback)
    {
        if (!_Grounded) return;
        if (leftDashInputCount == 1)
            leftDashInputCount = 2;
        if (rightDashInputCount == 1)
            rightDashInputCount = 2;
    }
    IEnumerator DashRelease()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.2f);
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    IEnumerator Dash()
    {
        float time = Time.time;
        while (Time.time - time < 0.48f)
        {
            yield return null;
            if (fsm.currentState == dash)
            {
                isDash = false;
                leftDashInputCount = 0;
                rightDashInputCount = 0;
                StopCoroutine(nameof(DashRelease));
                yield break;
            }
            if (fsm.currentState == hit) break;
            if (fsm.currentState == idle || fsm.currentState == run)
            {
                fsm.ChangeState(dash);
                break;
            }
        }
        yield return null;
        isDash = false;
        leftDashInputCount = 0;
        rightDashInputCount = 0;
    }
    #endregion
    [ReadOnlyInspector] public bool Jumped;
    void JumpCancel(InputAction.CallbackContext callback)
    {
        Jumped = false;
    }
    Camera _mainCamera;
    [HideInInspector] public Inventory inventoryUI;
    float parrySuccessTime;
    void HitHandler(HitData hData)
    {
        if (hData.attacker.Root() == transform)
        {
            MonsterControl monsterControl = hData.target.GetComponentInParent<MonsterControl>();
            if (monsterControl != null)
            {
                //Gear 기어 (수복의 기어) 005_RestorationGear
                bool outValue = false;
                if (DBManager.I.HasGear("005_RestorationGear", out outValue))
                {
                    if (outValue)
                    {
                        int level = DBManager.I.GetGearLevel("005_RestorationGear");
                        if (level == 0)
                        {
                            currHealth += 4f;
                        }
                        else if (level == 1)
                        {
                            currHealth += 8f;
                        }
                        currHealth = Mathf.Clamp(currHealth, 0, maxHealth);
                        hUDBinder.Refresh(1f);
                    }
                }
                float tempFloat = 1f;
                if (monsterControl.data.Type == MonsterType.Large || monsterControl.data.Type == MonsterType.Boss)
                    tempFloat = 0.36f;
                currBattery += lanternAttackAmount * tempFloat;
                currBattery = Mathf.Clamp(currBattery, 0, maxBattery);
                hUDBinder.RefreshBattery();
            }
        }
        if (hData.target.Root() != transform) return;
        if (fsm.currentState == die) return;
        if (hData.attackType == HitData.AttackType.Chafe)
        {
            if (isHit2) return;
            if (isHit1) return;
            isHit1 = true;
            hitCoolTime1speed = 1f;
            run.isStagger = true;
            StopCoroutine(nameof(HitCoolTime1));
            StartCoroutine(nameof(HitCoolTime1));
            if (_Avoided)
            {
                //Debug.Log("회피 성공");
                return;
            }
            if (fsm.currentState != jump
            && fsm.currentState != fall
            && !fallThroughPlatform)
            {
                Vector2 dir = 4.2f * (hData.target.position.x - hData.attacker.position.x) * Vector2.right;
                dir.y = 2f;
                Vector3 velo = rb.linearVelocity;
                rb.linearVelocity = 0.4f * velo;
                rb.AddForce(dir, ForceMode2D.Impulse);
            }
            currHealth -= (int)hData.damage;
            currHealth = Mathf.Clamp(currHealth, 0f, maxHealth);
            DBManager.I.currData.currHealth = currHealth;
            if (hData.particleNames != null)
            {
                int rnd = Random.Range(0, hData.particleNames.Length);
                string particleName = hData.particleNames[rnd];
                ParticleManager.I.PlayParticle(particleName, hData.hitPoint, Quaternion.identity, null);
            }
            AudioManager.I.PlaySFX("HitLittle", hData.hitPoint, null);
            if (currHealth <= 0)
                fsm.ChangeState(die);
            HitChangeColor(Color.white, 0);
            if (fsm.currentState == openInventory)
            {
                fsm.ChangeState(idle);
            }
            GameManager.I.onHitAfter.Invoke(hData);
            return;



        }
        else if (hData.attackType != HitData.AttackType.Chafe)
        {

            if (isHit2) return;
            isHit2 = true;
            StopCoroutine(nameof(HitCoolTime2));
            StartCoroutine(nameof(HitCoolTime2));

            if (_Avoided)
            {
                GameManager.I.onAvoid.Invoke(hData);
                ParticleManager.I.PlayText("Miss", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                AudioManager.I.PlaySFX("Woosh1");
                hitCoolTime1speed = 10f;
                hitCoolTime2speed = 10f;
                return;
            }
            if (_Parred)
            {
                if (!hData.isCannotParry)
                {
                    if (Time.time - parrySuccessTime > 0.3f)
                    {
                        parrySuccessTime = Time.time;
                        AudioManager.I.PlaySFX("Parry");
                        ParticleManager.I.PlayText("Parry", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                        GameManager.I.onParry.Invoke(hData);
                        if (hData.attackType == HitData.AttackType.Bullet)
                        {
                            Bullet bullet = hData.attacker.GetComponent<Bullet>();
                            bullet.Despawn();
                        }
                        GameManager.I.HitEffect(hData.hitPoint, 0.25f);
                        ParticleManager.I.PlayParticle("RadialLines", hData.hitPoint, Quaternion.identity);

                        OnParrySuccess(hData);

                        if (_mainCamera == null) _mainCamera = Camera.main;
                        UIParticle upa = ParticleManager.I.PlayUIParticle("UIAttBattery", MethodCollection.WorldTo1920x1080Position(transform.position, _mainCamera), Quaternion.identity);
                        AttractParticle ap = upa.GetComponent<AttractParticle>();
                        Vector3 pos = _mainCamera.ViewportToWorldPoint(new Vector3(0.07f, 0.85f, 0f));
                        ap.targetVector = pos;
                        // MonsterControl monsterControl = hData.attacker.GetComponent<Monster>
                        float tempFloat = 1f;
                        if (hData.attacker.TryGetComponent(out MonsterControl monsterControl))
                            if (monsterControl.data.Type == MonsterType.Large || monsterControl.data.Type == MonsterType.Boss)
                                tempFloat = 0.34f;

                        currBattery += lanternParryAmount * tempFloat;
                        currBattery = Mathf.Clamp(currBattery, 0, maxBattery);
                        hUDBinder.RefreshBattery();
                        hitCoolTime1speed = 30f;
                        hitCoolTime2speed = 30f;
                        StartCoroutine(nameof(ReleaseParred));
                        return;
                    }

                }
                else
                {
                    AudioManager.I.PlaySFX("Fail1");
                    ParticleManager.I.PlayText("Cannot Parry", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                    //Debug.Log("패링 불가 공격");
                }
            }


            hitCoolTime1speed = 1f;
            hitCoolTime2speed = 1f;
            if (usePotion.cts != null && !usePotion.cts.IsCancellationRequested)
            {
                usePotion.cts?.Cancel();
                usePotion.cts = null;
                usePotion.sfx2?.Despawn();
                usePotion.sfx2 = null;
            }

            float multiplier = 1f;
            switch (hData.staggerType)
            {
                case HitData.StaggerType.Small:
                    multiplier = 1.05f;
                    GameManager.I.HitEffect(hData.hitPoint, 0.25f);
                    break;
                case HitData.StaggerType.Middle:
                    multiplier = 1.22f;
                    GameManager.I.HitEffect(hData.hitPoint, 0.45f);
                    break;
                case HitData.StaggerType.Large:
                    multiplier = 1.29f;
                    GameManager.I.HitEffect(hData.hitPoint, 0.61f);
                    break;
            }
            Vector2 dir = 2.8f * multiplier * (hData.target.position.x - hData.attacker.position.x) * Vector2.right;
            dir.y = 2.1f * Mathf.Sqrt(multiplier) + (multiplier - 1f);
            if (Random.value < 0.23f) dir.y *= 0.2f;
            if (Random.value < 0.23f) dir.y = 0f;
            Vector3 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            rb.AddForce(dir, ForceMode2D.Impulse);
            if (hData.staggerType != HitData.StaggerType.None)
            {
                hit.staggerType = hData.staggerType;
                fsm.ChangeState(hit);
            }
            float diffMultiplier = 1f;
            switch (DBManager.I.currData.difficulty)
            {
                case 0:
                    diffMultiplier = 0.79f;
                    break;
                case 1:
                    diffMultiplier = 0.9f;
                    break;
                case 2:
                    diffMultiplier = 1.1f;
                    break;
            }
            currHealth -= (int)(hData.damage * diffMultiplier);
            currHealth = Mathf.Clamp(currHealth, 0f, maxHealth);
            DBManager.I.currData.currHealth = currHealth;
            if (currHealth <= 0)
            {
                fsm.ChangeState(die);
                GameManager.I.onDie.Invoke(hData);
            }
            if (hData.particleNames != null)
            {
                int rnd = Random.Range(0, hData.particleNames.Length);
                string particleName = hData.particleNames[rnd];
                ParticleManager.I.PlayParticle(particleName, hData.hitPoint, Quaternion.identity, null);
            }
            AudioManager.I.PlaySFX("Hit8bit2", hData.hitPoint, null);
            HitChangeColor(Color.white, 1);
            GameManager.I.onHitAfter.Invoke(hData);
            if (hData.attackType == HitData.AttackType.Bullet)
            {
                if (Random.value < 0.5f)
                {
                    Bullet bullet = hData.attacker.GetComponent<Bullet>();
                    bullet.Despawn();
                }
            }
            return;
        }
    }
    class MatInfo
    {
        public SpriteRenderer spriteRenderer;
        public Material[] originalMats;
        public Sequence[] sequences;
    }
    List<MatInfo> matInfos = new List<MatInfo>();
    void InitMatInfo()
    {
        matInfos.Clear();
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < srs.Length; i++)
        {
            MatInfo matInfo = new MatInfo();
            matInfo.spriteRenderer = srs[i];
            matInfo.originalMats = srs[i].sharedMaterials;
            matInfo.sequences = new Sequence[srs[i].sharedMaterials.Length];
            matInfos.Add(matInfo);
        }
    }
    [SerializeField] AnimationCurve hitColorCurve;
    void HitChangeColor(Color color, int index = 0)
    {
        foreach (var element in matInfos)
        {
            Material[] newMats = new Material[element.spriteRenderer.materials.Length];
            // element변수의 로컬 복사본을 만듭니다 (클로저 문제방지)
            var currentElement = element;
            for (int i = 0; i < currentElement.originalMats.Length; i++)
            {
                // 루프변수i의 로컬 복사본을 만듭니다 (클로저 문제방지)
                int materialIndex = i;
                if (currentElement.sequences[materialIndex] != null && currentElement.sequences[materialIndex].IsActive())
                    currentElement.sequences[materialIndex].Kill();
                newMats[materialIndex] = Instantiate(GameManager.I.hitTintMat);
                newMats[materialIndex].color = currentElement.originalMats[materialIndex].color;
                newMats[materialIndex].SetColor("_TintColor", new Color(color.r, color.g, color.b, 1f));
                currentElement.sequences[materialIndex] = DOTween.Sequence();
                currentElement.sequences[materialIndex].AppendInterval(0.09f);
                float _dur = 0.3f;
                if (index == 1) _dur = 0.52f;
                Tween colorTween = newMats[materialIndex].DOVector(
                    new Vector4(color.r, color.g, color.b, 0f),
                    "_TintColor",
                    _dur
                ).SetEase(hitColorCurve).SetLink(gameObject);
                currentElement.sequences[materialIndex].Append(colorTween);
                currentElement.sequences[materialIndex].OnComplete(() =>
                {
                    Material[] currentMats = currentElement.spriteRenderer.materials;
                    currentMats[materialIndex] = currentElement.originalMats[materialIndex];
                    currentElement.spriteRenderer.materials = currentMats;
                    // 인스턴스화된 hitTintMat을 제거합니다. (메모리 누수 방지)
                    Destroy(newMats[materialIndex]);
                });
                currentElement.sequences[materialIndex].Play();
            }
            currentElement.spriteRenderer.materials = newMats;
        }
    }
    bool isHit1;
    bool isHit2;
    IEnumerator HitCoolTime1()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.2f);
        run.isStagger = false;
        float elapsed = 0;
        float rnd = Random.Range(0.5f, 0.7f);
        while (elapsed < rnd)
        {
            elapsed += hitCoolTime1speed * Time.deltaTime;
            yield return null;
        }
        hitCoolTime1speed = 1f;
        isHit1 = false;
    }
    float hitCoolTime1speed = 1f;
    float hitCoolTime2speed = 1f;
    IEnumerator HitCoolTime2()
    {
        float elapsed = 0;
        while (elapsed < 1.12f)
        {
            elapsed += hitCoolTime2speed * Time.deltaTime;
            yield return null;
        }
        isHit2 = false;
        hitCoolTime2speed = 1f;
    }
    IEnumerator ReleaseParred()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.13f);
        _Parred = false;
    }
    #region Turn ON/OFF Lantern
    PlayerLight PlayerLight;
    float batteryTextCooltime;
    void LanternInput(InputAction.CallbackContext callback)
    {
        if (_Dead) return;
        if (fsm.currentState == stop) return;
        if (fsm.currentState == openInventory) return;
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;
        AudioManager.I.PlaySFX("FlashlightClick");
        GameObject light0 = PlayerLight.transform.GetChild(0).gameObject;
        GameObject light1 = PlayerLight.transform.GetChild(1).gameObject;
        GameObject light2 = PlayerLight.transform.GetChild(2).gameObject;
        if (light0.activeSelf)
        {
            light0.SetActive(false);
            light1.SetActive(false);
            light2.SetActive(true);
            GameManager.I.isLanternOn = false;
        }
        else
        {
            if (currBattery <= 3)
            {
                if (Time.time - batteryTextCooltime > 1.2f)
                {
                    batteryTextCooltime = Time.time;
                    ParticleManager.I.PlayText("Empty Battery", transform.position + Vector3.up, ParticleManager.TextType.PlayerNotice);
                }
                return;
            }
            light0.SetActive(true);
            light1.SetActive(true);
            light2.SetActive(false);
            GameManager.I.isLanternOn = true;
            GameManager.I.lanternOnStartTime = Time.time;
        }
    }
    [HideInInspector] public bool isNearSavePoint;
    [HideInInspector] public bool isNearSconceLight;
    public AnimationCurve curve;
    [HideInInspector] public bool isBatteryMalfunction;
    IEnumerator DecreaseBattery()
    {
        float diffMultiplier = 1f;
        switch (DBManager.I.currData.difficulty)
        {
            case 0:
                diffMultiplier = 0.7f;
                break;
            case 1:
                diffMultiplier = 0.85f;
                break;
            case 2:
                diffMultiplier = 1f;
                break;
        }

        float interval = 0.08f;
        while (true)
        {
            yield return YieldInstructionCache.WaitForSeconds(interval);
            float lanternOnTime = GameManager.I.isLanternOn ? (Time.time - GameManager.I.lanternOnStartTime) : 0f;
            float batteryPercent = currBattery / maxBattery;
            bool isCharging = isNearSavePoint || isNearSconceLight;
            if (isCharging && GameManager.I.isLanternOn)
            {
                GameManager.I.lanternOnStartTime = Time.time;
            }
            hUDBinder.UpdateBatteryUI(isCharging, batteryPercent, lanternOnTime, isBatteryMalfunction);
            float malfunctionFactor = 1f;
            if (!isCharging && GameManager.I.isLanternOn && !hUDBinder.isWarring && lanternOnTime > 10f && !isBatteryMalfunction)
            {
                if (Random.value < 0.006f)
                    isBatteryMalfunction = true;
            }
            if (!GameManager.I.isLanternOn) isBatteryMalfunction = false;
            if (isBatteryMalfunction) malfunctionFactor = 1.2f;
            if (isNearSavePoint)
            {
                if (currBattery <= 100)
                {
                    if (fsm.currentState == die) continue;
                    currBattery += 28f * interval;
                    currBattery = Mathf.Clamp(currBattery, 0f, maxBattery);
                    DBManager.I.currData.currBattery = currBattery;
                    hUDBinder.RefreshBattery();
                }
            }
            else if (isNearSconceLight)
            {
                if (currBattery <= 58)
                {
                    if (fsm.currentState == die) continue;
                    currBattery += 6.7f * interval;
                    currBattery = Mathf.Clamp(currBattery, 0f, maxBattery);
                    DBManager.I.currData.currBattery = currBattery;
                    hUDBinder.RefreshBattery();
                }
            }
            else
            {
                if (GameManager.I.isLanternOn)
                {
                    float timeDiff = Time.time - GameManager.I.lanternOnStartTime;
                    float tempFloat = Mathf.Clamp01(timeDiff / 14f);
                    float tempFloat2 = curve.Evaluate(tempFloat);
                    //Debug.Log($"timeDiff:{timeDiff},tempFloat2:{tempFloat2}");
                    float isOpenUI = 1f;
                    if (GameManager.I.isOpenDialog) isOpenUI = 0.2f;
                    if (GameManager.I.isOpenPop) isOpenUI = 0.4f;
                    //Gear 기어 (초신성 기어) 006_SuperNovaGear
                    float gearMultiplier = 1f;
                    if (GameManager.I.isSuperNovaGearEquip)
                    {
                        int level = DBManager.I.GetGearLevel("006_SuperNovaGear");
                        if (level == 0)
                        {
                            gearMultiplier = 1.5f;
                        }
                        else if (level == 1)
                        {
                            gearMultiplier = 1.3f;
                        }
                    }
                    currBattery += lanternDecreaseTick * diffMultiplier * gearMultiplier * isOpenUI * interval * tempFloat2 * malfunctionFactor;
                    currBattery = Mathf.Clamp(currBattery, 0f, maxBattery);
                    DBManager.I.currData.currBattery = currBattery;
                    hUDBinder.RefreshBattery();
                    if (currBattery <= 0)
                    {
                        AudioManager.I.PlaySFX("FlashlightClick");
                        ParticleManager.I.PlayText("Empty Battery", transform.position + Vector3.up, ParticleManager.TextType.PlayerNotice);
                        GameObject light0 = PlayerLight.transform.GetChild(0).gameObject;
                        GameObject light1 = PlayerLight.transform.GetChild(1).gameObject;
                        GameObject light2 = PlayerLight.transform.GetChild(2).gameObject;
                        light0.SetActive(false);
                        light1.SetActive(false);
                        light2.SetActive(true);
                        hUDBinder.RefreshBattery();
                        GameManager.I.isLanternOn = false;
                    }
                }
                else
                {
                    if (currBattery <= 15f)
                    {
                        if (fsm.currentState == die) continue;
                        currBattery += 1.3f * interval;
                        currBattery = Mathf.Clamp(currBattery, 0f, maxBattery);
                        DBManager.I.currData.currBattery = currBattery;
                        hUDBinder.RefreshBattery();
                    }
                }
            }
        }
    }
    #endregion
    [HideInInspector] public Vector2 startPosition;
    void FixBugPosition()
    {
        if (transform.position.y > -24.5) return;
        Debug.Log("BugPosition");
        transform.position = startPosition;
    }
    #region FootStep
    AudioSource sfxFootStep;
    public void PlayFootStep()
    {
        sfxFootStep.Play();
    }
    public void StopFootStep()
    {
        sfxFootStep.Pause();
    }
    #endregion

    [Space(40)]
    [Header("랜턴 다는 양, 차는 양 조절")]
    [SerializeField] float lanternDecreaseTick = -2.5f;
    [SerializeField] float lanternParryAmount = 31f;
    [SerializeField] float lanternAttackAmount = 3f;

    private void OnParrySuccess(HitData hData)
    {
        Vector3 position = hData.hitPoint;
        position += 0.5f * childTR.right;
        if (parry.lastSuccesCount % 2 == 1)
        {
            position += 0.58f * Vector3.up;
        }
        else
        {
            position -= 0.3f * Vector3.up;
            position += 0.11f * childTR.right;
        }
        Vector3 rndInCircle = Random.insideUnitSphere;
        rndInCircle.z = 0;
        position += 0.27f * rndInCircle;
        if (!string.IsNullOrEmpty(parryParticle))
        {
            ParticleManager.I.PlayParticle(parryParticle, position, Quaternion.identity, null);
        }
        StartOrExtendHitStop(parryHitStopDuration, parryHitStopTimeScale);
    }


    private void StartOrExtendHitStop(float duration, float timeScale)
    {
        float end = Time.realtimeSinceStartup + duration;
        if (end > _hitStopEndRealtime)
            _hitStopEndRealtime = end;

        if (_parryHitStopCo != null) return;

        _parryHitStopCo = StartCoroutine(HitStopRoutine(timeScale));
    }

    private IEnumerator HitStopRoutine(float timeScale)
    {
        Time.timeScale = Mathf.Clamp01(timeScale);
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;

        while (Time.realtimeSinceStartup < _hitStopEndRealtime)
            yield return null;



        float recoveryDuration = 0.25f;
        float elapsed = 0f;
        while (elapsed < recoveryDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / recoveryDuration);
            Time.timeScale = Mathf.Lerp(timeScale, baseTimeScale, t * t);
            Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
            yield return null;
        }
        Time.timeScale = baseTimeScale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;


        _parryHitStopCo = null;
    }






}
