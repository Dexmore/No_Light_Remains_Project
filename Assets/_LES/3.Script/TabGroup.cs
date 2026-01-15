using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro; 
using UnityEngine.EventSystems;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public class TabGroup : MonoBehaviour
    {
        [Header("탭 버튼 (순서대로 등록)")]
        [Tooltip("'랜턴', '기어', '소지템', '기록물' 순서로 Button을 등록하세요.")] // [수정] 순서 변경 반영
        [SerializeField] private List<Button> tabButtons;
        
        [Header("콘텐츠 패널 (위 탭과 순서 일치)")]
        [SerializeField] private List<CanvasGroup> contentPanels;

        [Header("탭 전환 버튼")]
        [SerializeField] private Button prevTabButton;
        [SerializeField] private Button nextTabButton;

        [Space(10)]
        [Header("탭 디자인 설정")]
        [Header("1. 버튼 배경 색상")]
        [SerializeField] private Color tabIdleColor = Color.gray;
        [SerializeField] private Color tabHoverColor = Color.white;
        [SerializeField] private Color tabActiveColor = Color.white;

        [Header("2. 텍스트 색상")]
        [Tooltip("비활성 상태 글자 색 (어둡게)")]
        [SerializeField] private Color textIdleColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
        [Tooltip("활성/포커스 상태 글자 색 (밝게)")]
        [SerializeField] private Color textActiveColor = Color.white; 

        [Header("3. 크기 강조 (확정 선택 시)")]
        [Tooltip("선택된 탭이 얼마나 커질지 (예: 1.15 = 15% 확대)")]
        [SerializeField] private Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1f); 
        [SerializeField] private float animationSpeed = 15f;

        private List<ITabContent> _tabContents;
        private int _currentTabIndex = -1;

        private void Awake()
        {
            _tabContents = new List<ITabContent>();
            foreach (var panel in contentPanels)
            {
                ITabContent content = panel.GetComponent<ITabContent>();
                if (content != null) _tabContents.Add(content);
                else Debug.LogWarning($"{panel.name}에 ITabContent가 없습니다!", panel.gameObject);
            }
        }

        private void Start()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                int index = i;
                tabButtons[i].onClick.AddListener(() => 
                {
                    AudioManager.I?.PlaySFX("InventoryUI_button1");
                    SelectTab(index);
                });
            }
            
            prevTabButton?.onClick.AddListener(() => {
                AudioManager.I?.PlaySFX("InventoryUI_button1");
                SelectPreviousTab();
            });
            nextTabButton?.onClick.AddListener(() => {
                AudioManager.I?.PlaySFX("InventoryUI_button1");
                SelectNextTab();
            });
        }
        
        private void OnEnable()
        {
            if (tabButtons.Count > 0)
            {
                foreach (var panel in contentPanels)
                {
                    panel.alpha = 0f;
                    panel.interactable = false;
                    panel.blocksRaycasts = false;
                }

                _currentTabIndex = 0;
                contentPanels[0].alpha = 1f;
                contentPanels[0].interactable = true;
                contentPanels[0].blocksRaycasts = true;
                
                _tabContents[0]?.OnShow();
            }
        }
        
        private void Update()
        {
            // 키보드 탭 전환 (Q/E)
            if (Keyboard.current != null)
            {
                if (Keyboard.current[Key.Q].wasPressedThisFrame) 
                {
                    AudioManager.I?.PlaySFX("InventoryUI_button1");
                    SelectPreviousTab();
                }
                if (Keyboard.current[Key.E].wasPressedThisFrame) 
                {
                    AudioManager.I?.PlaySFX("InventoryUI_button1");
                    SelectNextTab();
                }
            }

            UpdateTabVisualsRealtime();
        }

        private void UpdateTabVisualsRealtime()
        {
            GameObject focusedObj = EventSystem.current.currentSelectedGameObject;
            
            // 1. "지금 탭 버튼들 중에 포커스 된 놈이 있는가?" 확인
            bool isAnyTabFocused = false;
            foreach(var btn in tabButtons)
            {
                if (focusedObj == btn.gameObject) 
                {
                    isAnyTabFocused = true;
                    break;
                }
            }

            for (int i = 0; i < tabButtons.Count; i++)
            {
                bool isPageActive = (i == _currentTabIndex);       // 현재 페이지인가?
                bool isFocused = (focusedObj == tabButtons[i].gameObject); // 커서가 있는가?

                // ---------------------------------------------------------
                // A. 텍스트 색상 로직 (피드백 반영)
                // ---------------------------------------------------------
                Color targetTextColor = textIdleColor;

                if (isAnyTabFocused)
                {
                    // 탭 버튼에 커서가 있다면 -> 오직 '커서 있는 놈'만 밝게 (Active여도 커서 없으면 어둡게)
                    if (isFocused) targetTextColor = textActiveColor;
                }
                else
                {
                    // 탭 버튼에 커서가 없다면 (슬롯 조작 중) -> '현재 페이지'를 밝게
                    if (isPageActive) targetTextColor = textActiveColor;
                }
                
                var tmpText = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.color = Color.Lerp(tmpText.color, targetTextColor, Time.unscaledDeltaTime * animationSpeed);
                }

                // ---------------------------------------------------------
                // B. 크기 로직 (선택된 페이지는 항상 커짐)
                // ---------------------------------------------------------
                // "엔터로 선택하면 커짐" -> 즉 Active 상태일 때 커져 있으면 됩니다.
                Vector3 targetScale = isPageActive ? selectedScale : Vector3.one;
                tabButtons[i].transform.localScale = Vector3.Lerp(tabButtons[i].transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
            
                // ---------------------------------------------------------
                // C. 버튼 배경색 (Button 컴포넌트 동기화)
                // ---------------------------------------------------------
                var btnColors = tabButtons[i].colors;
                if (isPageActive) 
                {
                    btnColors.normalColor = tabActiveColor; 
                    btnColors.selectedColor = tabActiveColor;
                }
                else 
                {
                    btnColors.normalColor = tabIdleColor;
                    btnColors.selectedColor = tabHoverColor;
                }
                tabButtons[i].colors = btnColors;
            }
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

        private void SelectTab(int newIndex)
        {
            if (_currentTabIndex == newIndex || newIndex < 0 || newIndex >= tabButtons.Count) return;

            int oldIndex = _currentTabIndex;
            _currentTabIndex = newIndex;
            
            // 이전 탭 닫기
            if (oldIndex != -1)
            {
                _tabContents[oldIndex]?.OnHide();
                contentPanels[oldIndex].alpha = 0f;
                contentPanels[oldIndex].interactable = false;
                contentPanels[oldIndex].blocksRaycasts = false;
            }

            // 새 탭 열기
            _tabContents[newIndex]?.OnShow();
            contentPanels[newIndex].alpha = 1f;
            contentPanels[newIndex].interactable = true;
            contentPanels[newIndex].blocksRaycasts = true;
        }
    }
}