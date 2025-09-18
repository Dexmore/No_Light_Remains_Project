using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class ExamplePlayerControl : MonoBehaviour
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
        inputActionAsset.FindActionMap("Player").FindAction("Move").canceled -= MoveInputCancel;
    }
    void MoveInput(InputAction.CallbackContext callback)
    {
        moveDirection = callback.ReadValue<Vector2>();
        
        if (!isMove)
        {
            isMove = true;
            StartCoroutine(nameof(Move));
        }
    }
    void MoveInputCancel(InputAction.CallbackContext callback)
    {
        StopCoroutine(nameof(Move));
        isMove = false;
        moveDirection = Vector2.zero;
    }
    Rigidbody2D rb;
    Vector2 moveDirection;
    [SerializeField] float moveForce;
    void Awake()
    {
        TryGetComponent(out rb);
    }
    bool isMove;
    IEnumerator Move()
    {
        while (true)
        {
            moveDirection = Vector2.ClampMagnitude(moveDirection, 1f);
            Debug.Log(moveDirection);
            rb.AddForce(moveDirection * moveForce);
            yield return YieldInstructionCache.WaitForFixedUpdate;
        }
    }

    
}
