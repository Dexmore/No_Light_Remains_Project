
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using System.Threading.Tasks;

public class DialogueAndScenePortal : Interactable
{
    #region Interactable Settings
    public override Type type => Type.Normal;
    public override bool isReady { get; set; } = true;
    public override bool isAuto => false;
    #endregion

    [Header("1. 다이얼로그 설정")]
    [Tooltip("출력할 대사 번호 (DialogUI 기준)")]
    public int dialogIndex;
    public string sfxName;

    [Header("2. 씬 이동 설정")]
    public string sceneName;
    [Tooltip("씬 이동 전 저장할 위치 (Stage5 기준 예시)")]
    public Vector2 lastPos = new Vector2(-18, 2.05f);

    private Collider2D coll2D;

    void Awake()
    {
        isReady = true;
        TryGetComponent(out coll2D);
    }

    public override void Run()
    {
        // UI가 이미 열려있으면 중복 실행 방지
        if (GameManager.I.isOpenDialog || GameManager.I.isOpenPop || GameManager.I.isOpenInventory) return;

        isReady = false;
        if (coll2D != null) coll2D.enabled = false;

        // 다이얼로그 시작
        GameManager.I.onDialog.Invoke(dialogIndex, transform);

        if (!string.IsNullOrEmpty(sfxName))
        {
            AudioManager.I.PlaySFX(sfxName, transform.position, null, 0.2f);
        }

        // 대사 종료를 기다리는 코루틴 시작
        StartCoroutine(nameof(WaitDialogAndMoveScene));
    }

    IEnumerator WaitDialogAndMoveScene()
    {
        // 다이얼로그가 완전히 켜질 때까지 잠시 대기
        yield return YieldInstructionCache.WaitForSeconds(0.37f);

        // 모든 창이 닫힐 때까지 대기 (플레이어가 대사를 다 읽고 닫을 때까지)
        yield return new WaitUntil(() => !GameManager.I.isOpenDialog && !GameManager.I.isOpenPop && !GameManager.I.isOpenInventory);

        // 대사가 끝난 후의 여운을 위한 짧은 대기
        yield return YieldInstructionCache.WaitForSeconds(0.5f);

        // 씬 이동 전 데이터 저장 (EndingCreditPotal의 로직 참고)
        SaveBeforeLoading();

        // 씬 로드 시작
        GameManager.I.LoadSceneAsync(sceneName);
    }

    private void SaveBeforeLoading()
    {

        // 현재 난이도(difficulty)를 비트 위치로 사용하여 해당 비트를 1로 켭니다.
        // 1 << 0 = 1 (001)
        // 1 << 1 = 2 (010)
        // 1 << 2 = 4 (100)

        if (DBManager.I.IsSteam() && DBManager.I.IsSteamInit())
        {
            DBManager.I.allSaveDatasInLocal.ach10bitMask = DBManager.I.allSaveDatasInSteam.ach10bitMask;
            DBManager.I.allSaveDatasInLocal.ach10bitMask |= (1 << DBManager.I.currData.difficulty);
            if (DBManager.I.allSaveDatasInLocal.ach10bitMask == 7) // 111(2) = 7
            {
                DBManager.I.SteamAchievement("ACH_ALL_CLEAR");
                DBManager.I.allSaveDatasInSteam.ach10bitMask = 7;
            }
        }
        
        System.DateTime deathTime = System.DateTimeOffset.FromUnixTimeSeconds(DBManager.I.currData.ach15time).DateTime;
        System.TimeSpan timePassed = System.DateTime.UtcNow - deathTime;
        if (timePassed.TotalSeconds <= 3600)
        {
            switch (DBManager.I.currData.difficulty)
            {
                case 0:
                    DBManager.I.SteamAchievement("ACH_ONEHOUR_STORY");
                    break;
                case 1:
                    DBManager.I.SteamAchievement("ACH_ONEHOUR_NORMAL");
                    break;
                case 2:
                    DBManager.I.SteamAchievement("ACH_ONEHOUR_HARD");
                    break;
            }
        }
        if (DBManager.I.currData.ach14count == 4)
        {
            DBManager.I.SteamAchievement("ACH_ALL_MONSTERKILL");
        }

        // 현재 및 저장 데이터 갱신
        DBManager.I.currData.sceneName = "Stage5"; // 필요 시 변수화 가능
        DBManager.I.savedData.sceneName = "Stage5";
        DBManager.I.currData.lastPos = lastPos;
        DBManager.I.savedData.lastPos = lastPos;
        DBManager.I.Save();




    }
}