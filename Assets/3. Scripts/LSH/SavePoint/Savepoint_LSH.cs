using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint_LSH : Interactable
{
    public override bool isReady { get; set; }
    public override bool isAuto => false;
    public override Type type => Type.Normal;
    float startTime = 0;
    public async override void Run()
    {
        isReady = false;
        if (Time.time - startTime < 1.5f) return;
        startTime = Time.time;
        Vector2 pos2D = transform.position;
        string sceneName = SceneManager.GetActiveScene().name;
        DBManager.I.currData.sceneName = sceneName;
        DBManager.I.currData.lastPos = pos2D;
        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        Vector3 dir = Vector3.zero;

        AudioManager.I.PlaySFX("Save");
        ParticleManager.I.PlayText("Save", transform.position + 1.2f * Vector3.up + 0.8f * dir, ParticleManager.TextType.PlayerNotice);
        HUDBinder hUDBinder = FindAnyObjectByType<HUDBinder>();
        DBManager.I.currData.cpc = DBManager.I.currData.mpc;
        GameManager.I.potionDebt = 0;
        if(playerControl)
        {
            playerControl.currHealth = DBManager.I.currData.maxHealth;
        }
        DBManager.I.currData.currHealth = DBManager.I.currData.maxHealth;
        hUDBinder?.Refresh(2f);
        _ = GameManager.I.SaveAllMonsterAndObject();
        System.DateTime now = System.DateTime.Now;
        string datePart = now.ToString("yyyy.MM.dd");
        int secondsOfDay = (int)now.TimeOfDay.TotalSeconds;
        DBManager.I.currData.lastTime = $"{datePart}-{secondsOfDay}";
        DBManager.I.Save();
        CaptureArea();
        Debug.Log($"[SavePoint] Saved at {pos2D} in scene {sceneName}");
        PlayActivateOnce();
        await Task.Delay(1200);
        isReady = true;
    }
    [Header("Components")]
    [SerializeField] private Animator animator;
    [Header("Animator Params")]
    [SerializeField] private string activateTriggerName = "Activate";
    [SerializeField] private string deactivateTriggerName = "Deactivate";



    void Awake()
    {
        isReady = true;
        if (!animator)
            animator = GetComponentInChildren<Animator>();
    }

    private void PlayActivateOnce() // 활성화될때 애니메이션 재생
    {
        if (!animator) return;

        animator.ResetTrigger(deactivateTriggerName);
        animator.SetTrigger(activateTriggerName);
    }
    public void SetInactive()
    {
        if (!animator) return;

        animator.ResetTrigger(activateTriggerName);
        animator.SetTrigger(deactivateTriggerName);
    }





    float areaWidth = 11.25f;
    float areaHeight = 6f;
    int pixelWidth = 300;
    int pixelHeight = 160;
    public void CaptureArea()
    {
        if (DBManager.I.currSlot == -1) return;
#if UNITY_STANDALONE_WIN
        string fileLocation = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "My Games", "REKINDLE");
#else
        string fileLocation = Path.Combine(Application.persistentDataPath, "REKINDLE_SaveData");
#endif
        Vector3 targetPoint = transform.position;
        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        if (playerControl)
            targetPoint = 0.6f * transform.position + 0.4f * playerControl.transform.position + Vector3.up;
        // 임시 카메라 생성 및 설정
        GameObject camGo = new GameObject("AreaCaptureCamera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = areaHeight / 2f;
        cam.aspect = areaWidth / areaHeight;
        camGo.transform.position = targetPoint + new Vector3(0, 0, -10f);
        camGo.transform.LookAt(targetPoint);
        RenderTexture rt = new RenderTexture(pixelWidth, pixelHeight, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGBA32, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, pixelWidth, pixelHeight), 0, 0);
        screenShot.Apply();
        string path = Path.Combine(fileLocation, $"{DBManager.I.currSlot}.png");
        File.WriteAllBytes(path, screenShot.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.active = null;
        if (Application.isEditor)
        {
            DestroyImmediate(rt);
            DestroyImmediate(camGo);
        }
        else
        {
            Destroy(rt);
            Destroy(camGo);
        }
    }


}
