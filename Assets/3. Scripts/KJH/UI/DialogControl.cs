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
    PlayerControl playerControl;
    void Awake()
    {
        playerControl = FindAnyObjectByType<PlayerControl>();
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
        if (!GameManager.I.isOpenDialog)
        {
            //최초 켜지는 연출
        }
        else
        {
            //대사창 교체 연출
        }
        AudioManager.I.PlaySFX("OpenPopup");
        contentText.text = allDialogTexts[index, 0];
        canvasObject.SetActive(true);
        GameManager.I.isOpenDialog = true;
        if (playerControl.fsm.currentState != playerControl.openUIMenu)
            playerControl.fsm.ChangeState(playerControl.openUIMenu);
    }
    public void Close()
    {
        AudioManager.I.PlaySFX("UIClick");
        canvasObject.SetActive(false);
        GameManager.I.isOpenDialog = false;
    }
    string[,] allDialogTexts = new string[,]
    {
        //0
        {
            "대사0-1페이지...\n....\n..",
            "대사0-2페이지...\n....\n.."
        },
        //1
        {
            "대사1-1페이지...\n....\n..",
            "대사1-2페이지...\n....\n.."
        },
        //2
        {
            "대사0-1페이지...\n....\n..",
            "대사0-2페이지...\n....\n.."
        },
        //3
        {
            "대사3-1페이지...\n....\n..",
            "대사3-2페이지...\n....\n.."
        },
    };

#if UNITY_EDITOR
    [Header("Editor Test")]
    public int testIndex;
    [Button]
    public void TestOpen()
    {
        Open(testIndex);
    }
#endif



}
