using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class TestPlayerControl : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActionAsset;
    void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Move").performed += MoveInput;
        inputActionAsset.FindActionMap("Player").FindAction("Move").canceled += MoveInputCancel;
    }
    void OnDisable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Move").performed -= MoveInput;
        inputActionAsset.FindActionMap("Player").FindAction("Move").performed -= MoveInputCancel;
    }
    void MoveInput(InputAction.CallbackContext callback)
    {
        StartCoroutine(nameof(Move));
    }
    void MoveInputCancel(InputAction.CallbackContext callback)
    {
        StopCoroutine(nameof(Move));
    }
    IEnumerator Move()
    {
        while (true)
        {
            yield return null;
        }
    }

    
}
