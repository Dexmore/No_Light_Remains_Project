using System;
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

        rebindingOperation?.Cancel();

        rebindingOperation = actionToRebind.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse") 
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                operation.Dispose();
                actionToRebind.Enable();
                UpdateBindingDisplay();

                if (waitingForInputPanel != null)
                {
                    waitingForInputPanel.SetActive(false);
                }

                // FindObjectOfType<GameSettingManager_KWY>().OnKeyBindingChanged();
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
