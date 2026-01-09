using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;          // [로컬라이징 필수]
using UnityEngine.Localization.Settings; // [로컬라이징 필수]

// [신규] 인스펙터에서 관리할 번역 데이터 클래스
[System.Serializable]
public class TutorialStringData
{
    [Header("튜토리얼 메시지")]
    public LocalizedString step1_Select;      // Tutorial_Step1_Select
    public LocalizedString step2_Verify;      // Tutorial_Step2_Verify
    public LocalizedString step3_Error;       // Tutorial_Step3_Error
    public LocalizedString step4_Supply;      // Tutorial_Step4_Supply
    public LocalizedString step5_Auth;        // Tutorial_Step5_Auth
    public LocalizedString msg_Complete;      // Tutorial_Complete
}

public class WorkbenchTutorialController : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private WorkbenchUI workbenchUI;
    [SerializeField] private TutorialOverlay tutorialOverlay;
    [SerializeField] private NotificationUI notificationUI;

    [Header("더미 데이터")]
    [SerializeField] private GearData tutorialGear; 
    [SerializeField] private ItemData tutorialMaterial; 

    [Header("로컬라이징 데이터")]
    [SerializeField] private TutorialStringData locStrings; // [신규] 여기에 키 연결

    private int _step = 0;

    private void Start()
    {
        // 1. 저장된 진행도 확인 (완료했으면 꺼짐)
        if (DBManager.I.GetProgress("Tutorial_Workbench") == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        // 2. 튜토리얼 시작
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // 1. UI 켜질 때까지 대기
        while (!workbenchUI.IsUIActive()) yield return null;
        
        // 2. 부팅 연출 대기
        yield return new WaitForSeconds(9.4f); 

        // 3. 튜토리얼 모드 진입
        List<GearData> dummies = new List<GearData> { tutorialGear };
        workbenchUI.BeginTutorialMode(dummies);

        workbenchUI.OnGearSelectedEvent += HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent += HandleEnhanceTry;

        // ====================================================
        // STEP 1: 기어 선택
        // ====================================================
        _step = 1;
        
        // [수정] 로컬라이징 적용 (fallback: 한국어)
        string msg1 = GetLocStr(locStrings.step1_Select, "시스템 보정: 강화할 기어 모듈을 선택하십시오.");
        ShowMessage(msg1);
        
        RectTransform slotRect = workbenchUI.GetSlotRect(0);
        tutorialOverlay.FocusOn(slotRect);
        tutorialOverlay.PlayFadeIn(0.5f); 

        while (_step == 1) yield return null; 


        // ====================================================
        // STEP 2: 강화 시도 (재료 없음)
        // ====================================================
        string msg2 = GetLocStr(locStrings.step2_Verify, "무결성 검증: 강화 프로세스를 실행하십시오.");
        ShowMessage(msg2);
        
        RectTransform btnRect = workbenchUI.GetEnhanceButtonRect();
        tutorialOverlay.FocusOn(btnRect);

        while (_step == 2) yield return null;


        // ====================================================
        // STEP 3: 재료 부족 확인 (3초 대기)
        // ====================================================
        string msg3 = GetLocStr(locStrings.step3_Error, "오류: 자원 부족. 필요한 자원을 확인하십시오.");
        ShowMessage(msg3);
        
        RectTransform costPanelRect = workbenchUI.GetCostPanelRect();
        if (costPanelRect == null) costPanelRect = workbenchUI.GetCostTextRect(0);
        tutorialOverlay.FocusOn(costPanelRect);

        yield return new WaitForSeconds(3.0f);


        // ====================================================
        // STEP 4: 재료 공급 및 재선택
        // ====================================================
        workbenchUI.AddTutorialDummyMaterial(10); 
        workbenchUI.ForceDeselect(); 
        
        string msg4 = GetLocStr(locStrings.step4_Supply, "자원 보급 완료. 기어를 다시 선택하여 초기화를 재개하십시오.");
        ShowMessage(msg4);
        
        tutorialOverlay.FocusOn(slotRect);

        _step = 4; 
        while (_step == 4) yield return null;


        // ====================================================
        // STEP 5: 강화 재시도 (성공)
        // ====================================================
        string msg5 = GetLocStr(locStrings.step5_Auth, "출력 승인됨. 강화를 실행하십시오.");
        ShowMessage(msg5);
        
        tutorialOverlay.FocusOn(btnRect); 

        while (_step == 5) yield return null;


        // ====================================================
        // [종료]
        // ====================================================
        string msgComplete = GetLocStr(locStrings.msg_Complete, "보정 완료. 시스템 정상 가동.");
        ShowMessage(msgComplete);
        
        tutorialOverlay.PlayFadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);

        // 이벤트 해제 및 정리
        workbenchUI.OnGearSelectedEvent -= HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent -= HandleEnhanceTry;
        workbenchUI.EndTutorialMode(); 
        
        // 저장 (DBManager에 저장)
        DBManager.I.SetProgress("Tutorial_Workbench", 1);
        DBManager.I.Save();
        
        Destroy(gameObject); 
    }

    // --------------------------------------------------------
    // 이벤트 핸들러
    // --------------------------------------------------------
    private void HandleGearSelected(GearData gear)
    {
        if ((_step == 1 || _step == 4) && gear == tutorialGear) _step++;
    }

    private void HandleEnhanceTry(EnhancementManager.EnhancementResult result)
    {
        if (_step == 2 && result == EnhancementManager.EnhancementResult.NotEnoughResources) _step++;
        else if (_step == 5 && result == EnhancementManager.EnhancementResult.Success) _step++;
    }

    private void ShowMessage(string msg)
    {
        if (notificationUI != null) notificationUI.ShowMessage(msg);
    }

    // [로컬라이징 헬퍼 함수]
    private string GetLocStr(LocalizedString locString, string fallback)
    {
        if (locString == null || locString.IsEmpty) return fallback;
        return locString.GetLocalizedString();
    }
}