using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControl : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActionAsset;
    [System.Serializable]
    public enum State
    {
        Idle,
        Move,
        Jump,
    }
    public State state;
    void OnEnable()
    {
        inputActionAsset.FindActionMap("Player").FindAction("Move").performed += MoveButtonDown;
    }
    public void MoveButtonDown(InputAction.CallbackContext callbackContext)
    {
        Debug.Log(callbackContext.ReadValue<Vector2>());
    }

    



    
    

}
