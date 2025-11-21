using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


public class KeyRemapper_KWY : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private Button bindingButton;
    [SerializeField] private TextMeshProUGUI bindingKeyText;
    [SerializeField] private GameObject waitingForInputPanel;

    private InputAction actionToRebind;
    private int bindingIndex;
    private string oldBindingPath;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    public void Initialize(InputAction action, int bindingIndex, string actionName)
    {
        this.actionToRebind = action;
        this.bindingIndex = bindingIndex;
        actionNameText.text = actionName;

        bindingButton.onClick.AddListener(StartRebinding);
        UpdateBindingDisplay();

        if (waitingForInputPanel != null)
        {
            waitingForInputPanel.SetActive(false);
        }
    }

    public void UpdateBindingDisplay()
    {
        if (actionToRebind != null)
        {
            string bindingPath = actionToRebind.bindings[bindingIndex].effectivePath;
            bindingKeyText.text = InputControlPath.ToHumanReadableString(
                bindingPath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }
    }

    private void StartRebinding()
    {
        if (waitingForInputPanel != null)
        {
            waitingForInputPanel.SetActive(true);
        }

        actionToRebind.Disable();

        oldBindingPath = actionToRebind.bindings[bindingIndex].effectivePath;

        rebindingOperation?.Cancel();

        rebindingOperation = actionToRebind.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                var newBinding = actionToRebind.bindings[bindingIndex];
                var newBindingPath = newBinding.effectivePath;
                bool isDuplicate = false;

                foreach (var action in actionToRebind.actionMap.asset.actionMaps.SelectMany(map => map.actions))
                {
                    foreach (var binding in action.bindings)
                    {
                        if (binding.id == newBinding.id) continue;
                        if (!binding.path.Contains("<Keyboard>") && !binding.path.Contains("<Mouse>")) continue;
                        if (binding.effectivePath == newBindingPath)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                    if (isDuplicate) break;
                }

                if (isDuplicate)
                {
                    actionToRebind.ApplyBindingOverride(bindingIndex, oldBindingPath);
                    Debug.LogWarning($"중복된 키 입력({newBindingPath})입니다. 원래 키({oldBindingPath})로 되돌립니다.");
                }
                else
                {
                    FindObjectOfType<GameSettingManager_KWY>().OnKeyBindingChanged();
                }

                operation.Dispose();
                actionToRebind.Enable();
                UpdateBindingDisplay();

                if (waitingForInputPanel != null)
                {
                    waitingForInputPanel.SetActive(false);
                }
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                actionToRebind.Enable();
                UpdateBindingDisplay();

                if (waitingForInputPanel != null)
                {
                    waitingForInputPanel.SetActive(false);
                }
            });

        rebindingOperation.Start();
    }
}
