using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerInteraction : MonoBehaviour
{
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        Init();
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
    void UniTaskCancel()
    {
        UnInit();
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    [SerializeField] float interactDistance = 1.3f;
    [SerializeField] LayerMask interactLayer;
    Collider2D[] colliders = new Collider2D[50];
    List<SensorData> sensorDatas = new List<SensorData>();
    [SerializeField] InputActionAsset inputActionAsset;
    private InputAction interactionAction;
    private InputAction lanternAction;
    struct SensorData
    {
        public Collider2D collider;
        public Interactable interactable;
        public Lanternable lanternable;
    }
    Transform camTR;
    PlayerControl playerControl;
    [ReadOnlyInspector][SerializeField] Interactable target1;
    [ReadOnlyInspector][SerializeField] Lanternable target2;
    Vector3 distancePivot;
    PromptControl prompt;
    PlayerLight PlayerLight;
    HUDBinder hUDBinder;
    void Awake()
    {
        prompt = FindAnyObjectByType<PromptControl>();
        hUDBinder = FindAnyObjectByType<HUDBinder>();
        PlayerLight = FindAnyObjectByType<PlayerLight>();
    }
    void UnInit()
    {
        target1 = null;
        target2 = null;
        interactionAction.performed -= InputInteract;
        interactionAction.canceled -= CancelInteract;
        lanternAction.performed -= InputLanternInteract;
        lanternAction.canceled -= CancelLanternInteract;
    }
    void Init()
    {
        interactionAction = inputActionAsset.FindActionMap("Player").FindAction("Interaction");
        lanternAction = inputActionAsset.FindActionMap("Player").FindAction("LanternInteraction");
        camTR = FindAnyObjectByType<FollowCamera>(FindObjectsInactive.Include).transform.GetChild(0);
        TryGetComponent(out playerControl);
        Sensor(cts.Token).Forget();
        target1 = null;
        target2 = null;
        interactionAction.performed += InputInteract;
        interactionAction.canceled += CancelInteract;
        lanternAction.performed += InputLanternInteract;
        lanternAction.canceled += CancelLanternInteract;
        flag1 = false;

        // 초기화 로직
        lanternFreeform = playerControl.transform.Find("PlayerLight/Lantern/FreeformLight").GetComponent<Light2D>();
        float s = 0.12f; // 두께 설정
        float val = 0f;  // 초기 길이는 0
        Vector3[] path = new Vector3[4];
        path[0] = new Vector3(0, s, 0);
        path[1] = new Vector3(0, -s, 0);
        path[2] = new Vector3(val, -s, 0);
        path[3] = new Vector3(val, s, 0);

    }
    public bool press1;
    public bool press2;
    void InputInteract(InputAction.CallbackContext callback)
    {
        if (playerControl.fsm.currentState == playerControl.openInventory) return;
        if (playerControl.fsm.currentState == playerControl.die) return;
        if (!press1)
        {
            if (target1 != null)
            {
                if (target1.type != Interactable.Type.DropItem && !playerControl.Grounded) return;
                if (target1.type != Interactable.Type.DropItem)
                {
                    Vector2 targetCenter = target1.transform.position + 1.3f * Vector3.up;
                    SpriteRenderer spriteRenderer = target1.GetComponent<SpriteRenderer>();
                    Collider2D collider2D = target1.GetComponent<Collider2D>();
                    if (spriteRenderer != null)
                    {
                        targetCenter = spriteRenderer.bounds.center;
                    }
                    else if (collider2D != null)
                    {
                        targetCenter = collider2D.bounds.center;
                    }
                    else if (target1.transform.childCount > 1)
                    {
                        spriteRenderer = target1.transform.GetChild(0).GetComponent<SpriteRenderer>();
                        collider2D = target1.transform.GetChild(0).GetComponent<Collider2D>();
                        if (spriteRenderer != null)
                        {
                            targetCenter = spriteRenderer.bounds.center;
                        }
                        else if (collider2D != null)
                        {
                            targetCenter = collider2D.bounds.center;
                        }
                        else
                        {
                            targetCenter = target1.transform.position + Vector3.up;
                        }
                    }
                    else
                    {
                        targetCenter = target1.transform.position + Vector3.up;
                    }
                    RaycastHit2D[] raycastHits = Physics2D.LinecastAll
                    (
                        (Vector2)playerControl.transform.position + 1.4f * Vector2.up,
                        (Vector2)targetCenter,
                        playerControl.groundLayer
                    );
                    bool isBlocked = false;
                    for (int i = 0; i < raycastHits.Length; i++)
                    {
                        if (raycastHits[i].collider.isTrigger) continue;
                        if (raycastHits[i].collider.transform.Root() == target1.transform.Root()) continue;
                        isBlocked = true;
                        break;
                    }
                    if (isBlocked)
                    {
                        return;
                    }
                }
                if (flag1) return;
                flag1 = true;
                AudioManager.I.PlaySFX("UIClick2");
                prompt.ClickEffect(0);
                Run(target1, cts.Token).Forget();
            }
        }
        press1 = true;
    }
    bool flag1;
    async UniTask Run(Interactable iobj, CancellationToken token)
    {
        await UniTask.Delay(110, cancellationToken: token);
        prompt.Close(0);
        await UniTask.Delay(110, cancellationToken: token);
        iobj.Run();
        flag1 = false;
    }
    void CancelInteract(InputAction.CallbackContext callback)
    {
        press1 = false;
    }
    void InputLanternInteract(InputAction.CallbackContext callback)
    {
        if (playerControl.fsm.currentState == playerControl.openInventory) return;
        if (playerControl.fsm.currentState == playerControl.die) return;
        if (playerControl.fsm.currentState != playerControl.idle) return;
        if (!playerControl.Grounded) return;
        if (target2 == null) return;
        if (!press2)
        {
            ctsLanternInteraction?.Cancel();
            ctsLanternInteraction = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternInteraction.Token);
            LanternInteraction(ctsLink.Token).Forget();
            target2.PromptFill();
            ctsLanternAnimationStart?.Cancel();
            ctsLanternAnimationExit?.Cancel();
            ctsLanternAnimationStart = new CancellationTokenSource();
            ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationStart.Token);
            LanternAnimationStart(ctsLink.Token).Forget();
        }
        press2 = true;
    }
    void CancelLanternInteract(InputAction.CallbackContext callback)
    {
        press2 = false;
        ctsLanternInteraction?.Cancel();
        DOTween.Kill(prompt.lanternCanvas.transform.Find("Wrap/PressFill"));
        DOTween.Kill(prompt.lanternCanvas.transform.Find("Wrap/PressRing"));
        target2?.PromptCancel();
        ctsLanternAnimationStart?.Cancel();
        ctsLanternAnimationExit?.Cancel();
        ctsLanternAnimationExit = new CancellationTokenSource();
        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
        LanternAnimationExit(ctsLink.Token).Forget();
        sfxLanternInteraction?.Despawn();
        sfxLanternInteraction = null;
        target2 = null;
    }
    CancellationTokenSource ctsLanternAnimationStart;
    CancellationTokenSource ctsLanternAnimationExit;
    Transform playerLightTr;

    private Transform lanternGroup;     // playerLightTr/Lantern
    private SpriteRenderer lanternSprite;
    private LineRenderer lanternLine;
    private Light2D lanternFreeform;    // 사각형 프리폼 라이트

    // 1. 애니메이션 시작 (Start)
    async UniTask LanternAnimationStart(CancellationToken token)
    {
        if (playerLightTr == null)
            playerLightTr = playerControl.transform.Find("PlayerLight");

        // 컴포넌트 캐싱
        if (lanternGroup == null)
        {
            lanternGroup = playerLightTr.Find("Lantern");
            lanternSprite = lanternGroup.GetChild(0).GetComponent<SpriteRenderer>();
            lanternLine = lanternGroup.GetComponent<LineRenderer>();
            lanternFreeform = lanternGroup.Find("FreeformLight").GetComponent<Light2D>();
        }
        // 모든 진행 중인 트윈 중지
        DOTween.Kill(playerLightTr);
        DOTween.Kill(lanternGroup);
        DOTween.Kill(lanternSprite);

        lanternGroup.gameObject.SetActive(true);
        // [초기화] 랜턴 중심을 둘러싸는 작은 정사각형 (길이 0)
        Vector3[] initPath = new Vector3[4];
        float s = 0.12f; // 두께 설정
        initPath[0] = new Vector3(0, s, 0);   // 좌상
        initPath[1] = new Vector3(0, -s, 0);  // 좌하
        initPath[2] = new Vector3(0.01f, -s, 0); // 우하 (미세한 간격)
        initPath[3] = new Vector3(0.01f, s, 0);  // 우상
        lanternFreeform.SetShapePath(initPath);

        float duration = 0.13f;
        Vector3 targetBasePos = target2.transform.Find("LightPoint").position;
        Vector3 midPoint = Vector3.Lerp(playerControl.transform.position, targetBasePos, 0.3f);
        Vector3 floatPosWorld = midPoint + Vector3.up * 1.2f;
        for (int j = 0; j < 10; j++)
        {
            if (Vector3.Distance(floatPosWorld, targetBasePos) <= 0.8f)
            {
                //Debug.Log("물체랑 너무 가까워서 뒤로 조금 밈");
                Vector3 outerDir = playerControl.transform.position - targetBasePos;
                outerDir.y = 0f;
                outerDir.Normalize();
                floatPosWorld += 2f * outerDir;
            }
            if (Vector3.Distance(floatPosWorld, playerControl.transform.position + Vector3.up) <= 0.9f)
            {
                //Debug.Log("플레이어랑 너무 가까워서 뒤로 조금 밈");
                Vector3 outerDir = floatPosWorld - targetBasePos;
                outerDir.y *= 0.2f;
                outerDir.Normalize();
                outerDir = outerDir - playerControl.childTR.right;
                outerDir.Normalize();
                floatPosWorld += 1f * outerDir;
            }
            //Debug.Log("랜턴 위치가 어떤 콜라이더의 속이라면 위치 재조정");
            Collider2D[] colliders = Physics2D.OverlapCircleAll(floatPosWorld, 0.2f, playerControl.groundLayer);
            bool isBlocked = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].isTrigger) continue;
                isBlocked = true;
                break;
            }
            if (!isBlocked)
            {
                // 최종 랜턴 위치 확정
                break;
            }
            // [4] 랜덤하게 재조정 (등 뒤 탐색)
            float playerYRot = playerControl.childTR.localRotation.eulerAngles.y;
            float backDirX = (Mathf.Abs(playerYRot - 0f) < 1f) ? -1f : 1f;
            float searchRange = 1f + (j * 0.2f);
            Vector3 randomOffset = new Vector3(
                Random.Range(0.5f, searchRange) * backDirX,
                Random.Range(-0.5f, 0.5f),
                0f
            );
            floatPosWorld += randomOffset;
        }

        Vector3 floatPosLocal = playerLightTr.parent.InverseTransformPoint(floatPosWorld);
        await UniTask.Delay(10, cancellationToken: token);
        sfxLanternInteraction = AudioManager.I.PlaySFX("ElectricityUsing");

        // 단계 1: 부상 및 활성화
        playerLightTr.DOLocalMove(floatPosLocal, duration).SetEase(Ease.OutCubic);
        lanternSprite.DOFade(1f, duration).SetLink(gameObject);
        DOTween.To(() => lanternFreeform.intensity, x => lanternFreeform.intensity = x, 3f, duration);

        await UniTask.Delay((int)(duration * 0.5f * 1000), cancellationToken: token);

        // 단계 2: 회전 조준
        Quaternion targetRot = Quaternion.FromToRotation(Vector3.right, (targetBasePos - floatPosWorld).normalized);
        lanternGroup.DORotateQuaternion(targetRot, duration * 0.5f).SetEase(Ease.OutBack);

        await UniTask.Delay((int)(duration * 0.4f * 1000), cancellationToken: token);

        // 단계 3: 빛 발사 (WaitUntil 방식)
        // 기준점을 lanternGroup(실제 랜턴 위치)으로 변경하여 오차 제거
        float distance = Vector3.Distance(lanternGroup.position, targetBasePos);
        s = 0.12f;
        bool isTweenFinished = false;

        // 1. 라인 렌더러 끝점 설정 (X축 로컬 좌표로 늘림)
        // 시작점(0,0,0)에서 끝점(distance,0,0)까지
        DOTween.To(() => lanternLine.GetPosition(1), x => lanternLine.SetPosition(1, x), new Vector3(distance, 0, 0), 0.06f);

        // 2. 프리폼 라이트 노드 설정
        DOTween.To(() => 0f, (val) =>
        {
            Vector3[] path = new Vector3[4];
            path[0] = new Vector3(0, s, 0);
            path[1] = new Vector3(0, -s, 0);
            path[2] = new Vector3(val, -s, 0); // val이 distance까지 도달함
            path[3] = new Vector3(val, s, 0);
            lanternFreeform.SetShapePath(path);
        }, distance, 0.06f) // 목표값을 정확히 위에서 계산한 distance로 설정
        .SetEase(Ease.OutCubic)
        .OnComplete(() => isTweenFinished = true);

        await UniTask.WaitUntil(() => isTweenFinished, cancellationToken: token);

    }

    // 2. 애니메이션 종료 (Exit)
    async UniTask LanternAnimationExit(CancellationToken token)
    {

        if (playerLightTr == null)
            playerLightTr = playerControl.transform.Find("PlayerLight");

        // 컴포넌트 캐싱
        if (lanternGroup == null)
        {
            lanternGroup = playerLightTr.Find("Lantern");
            lanternSprite = lanternGroup.GetChild(0).GetComponent<SpriteRenderer>();
            lanternLine = lanternGroup.GetComponent<LineRenderer>();
            lanternFreeform = lanternGroup.Find("FreeformLight").GetComponent<Light2D>();
        }
        // 모든 진행 중인 트윈 중지
        DOTween.Kill(playerLightTr);
        DOTween.Kill(lanternGroup);
        DOTween.Kill(lanternSprite);

        float exitDuration = 0.5f;
        lanternSprite.DOFade(0f, exitDuration).SetLink(gameObject);
        DOTween.To(() => lanternFreeform.intensity, x => lanternFreeform.intensity = x, 0f, exitDuration);

        // 라인 및 프리폼 즉시 초기화 (필요시 트윈으로 변경 가능)
        lanternLine.SetPosition(1, Vector3.zero);
        lanternFreeform.SetShapePath(new Vector3[4]);

        bool isExitFinished = false;
        lanternGroup.DOLocalRotateQuaternion(Quaternion.identity, exitDuration);
        playerLightTr.DOLocalMove(new Vector3(0f, 0.5f, 0f), exitDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => { isExitFinished = true; lanternGroup.gameObject.SetActive(false); });

        await UniTask.WaitUntil(() => isExitFinished, cancellationToken: token);
    }
    CancellationTokenSource ctsLanternInteraction;
    SFX sfxLanternInteraction;
    async UniTask LanternInteraction(CancellationToken token)
    {
        Transform tr1 = prompt.lanternCanvas.transform.Find("Wrap/PressFill");
        Transform tr2 = prompt.lanternCanvas.transform.Find("Wrap/PressRing");
        DOTween.Kill(tr1);
        DOTween.Kill(tr2);
        tr1.gameObject.SetActive(true);
        tr2.gameObject.SetActive(true);
        tr1.DOScale(1f, 2f).SetLink(gameObject);
        tr2.DOScale(1f, 2f).SetLink(gameObject);
        bool isCancel = false;
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            if (playerControl.fsm.currentState == playerControl.openInventory)
            {
                isCancel = true;
                break;
            }
            if (playerControl.fsm.currentState == playerControl.die)
            {
                isCancel = true;
                break;
            }
            if (target2 == null)
            {
                isCancel = true;
                break;
            }
            if (playerControl.fsm.currentState != playerControl.idle)
            {
                isCancel = true;
                break;
            }
            target2.promptFill += 0.3f * target2.fillSpeed * Time.deltaTime;
            target2.promptFill = Mathf.Clamp01(target2.promptFill);
            prompt.lanternFill.fillAmount = target2.promptFill;
            if (target2.promptFill == 1f) break;
        }
        if (isCancel)
        {
            prompt.Close(1);
            target2?.PromptCancel();
            ctsLanternAnimationStart?.Cancel();
            ctsLanternAnimationExit?.Cancel();
            ctsLanternAnimationExit = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
            LanternAnimationExit(ctsLink.Token).Forget();
            sfxLanternInteraction?.Despawn();
            sfxLanternInteraction = null;
            target2 = null;
        }
        else if (target2 != null)
        {
            target2.Run();
            prompt.Close(1);
            target2?.PromptCancel();
            ctsLanternAnimationStart?.Cancel();
            ctsLanternAnimationExit?.Cancel();
            ctsLanternAnimationExit = new CancellationTokenSource();
            var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
            LanternAnimationExit(ctsLink.Token).Forget();
            sfxLanternInteraction?.Despawn();
            sfxLanternInteraction = null;
            target2 = null;
        }
    }
    async UniTask Sensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        GameObject fLight = PlayerLight.transform.GetChild(1).gameObject;
        while (!token.IsCancellationRequested)
        {
            int rnd = Random.Range(12, 18);
            await UniTask.DelayFrame(rnd, cancellationToken: token);
            if (playerControl.fsm.currentState == playerControl.openInventory)
            {
                prompt.Close(0);
                target1 = null;
                prompt.Close(1);
                target2?.PromptCancel();
                ctsLanternAnimationStart?.Cancel();
                ctsLanternAnimationExit?.Cancel();
                ctsLanternAnimationExit = new CancellationTokenSource();
                var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                LanternAnimationExit(ctsLink.Token).Forget();
                sfxLanternInteraction?.Despawn();
                sfxLanternInteraction = null;
                target2 = null;
                continue;
            }
            if (playerControl.fsm.currentState == playerControl.die)
            {
                prompt.Close(0);
                target1 = null;
                prompt.Close(1);
                target2?.PromptCancel();
                ctsLanternAnimationStart?.Cancel();
                ctsLanternAnimationExit?.Cancel();
                ctsLanternAnimationExit = new CancellationTokenSource();
                var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                LanternAnimationExit(ctsLink.Token).Forget();
                sfxLanternInteraction?.Despawn();
                sfxLanternInteraction = null;
                target2 = null;
                continue;
            }
            distancePivot = transform.position + (0.4f * playerControl.height * Vector3.up) + (0.4f * interactDistance * (camTR.position - transform.position).normalized);
            colliders = Physics2D.OverlapCircleAll(transform.position, interactDistance, interactLayer);
            sensorDatas.Clear();
            // Auto Interaction
            playerControl.isNearSavePoint = false;
            playerControl.isNearSconceLight = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                int find = sensorDatas.FindIndex(x => x.collider == colliders[i]);
                if (find != -1) continue;
                Transform root = colliders[i].transform.Root();
                if (root.TryGetComponent(out Interactable interactable))
                {
                    SavePoint_LSH savePoint_LSH = interactable as SavePoint_LSH;
                    if (savePoint_LSH != null)
                    {
                        playerControl.isNearSavePoint = true;
                    }
                    if (!interactable.isReady) continue;
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.interactable = interactable;
                    sensorDatas.Add(data);
                    if (interactable.isAuto)
                    {
                        interactable.Run();
                        hUDBinder.Refresh();
                    }
                }
                else if (root.TryGetComponent(out Lanternable lanternable))
                {
                    SconceLight sconceLight = lanternable as SconceLight;
                    if (sconceLight != null && !sconceLight.isReady)
                    {
                        playerControl.isNearSconceLight = true;
                    }
                    if (!lanternable.isReady) continue;
                    SensorData data = new SensorData();
                    data.collider = colliders[i];
                    data.lanternable = lanternable;
                    sensorDatas.Add(data);
                    if (lanternable.isAuto)
                    {
                        lanternable.Run();
                        hUDBinder.Refresh();
                    }
                }
            }
            //
            sensorDatas.Sort((x, y) =>
            {
                int px = x.interactable != null ? x.interactable.Priority : 9999;
                int py = y.interactable != null ? y.interactable.Priority : 9999;

                // 1) 우선순위 먼저
                int pComp = px.CompareTo(py);
                if (pComp != 0) return pComp;

                // 2) 우선순위 같으면 거리
                float dx = Vector3.SqrMagnitude(x.collider.transform.position - distancePivot);
                float dy = Vector3.SqrMagnitude(y.collider.transform.position - distancePivot);
                return dx.CompareTo(dy);
            });

            if (sensorDatas.Count > 0)
            {
                // Sensor --> Interactable Prompt Open / Close
                int find = -1;
                for (int k = 0; k < sensorDatas.Count; k++)
                {
                    var _interactable = sensorDatas[k].interactable;
                    if (_interactable == null) continue;
                    if (!_interactable.isAuto)
                    {
                        find = k;
                        break;
                    }
                }
                if (find >= 0)
                {
                    if (target1 != sensorDatas[find].interactable)
                    {
                        target1 = sensorDatas[find].interactable;
                        prompt.OpenType1(target1);
                    }
                }
                else if (target1 != null)
                {
                    target1 = null;
                    prompt.Close(0);
                }

                // Sensor --> Lanternable Prompt Open / Close
                find = -1;
                if (GameManager.I.isLanternOn)
                {
                    for (int k = 0; k < sensorDatas.Count; k++)
                    {
                        var _lanternable = sensorDatas[k].lanternable;
                        if (_lanternable == null) continue;
                        if (!_lanternable.isAuto)
                        {
                            find = k;
                            break;
                        }
                    }
                    if (find >= 0)
                    {
                        if (target2 != sensorDatas[find].lanternable)
                        {
                            target2 = sensorDatas[find].lanternable;
                            prompt.OpenType2(target2);
                        }
                    }
                    else if (target2 != null)
                    {
                        prompt.Close(1);
                        target2?.PromptCancel();
                        ctsLanternAnimationStart?.Cancel();
                        ctsLanternAnimationExit?.Cancel();
                        ctsLanternAnimationExit = new CancellationTokenSource();
                        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                        LanternAnimationExit(ctsLink.Token).Forget();
                        sfxLanternInteraction?.Despawn();
                        sfxLanternInteraction = null;
                        target2 = null;
                    }
                }
                else
                {
                    if (target2 != null)
                    {
                        prompt.Close(1);
                        target2?.PromptCancel();
                        ctsLanternAnimationStart?.Cancel();
                        ctsLanternAnimationExit?.Cancel();
                        ctsLanternAnimationExit = new CancellationTokenSource();
                        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                        LanternAnimationExit(ctsLink.Token).Forget();
                        sfxLanternInteraction?.Despawn();
                        sfxLanternInteraction = null;
                        target2 = null;
                    }
                    if (prompt.lanternCanvas.gameObject.activeInHierarchy)
                    {
                        prompt.Close(1, true);
                        target2?.PromptCancel();
                        ctsLanternAnimationStart?.Cancel();
                        ctsLanternAnimationExit?.Cancel();
                        ctsLanternAnimationExit = new CancellationTokenSource();
                        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                        LanternAnimationExit(ctsLink.Token).Forget();
                        sfxLanternInteraction?.Despawn();
                        sfxLanternInteraction = null;
                        target2 = null;
                    }
                }
            }
            else if (sensorDatas.Count == 0)
            {
                if (target1 != null)
                {
                    prompt.Close(0);
                    target1 = null;
                }
                if (target2 != null)
                {
                    prompt.Close(1);
                    sfxLanternInteraction?.Despawn();
                    sfxLanternInteraction = null;
                    if (target2 != null)
                    {
                        target2?.PromptCancel();
                        ctsLanternAnimationStart?.Cancel();
                        ctsLanternAnimationExit?.Cancel();
                        ctsLanternAnimationExit = new CancellationTokenSource();
                        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                        LanternAnimationExit(ctsLink.Token).Forget();
                        target2 = null;
                    }
                }
                if (prompt.itrctCanvas.gameObject.activeInHierarchy)
                {
                    prompt.Close(0, true);
                    target1 = null;
                }
                if (prompt.lanternCanvas.gameObject.activeInHierarchy)
                {
                    prompt.Close(1);
                    sfxLanternInteraction?.Despawn();
                    sfxLanternInteraction = null;
                    if (target2 != null)
                    {
                        target2?.PromptCancel();
                        ctsLanternAnimationStart?.Cancel();
                        ctsLanternAnimationExit?.Cancel();
                        ctsLanternAnimationExit = new CancellationTokenSource();
                        var ctsLink = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ctsLanternAnimationExit.Token);
                        LanternAnimationExit(ctsLink.Token).Forget();
                        target2 = null;
                    }
                }
            }
        }
    }
}
