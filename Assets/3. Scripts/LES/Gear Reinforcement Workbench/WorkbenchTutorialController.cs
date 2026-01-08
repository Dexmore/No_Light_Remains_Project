using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorkbenchTutorialController : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private WorkbenchUI workbenchUI;
    [SerializeField] private TutorialOverlay tutorialOverlay;
    [SerializeField] private NotificationUI notificationUI;

    [Header("더미 데이터")]
    [SerializeField] private GearData tutorialGear; 
    [SerializeField] private ItemData tutorialMaterial; 

    private int _step = 0;

    private void Start()
    {
        // [수정] PlayerPrefs 대신 DBManager의 ProgressData 확인
        // "Tutorial_Workbench"가 1이면 완료된 것
        if (DBManager.I.GetProgress("Tutorial_Workbench") == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        // [디버그] 개발 중에는 위 코드를 주석 처리하고 테스트하세요.
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // 1. UI 켜질 때까지 대기
        while (!workbenchUI.IsUIActive()) yield return null;
        
        // 2. 부팅 연출 대기 (9.4초는 테스트 시 너무 길 수 있으니 확인 필요)
        yield return new WaitForSeconds(9.4f); 

        // 3. 튜토리얼 모드 진입
        List<GearData> dummies = new List<GearData> { tutorialGear };
        workbenchUI.BeginTutorialMode(dummies);

        workbenchUI.OnGearSelectedEvent += HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent += HandleEnhanceTry;

        // STEP 1: 기어 선택
        _step = 1;
        ShowMessage("시스템 보정: 강화할 기어 모듈을 선택하십시오.");
        RectTransform slotRect = workbenchUI.GetSlotRect(0);
        
        tutorialOverlay.FocusOn(slotRect);
        tutorialOverlay.PlayFadeIn(0.5f); // 페이드 인

        while (_step == 1) yield return null; 

        // STEP 2: 강화 시도 (재료 없음)
        ShowMessage("무결성 검증: 강화 프로세스를 실행하십시오.");
        RectTransform btnRect = workbenchUI.GetEnhanceButtonRect();
        tutorialOverlay.FocusOn(btnRect);

        while (_step == 2) yield return null;

        // STEP 3: 재료 부족 확인 (3초 대기)
        ShowMessage("오류: 자원 부족. 필요한 자원을 확인하십시오.");
        RectTransform costPanelRect = workbenchUI.GetCostPanelRect();
        if (costPanelRect == null) costPanelRect = workbenchUI.GetCostTextRect(0);
        tutorialOverlay.FocusOn(costPanelRect);

        yield return new WaitForSeconds(3.0f);

        // STEP 4: 재료 공급 및 재선택
        // [수정] DBManager 대신 UI 내부 가짜 재료함에 추가 (실제 인벤토리 보호)
        workbenchUI.AddTutorialDummyMaterial(10); 
        
        workbenchUI.ForceDeselect(); // 선택 해제
        
        ShowMessage("자원 보급 완료. 기어를 다시 선택하여 초기화를 재개하십시오.");
        tutorialOverlay.FocusOn(slotRect);

        _step = 4; 
        while (_step == 4) yield return null;

        // STEP 5: 강화 재시도 (성공)
        ShowMessage("출력 승인됨. 강화를 실행하십시오.");
        tutorialOverlay.FocusOn(btnRect); 

        while (_step == 5) yield return null;

        // [종료]
        ShowMessage("보정 완료. 시스템 정상 가동.");
        tutorialOverlay.PlayFadeOut(0.5f);
        yield return new WaitForSeconds(0.5f);

        workbenchUI.OnGearSelectedEvent -= HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent -= HandleEnhanceTry;
        workbenchUI.EndTutorialMode(); 
        
        // 가짜 재료 썼으므로 회수할 필요 없음

        // [저장] PlayerPrefs 대신 DBManager에 저장
        // 현재 스팀 온라인/오프라인 모드에 맞춰 자동으로 해당 슬롯에 저장됨
        DBManager.I.SetProgress("Tutorial_Workbench", 1);
        DBManager.I.Save();
        
        Destroy(gameObject); 
    }

    // 핸들러는 그대로
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
}