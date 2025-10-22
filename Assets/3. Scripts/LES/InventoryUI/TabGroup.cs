using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

namespace YourProject.UI
{
    [DisallowMultipleComponent]
    public class TabGroup : MonoBehaviour
    {
        [Header("탭 버튼 (순서대로 등록)")]
        [Tooltip("'소지템', '랜턴', '기어', '기록물' 순서로 Button을 등록하세요.")]
        [SerializeField]
        private List<Button> tabButtons;
        
        [Header("콘텐츠 패널 (위 탭과 순서 일치)")]
        [Tooltip("각 탭에 해당하는 콘텐츠 Panel의 CanvasGroup을 순서대로 등록하세요.")]
        [SerializeField]
        private List<CanvasGroup> contentPanels;

        [Header("탭 전환 버튼")]
        [SerializeField] private Button prevTabButton;
        [SerializeField] private Button nextTabButton;

        [Header("탭 상태 색상")]
        [SerializeField] private Color tabIdleColor = Color.gray;
        [SerializeField] private Color tabHoverColor = Color.white;
        [SerializeField] private Color tabActiveColor = Color.white;
        
        [Header("효과 설정")]
        [SerializeField] [Range(0.1f, 1f)] 
        private float contentFadeDuration = 0.2f;

        // [추가] 각 패널의 기능 스크립트를 담을 리스트
        private List<ITabContent> _tabContents;

        private int _currentTabIndex = -1;
        private Coroutine _tabSwitchCoroutine;

        // Awake는 Start보다 먼저 호출됩니다. 초기화에 적합합니다.
        private void Awake()
        {
            // [추가] contentPanels을 기반으로 기능 스크립트를 찾아 리스트에 저장
            _tabContents = new List<ITabContent>();
            foreach (var panel in contentPanels)
            {
                // 각 패널 게임 오브젝트에서 ITabContent 인터페이스를 구현한 컴포넌트를 찾습니다.
                ITabContent content = panel.GetComponent<ITabContent>();
                if (content != null)
                {
                    _tabContents.Add(content);
                }
                else
                {
                    Debug.LogWarning($"{panel.name}에 ITabContent를 구현한 스크립트가 없습니다!", panel.gameObject);
                }
            }
        }

        private void Start()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                int index = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(index));
            }
            prevTabButton?.onClick.AddListener(SelectPreviousTab);
            nextTabButton?.onClick.AddListener(SelectNextTab);
        }
        
        // OnEnable은 UI가 활성화될 때마다 호출됩니다.
        private void OnEnable()
        {
            // [수정 1] UI가 켜질 때 첫 탭 콘텐츠가 바로 보이도록 수정
            // 페이드 효과 없이 즉시 첫 탭의 상태를 설정합니다.
            // 전체 UI의 페이드 효과는 부모인 InventoryUI가 담당하므로 여기서 또 페이드를 할 필요가 없습니다.
            if (tabButtons.Count > 0)
            {
                // 모든 콘텐츠 패널을 일단 투명하게 초기화
                foreach (var panel in contentPanels)
                {
                    panel.alpha = 0f;
                    panel.interactable = false;
                    panel.blocksRaycasts = false;
                }

                // 첫 번째 탭만 즉시 보이도록 설정
                _currentTabIndex = 0;
                contentPanels[0].alpha = 1f;
                contentPanels[0].interactable = true;
                contentPanels[0].blocksRaycasts = true;
                
                // 탭 버튼 색상도 첫 탭 기준으로 즉시 설정
                UpdateTabButtonColors();

                // [추가] 첫 탭의 OnShow() 호출
                _tabContents[0]?.OnShow();
            }
        }
        
        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current[Key.Q].wasPressedThisFrame) SelectPreviousTab();
            if (Keyboard.current[Key.E].wasPressedThisFrame) SelectNextTab();
        }

        public void SelectNextTab()
        {
            if (tabButtons.Count == 0) return;
            int nextIndex = (_currentTabIndex + 1) % tabButtons.Count;
            SelectTab(nextIndex);
        }

        public void SelectPreviousTab()
        {
            if (tabButtons.Count == 0) return;
            int prevIndex = _currentTabIndex - 1;
            if (prevIndex < 0) prevIndex = tabButtons.Count - 1;
            SelectTab(prevIndex);
        }

        // [수정 2] 탭 전환 로직을 안정적으로 변경
        private void SelectTab(int newIndex)
        {
            // 같은 탭을 또 누르면 아무것도 하지 않음
            if (_currentTabIndex == newIndex || newIndex < 0 || newIndex >= tabButtons.Count) return;

            // 이전에 실행되던 전환 코루틴이 있다면 중지
            if (_tabSwitchCoroutine != null)
            {
                StopCoroutine(_tabSwitchCoroutine);
            }
            
            // 새로운 탭 전환 코루틴을 실행
            _tabSwitchCoroutine = StartCoroutine(SwitchTabsCoroutine(newIndex));
        }

        private IEnumerator SwitchTabsCoroutine(int newIndex)
        {
            int oldIndex = _currentTabIndex;
            _currentTabIndex = newIndex; // 현재 탭 인덱스를 즉시 업데이트
            
            UpdateTabButtonColors(); // 탭 버튼 색상도 즉시 업데이트

            // 이전 콘텐츠를 페이드 아웃
            if (oldIndex != -1)
            {
                _tabContents[oldIndex]?.OnHide();
                yield return StartCoroutine(FadeContent(contentPanels[oldIndex], 0f));
            }

            // 새 콘텐츠를 페이드 인
            _tabContents[newIndex]?.OnShow();
            yield return StartCoroutine(FadeContent(contentPanels[newIndex], 1f, true));
        }
        
        private void UpdateTabButtonColors()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                var colors = tabButtons[i].colors;
                colors.normalColor = (i == _currentTabIndex) ? tabActiveColor : tabIdleColor;
                colors.highlightedColor = (i == _currentTabIndex) ? tabActiveColor : tabHoverColor;
                tabButtons[i].colors = colors;
            }
        }

        private IEnumerator FadeContent(CanvasGroup panel, float targetAlpha, bool isInteractableAfter = false)
        {
            panel.interactable = false;
            panel.blocksRaycasts = false;
            
            float startAlpha = panel.alpha;
            float timer = 0f;

            while (timer < contentFadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / contentFadeDuration);
                yield return null;
            }

            panel.alpha = targetAlpha;
            if (isInteractableAfter)
            {
                panel.interactable = true;
                panel.blocksRaycasts = true;
            }
        }
    }
}