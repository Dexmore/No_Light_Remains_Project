using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using NaughtyAttributes;
using DG.Tweening;

public class DialogControl : MonoBehaviour
{
    [SerializeField] private InputActionReference cancelAction;
    GameObject canvasObject;
    TMP_Text contentText;
    void Awake()
    {
        canvasObject = transform.GetChild(0).gameObject;
        contentText = transform.GetComponentInChildren<TMP_Text>(true);
        canvasObject.SetActive(false);
    }
    void OnEnable()
    {
        cancelAction.action.performed += InputESC;
    }
    void OnDisable()
    {
        cancelAction.action.performed -= InputESC;
    }
    void InputESC(InputAction.CallbackContext callbackContext)
    {
        
    }
    public void Open(int index)
    {
        AudioManager.I.PlaySFX("OpenPopup");
        contentText.text = allDialogTexts[index];
        canvasObject.SetActive(true);
        GameManager.I.isOpenDialog = true;
    }
    public void Close()
    {
        AudioManager.I.PlaySFX("UIClick");
        canvasObject.SetActive(false);
        GameManager.I.isOpenDialog = false;
    }
    string[] allDialogTexts = new string[]
    {
        //0
        "대사0...\n....\n.."
        ,
        //1
        "대사1...\n....\n.."
        ,
        //2
        "대사2...\n....\n.."
        ,
        //3
        "대사3...\n....\n.."
        ,
        //3
        "대사4...\n....\n.."




    };


#if UNITY_EDITOR
    public int testIndex;
    [Button]
    public void TestOpenPopup()
    {
        Open(testIndex);
    }
#endif



}
