using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour
{

    [Header("Player HP")]
    public float maxHealth = 1000;
    public float currentHealth;

    [Header("Light Resource")]
    public float maxLight = 100;
    public float currentLight;

    [Header("Move")]
    public float moveSpeed = 6f;
    public float airMoveMultiplier = 0.85f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Input (use bound actions)")]
    public InputActionAsset inputActionAsset;
    private InputAction lanternAction;

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
    [HideInInspector] public PlayerDash dash;
    [HideInInspector] public PlayerParry parry;
    [HideInInspector] public PlayerHit hit;
    [HideInInspector] public PlayerDie die;
    [HideInInspector] public PlayerUsePotion usePotion;
    [HideInInspector] public PlayerOpenInventory openInventory;
    // [HideInInspector] public PlayerJumpAttack jumpAttack;


    // === Ground 체크 ===
    [Header("Ground Sensor (정교 판정)")]
    [SerializeField] private LayerMask groundLayer;
    CapsuleCollider2D capsuleCollider2D;
    [HideInInspector] public float height;
    [HideInInspector] public float width;
    private readonly ContactPoint2D[] _contactPts = new ContactPoint2D[8];
    [HideInInspector] public Dictionary<Collider2D, Vector2> contactPts = new Dictionary<Collider2D, Vector2>();
    [HideInInspector] public Dictionary<Collider2D, Vector2> collisions = new Dictionary<Collider2D, Vector2>();

    // Runtime
    [ReadOnlyInspector] public bool Grounded { get; private set; }
    [ReadOnlyInspector] public bool Parred { get; set; }
    [ReadOnlyInspector] public bool Avoided { get; set; }
    [ReadOnlyInspector] public bool Dead { get; set; }
    void Awake()
    {
        TryGetComponent(out rb);
        childTR = transform.GetChild(0);
        animator = GetComponentInChildren<Animator>(true);
        capsuleCollider2D = GetComponentInChildren<CapsuleCollider2D>(true);
        attackRange = GetComponentInChildren<AttackRange>(true);
        height = capsuleCollider2D.size.y;
        width = capsuleCollider2D.size.x;
        lightSystem = GetComponentInChildren<LightSystem>(true);
        fsm = new PlayerStateMachine();
        idle = new PlayerIdle(this, fsm);
        run = new PlayerRun(this, fsm);
        jump = new PlayerJump(this, fsm);
        fall = new PlayerFall(this, fsm);
        attack = new PlayerAttack(this, fsm);
        attackCombo = new PlayerAttackCombo(this, fsm);
        parry = new PlayerParry(this, fsm);
        dash = new PlayerDash(this, fsm);
        hit = new PlayerHit(this, fsm);
        die = new PlayerDie(this, fsm);
        usePotion = new PlayerUsePotion(this, fsm);
        openInventory = new PlayerOpenInventory(this, fsm);
        InitMatInfo();
        sfxFootStep = GetComponentInChildren<AudioSource>();
    }
    void Start()
    {
        GameObject light0 = lightSystem.transform.GetChild(0).gameObject;
        GameObject light1 = lightSystem.transform.GetChild(1).gameObject;
        CharacterData characterData = DBManager.I.currentCharData;
        if (characterData.sceneName == "" && characterData.HP == 0)
        {
            CharacterData newData = new CharacterData();
            newData.money = 0;
            newData.sceneName = "Stage1";
            newData.lastPosition = Vector2.zero;
            newData.HP = maxHealth;
            newData.MP = maxLight;
            newData.potionCount = 5;
            newData.itemDatas = new List<CharacterData.ItemData>();
            newData.gearDatas = new List<CharacterData.GearData>();
            newData.lanternDatas = new List<CharacterData.LanternData>();
            DBManager.I.currentCharData = newData;
            light0.SetActive(false);
            light1.SetActive(false);
            DBManager.I.AddItem("UsefulSword", 1);
            DBManager.I.AddItem("Helmet", 1);
            DBManager.I.AddItem("LeatherArmor", 1);
        }
        else
        {
            currentHealth = DBManager.I.currentCharData.HP;
            currentLight = DBManager.I.currentCharData.MP;
            light0.SetActive(DBManager.I.isLanternOn);
            light1.SetActive(DBManager.I.isLanternOn);
        }
        inventoryUI = FindAnyObjectByType<Inventory>();
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
        GameManager.I.onHit -= HitHandler;
    }

    void Update()
    {
        fsm.Update();
        CheckGroundedPrecise();
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (!collisions.ContainsKey(collision.collider))
                collisions.Add(collision.collider, collision.contacts[0].point);
            else
                collisions[collision.collider] = collision.contacts[0].point;
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if ((collision.collider.gameObject.layer & groundLayer) != 0)
            if (collisions.ContainsKey(collision.collider))
                collisions.Remove(collision.collider);
    }
    Ray2D groundRay = new Ray2D();
    RaycastHit2D groundRayHit;
    float groundCheckTime;
    void CheckGroundedPrecise()
    {
        Grounded = false;
        if (collisions.Count > 0)
            foreach (var element in collisions)
                if (Mathf.Abs(element.Value.y - transform.position.y) < 0.1f * capsuleCollider2D.size.y)
                {
                    Grounded = true;
                    break;
                }
        if (!Grounded)
        {
            if (Time.time - groundCheckTime > Random.Range(0.1f, 0.3f))
            {
                groundCheckTime = Time.time;
                groundRay.origin = (Vector2)transform.position + 0.08f * Vector2.up;
                groundRay.direction = Vector2.down;
                groundRayHit = Physics2D.Raycast(groundRay.origin, groundRay.direction, 0.1f, groundLayer);
                if (groundRayHit)
                {
                    Grounded = true;
                }
            }
        }
    }

    #region Dash
    int leftDashInputCount = 0;
    int rightDashInputCount = 0;
    void DashInput(InputAction.CallbackContext callback)
    {
        if (!Grounded) return;
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
        if (!Grounded) return;
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
    [HideInInspector] public Inventory inventoryUI;
    void HitHandler(HitData hData)
    {
        if (hData.target.Root() != transform) return;
        if (fsm.currentState == die) return;
        if (hData.attackType == HitData.AttackType.Chafe)
        {
            if (isHit2) return;
            if (isHit1) return;
            isHit1 = true;
            run.isStagger = true;
            StopCoroutine(nameof(HitCoolTime1));
            StartCoroutine(nameof(HitCoolTime1));
            if (Avoided)
            {
                //Debug.Log("회피 성공");
                return;
            }
            Vector2 dir = 4.2f * (hData.target.position.x - hData.attacker.position.x) * Vector2.right;
            dir.y = 2f;
            Vector3 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            rb.AddForce(dir, ForceMode2D.Impulse);
            currentHealth -= (int)hData.damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            DBManager.I.currentCharData.HP = currentHealth;
            if (hData.particleNames != null)
            {
                int rnd = Random.Range(0, hData.particleNames.Length);
                string particleName = hData.particleNames[rnd];
                ParticleManager.I.PlayParticle(particleName, hData.hitPoint, Quaternion.identity, null);
            }
            AudioManager.I.PlaySFX("HitLittle", hData.hitPoint, null);
            if (currentHealth <= 0)
                fsm.ChangeState(die);
            HitChangeColor(Color.white, 0);
            if(fsm.currentState == openInventory)
            {
                fsm.ChangeState(idle);
            }
            return;
        }
        else if (hData.attackType == HitData.AttackType.Default)
        {
            if (isHit2) return;
            isHit2 = true;
            StopCoroutine(nameof(HitCoolTime2));
            StartCoroutine(nameof(HitCoolTime2));
            if (Avoided)
            {
                GameManager.I.onAvoid.Invoke(hData.attacker.Root());
                ParticleManager.I.PlayText("Miss", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                AudioManager.I.PlaySFX("Woosh1");
                return;
            }
            if (Parred)
            {
                if (!hData.isCannotParry)
                {
                    AudioManager.I.PlaySFX("Parry");
                    ParticleManager.I.PlayText("Parry", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                    GameManager.I.onParry.Invoke(hData.attacker.Root());
                    StartCoroutine(nameof(ReleaseParred));
                    return;
                }
                else
                {
                    AudioManager.I.PlaySFX("Fail1");
                    ParticleManager.I.PlayText("Cannot Parry", hData.hitPoint, ParticleManager.TextType.PlayerNotice);
                    //Debug.Log("패링 불가 공격");
                }
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
                    multiplier = 1.3f;
                    GameManager.I.HitEffect(hData.hitPoint, 0.65f);
                    break;
            }
            Vector2 dir = 2.8f * multiplier * (hData.target.position.x - hData.attacker.position.x) * Vector2.right;
            dir.y = 2.3f * Mathf.Sqrt(multiplier) + (multiplier - 1f);
            Vector3 velo = rb.linearVelocity;
            rb.linearVelocity = 0.4f * velo;
            rb.AddForce(dir, ForceMode2D.Impulse);
            if (hData.staggerType != HitData.StaggerType.None)
            {
                hit.staggerType = hData.staggerType;
                fsm.ChangeState(hit);
            }
            currentHealth -= (int)hData.damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            DBManager.I.currentCharData.HP = currentHealth;
            if (currentHealth <= 0)
                fsm.ChangeState(die);
            if (hData.particleNames != null)
            {
                int rnd = Random.Range(0, hData.particleNames.Length);
                string particleName = hData.particleNames[rnd];
                ParticleManager.I.PlayParticle(particleName, hData.hitPoint, Quaternion.identity, null);
            }
            AudioManager.I.PlaySFX("Hit8bit2", hData.hitPoint, null);
            HitChangeColor(Color.white, 1);
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
                float _dur = 0.21f;
                if (index == 1) _dur = 0.5f;
                Tween colorTween = newMats[materialIndex].DOVector(
                    new Vector4(color.r, color.g, color.b, 0f),
                    "_TintColor",
                    _dur
                ).SetEase(Ease.OutBounce);
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
        yield return YieldInstructionCache.WaitForSeconds(Random.Range(0.5f, 0.7f));
        isHit1 = false;
    }
    IEnumerator HitCoolTime2()
    {
        yield return YieldInstructionCache.WaitForSeconds(1.12f);
        isHit2 = false;
    }
    IEnumerator ReleaseParred()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.15f);
        Parred = false;
    }
    #region Turn ON/OFF Lantern
    LightSystem lightSystem;
    void LanternInput(InputAction.CallbackContext callback)
    {
        if (Dead) return;
        GameObject light0 = lightSystem.transform.GetChild(0).gameObject;
        GameObject light1 = lightSystem.transform.GetChild(1).gameObject;
        AudioManager.I.PlaySFX("FlashlightClick");
        if (light0.activeSelf)
        {
            light0.SetActive(false);
            light1.SetActive(false);
            DBManager.I.isLanternOn = false;
        }
        else
        {
            light0.SetActive(true);
            light1.SetActive(true);
            DBManager.I.isLanternOn = true;
        }
    }
    #endregion
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


}
