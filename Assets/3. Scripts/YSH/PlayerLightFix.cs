using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;

public class PlayerLightFix : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Light2D freeformLight; 
    private Light2D globalLight;
    private PlayerControl playerCtrl;

    [Header("Settings")]
    public float currentRadius; 
    public LayerMask layerMask;
    public int polyCount = 127;

    [Header("Visual Stats")]
    public float offRadius = 5.0f;       // 랜턴 OFF (할로우 나이트 스타일)
    public float onRadius = 20.0f;       // 랜턴 ON (넓은 시야)
    public float offGlobalInten = 0.05f; // 랜턴 OFF 시 배경 어둡게
    public float onGlobalInten = 0.5f;   // 랜턴 ON 시 배경 밝게
    public float lerpSpeed = 4f;

    [HideInInspector] public CancellationTokenSource cts;

    void Awake()
    {
        // 컴포넌트 자동 할당
        if (freeformLight == null) TryGetComponent(out freeformLight);
        playerCtrl = GetComponentInParent<PlayerControl>();
        
        // 씬에서 글로벌 라이트 찾기
        GameObject gLightObj = GameObject.Find("Global Light 2D");
        if (gLightObj != null) globalLight = gLightObj.GetComponent<Light2D>();
    }

    void OnEnable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        
        if (freeformLight != null)
        {
            freeformLight.gameObject.SetActive(true);
            StartDeform(cts.Token).Forget();
        }
    }

    void Update()
    {
        if (GameManager.I == null) return;

        // 1. 랜턴 ON/OFF 상태 및 배터리 유무에 따른 목표 수치 설정
        // 배터리가 0이면 강제로 OFF 수치를 추종하도록 설계
        bool isEffectivelyOn = GameManager.I.isLanternOn && playerCtrl.currBattery > 0;
        
        float targetR = isEffectivelyOn ? onRadius : offRadius;
        float targetG = isEffectivelyOn ? onGlobalInten : offGlobalInten;

        // 2. 부드러운 값 보간 (Lerp)
        currentRadius = Mathf.Lerp(currentRadius, targetR, Time.deltaTime * lerpSpeed);
        if (globalLight != null)
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetG, Time.deltaTime * lerpSpeed);
    }

    void OnDisable() => cts?.Cancel();
    void OnDestroy() => UniTaskCancel();

    void UniTaskCancel()
    {
        cts?.Cancel();
        try { cts?.Dispose(); } catch { }
        cts = null;
    }

    async UniTask StartDeform(CancellationToken token)
    {
        Vector3[] buffer = new Vector3[polyCount];
        RaycastHit2D hit;

        while (true)
        {
            await UniTask.Yield(token);
            float segmentAngle = 360f / polyCount;
            Vector2 myPos = (Vector2)transform.position;

            for (int i = 0; i < polyCount; i++)
            {
                // 성능 최적화: 20개 레이마다 한 프레임 대기
                if (i % 20 == 0) await UniTask.Yield(token); 

                Vector3 dir3D = Quaternion.Euler(0f, 0f, i * segmentAngle) * Vector3.up;
                Vector2 dir = (Vector2)dir3D;

                if (hit = Physics2D.Raycast(myPos, dir, currentRadius, layerMask))
                {
                    buffer[i] = Vector3.Slerp(buffer[i], (Vector3)(hit.point - myPos + dir * 0.1f), 50f * Time.deltaTime);
                }
                else
                {
                    buffer[i] = Vector3.Slerp(buffer[i], (Vector3)(dir * currentRadius), 50f * Time.deltaTime);
                }
            }
            freeformLight.SetShapePath(buffer);
        }
    }
}