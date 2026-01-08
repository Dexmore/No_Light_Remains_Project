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
        // if (PlayerPrefs.GetInt("Tutorial_Workbench_Completed", 0) == 1)
        // {
        //     Debug.Log("1");
        //     gameObject.SetActive(false);
        //     return;
        // }
        Debug.Log("[Tutorial] 컨트롤러 시작됨. 시퀀스 가동 준비.");
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // 1. UI 켜질 때까지 대기
        while (!workbenchUI.IsUIActive()) yield return null;
        
        // 2. 부팅 연출 대기 (연출 길이에 맞춰 조절하세요)
        yield return new WaitForSeconds(9.4f); 

        // 3. 튜토리얼 모드 진입 (더미 데이터 표시)
        List<GearData> dummies = new List<GearData> { tutorialGear };
        workbenchUI.BeginTutorialMode(dummies);

        // 이벤트 구독
        workbenchUI.OnGearSelectedEvent += HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent += HandleEnhanceTry;

        // ====================================================
        // STEP 1: 기어 슬롯 보여주기 & 선택 유도
        // ====================================================
        _step = 1;
        ShowMessage("시스템 보정: 강화할 기어 모듈을 선택하십시오.");
        
        // 첫 번째 슬롯 강조
        RectTransform slotRect = workbenchUI.GetSlotRect(0);
        tutorialOverlay.FocusOn(slotRect);
        tutorialOverlay.PlayFadeIn(0.5f); // 0.5초 동안 페이드 인

        while (_step == 1) yield return null; // 선택할 때까지 대기


        // ====================================================
        // STEP 2: 강화 버튼 누르기 (실패 유도)
        // ====================================================
        ShowMessage("무결성 검증: 강화 프로세스를 실행하십시오.");
        
        RectTransform btnRect = workbenchUI.GetEnhanceButtonRect();
        tutorialOverlay.FocusOn(btnRect);

        while (_step == 2) yield return null; // 버튼 누를 때까지 대기 (결과는 HandleEnhanceTry에서 확인)


        // ====================================================
        // STEP 3: 재료 부족 확인 (3초간 재료 부분 강조)
        // ====================================================
        ShowMessage("오류: 자원 부족. 필요한 자원을 확인하십시오.");

        // 재료 하나가 아니라, 재료가 모여있는 '전체 패널'을 강조합니다.
        RectTransform costPanelRect = workbenchUI.GetCostPanelRect();
        
        // 만약 패널을 못 찾으면 기존 방식대로 첫 번째 텍스트라도 보여줌 (안전장치)
        if (costPanelRect == null) costPanelRect = workbenchUI.GetCostTextRect(0);
        
        tutorialOverlay.FocusOn(costPanelRect);

        // 3초간 대기
        yield return new WaitForSeconds(3.0f);


        // ====================================================
        // STEP 4: 재료 공급 & 다시 선택 유도
        // ====================================================
        // 재료 지급
        DBManager.I.AddItem(tutorialMaterial.name, 10);
        
        // [중요] '다시 선택'하게 하기 위해 기존 선택을 강제로 해제함
        workbenchUI.ForceDeselect();

        ShowMessage("자원 보급 완료. 기어를 다시 선택하여 초기화를 재개하십시오.");
        
        // 다시 슬롯 강조
        tutorialOverlay.FocusOn(slotRect);

        // 스텝 변수 조정 (다시 선택 단계로)
        _step = 4; 
        while (_step == 4) yield return null; // 다시 선택할 때까지 대기


        // ====================================================
        // STEP 5: 강화 재시도 (성공 유도)
        // ====================================================
        // 선택되면 프리뷰가 떴을 테니 메시지 변경
        ShowMessage("출력 승인됨. 강화를 실행하십시오.");
        tutorialOverlay.FocusOn(btnRect); // 버튼 강조

        while (_step == 5) yield return null; // 성공할 때까지 대기


        // ====================================================
        // [종료] 완료 처리
        // ====================================================
        ShowMessage("보정 완료. 시스템 정상 가동.");
        tutorialOverlay.PlayFadeOut(0.5f);

        // 페이드 아웃 되는 동안 잠시 기다렸다가 데이터 정리 (선택 사항)
        yield return new WaitForSeconds(0.5f);
        
        // 정리
        workbenchUI.OnGearSelectedEvent -= HandleGearSelected;
        workbenchUI.OnEnhanceTryEvent -= HandleEnhanceTry;
        workbenchUI.EndTutorialMode(); // 실제 데이터로 복귀
        
        // 지급했던 재료 회수
        DBManager.I.AddItem(tutorialMaterial.name, -10); 

        // 저장
        PlayerPrefs.SetInt("Tutorial_Workbench_Completed", 1);
        PlayerPrefs.Save();
        
        Destroy(gameObject); 
    }

    // --------------------------------------------------------
    // 이벤트 핸들러
    // --------------------------------------------------------

    private void HandleGearSelected(GearData gear)
    {
        // STEP 1: 처음 선택할 때
        if (_step == 1 && gear == tutorialGear)
        {
            _step++; 
        }
        // STEP 4: 재료 받고 다시 선택할 때
        else if (_step == 4 && gear == tutorialGear)
        {
            _step++;
        }
    }

    private void HandleEnhanceTry(EnhancementManager.EnhancementResult result)
    {
        // STEP 2: 재료 없어서 실패해야 함
        if (_step == 2)
        {
            if (result == EnhancementManager.EnhancementResult.NotEnoughResources)
            {
                // 실패 확인 후 다음 단계(재료 보여주기)로 진행
                _step++; 
            }
        }
        // STEP 5: 이제 성공해야 함
        else if (_step == 5)
        {
            if (result == EnhancementManager.EnhancementResult.Success)
            {
                _step++; // 루프 탈출
            }
        }
    }

    private void ShowMessage(string msg)
    {
        if (notificationUI != null) notificationUI.ShowMessage(msg);
    }
}