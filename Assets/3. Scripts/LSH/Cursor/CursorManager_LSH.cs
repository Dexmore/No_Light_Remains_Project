using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CursorManager_LSH : MonoBehaviour
{
    [Header("UI Components")]
    public Image cursorImage;
    private CanvasGroup _cursorCanvasGroup;
    private RectTransform _cursorRect;

    [Header("Cursor Sprites")]
    public Sprite normalSprite;
    public Sprite attackSprite;
    public Sprite interactSprite;

    [Header("Option")]
    [Tooltip("체크하면 마우스가 게임 창 밖으로 못 나가게 가둡니다.")]
    public bool lockInWindow = true;

    [Header("Fade Settings")]
    public float autoHideTime = 3.0f;
    public float fadeDuration = 0.5f;

    private static CursorManager_LSH _instance;

    private float _lastMoveTime;
    private Vector2 _lastMousePos;
    private bool _isFadingOut = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (cursorImage != null)
        {
            _cursorRect = cursorImage.GetComponent<RectTransform>();
            _cursorCanvasGroup = cursorImage.GetComponent<CanvasGroup>();
            if (_cursorCanvasGroup == null)
                _cursorCanvasGroup = cursorImage.gameObject.AddComponent<CanvasGroup>();

            _cursorCanvasGroup.blocksRaycasts = false;
            _cursorCanvasGroup.interactable = false;

            // [추가됨] 시작 전(Awake)에는 투명하게(0) 만들어서 깜빡임 방지
            _cursorCanvasGroup.alpha = 0f; 
        }
    }

    void Start()
    {
        HideSystemCursor();
        
        _lastMoveTime = Time.time;
        if (Mouse.current != null)
            _lastMousePos = Mouse.current.position.ReadValue();

        SetNormal();

        // [추가됨] 게임이 시작(Start)되면 즉시 보이게(1) 설정
        if (_cursorCanvasGroup != null)
        {
            _cursorCanvasGroup.alpha = 1f;
        }
    }

    void Update()
    {
        Cursor.visible = false; 

        if (lockInWindow)
            Cursor.lockState = CursorLockMode.Confined;
        else
            Cursor.lockState = CursorLockMode.None;

        HandleVisibility();
    }

    void LateUpdate()
    {
        if (_cursorRect != null && Mouse.current != null)
        {
            _cursorRect.position = Mouse.current.position.ReadValue();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            HideSystemCursor();
        }
    }

    private void HideSystemCursor()
    {
        Cursor.visible = false;
        if (lockInWindow)
            Cursor.lockState = CursorLockMode.Confined;
    }

    private void HandleVisibility()
    {
        if (_cursorCanvasGroup == null || Mouse.current == null) return;

        Vector2 currentPos = Mouse.current.position.ReadValue();

        // 마우스 움직임 감지
        if ((currentPos - _lastMousePos).sqrMagnitude > 0.1f)
        {
            _lastMoveTime = Time.time;
            _lastMousePos = currentPos;

            _cursorCanvasGroup.alpha = 1f;
            _isFadingOut = false;
        }
        else
        {
            // 움직임이 멈추고 설정된 시간이 지나면 페이드 아웃 시작
            if (Time.time - _lastMoveTime > autoHideTime)
            {
                _isFadingOut = true;
            }
        }

        // 서서히 사라지는 연출
        if (_isFadingOut)
        {
            _cursorCanvasGroup.alpha = Mathf.MoveTowards(_cursorCanvasGroup.alpha, 0f, Time.deltaTime / fadeDuration);
        }
    }

    private void SetCursorImage(Sprite sprite)
    {
        if (cursorImage == null || sprite == null) return;
        cursorImage.sprite = sprite;
        cursorImage.SetNativeSize();
    }

    public void SetNormal() { SetCursorImage(normalSprite); }
    public void SetAttack() { SetCursorImage(attackSprite); }
    public void SetInteract() { SetCursorImage(interactSprite); }
}