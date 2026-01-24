
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
        // 현재 및 저장 데이터 갱신
        DBManager.I.currData.sceneName = "Stage5"; // 필요 시 변수화 가능
        DBManager.I.savedData.sceneName = "Stage5";
        
        DBManager.I.currData.lastPos = lastPos;
        DBManager.I.savedData.lastPos = lastPos;
        
        DBManager.I.Save();
    }
}