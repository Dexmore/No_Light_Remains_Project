using UnityEngine;

public class CursorManager_LSH : MonoBehaviour
{
    [Header("Cursor Textures")]
    public Texture2D normalCursor;
    public Texture2D attackCursor;
    public Texture2D interactCursor;

    [Header("Option")]
    public bool lockInWindow = true;

    private static CursorManager_LSH _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (lockInWindow)
            Cursor.lockState = CursorLockMode.Confined;

        SetNormal();
    }

    private void SetCursor(Texture2D tex, Vector2 hotspot)
    {
        Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
    }

    private Vector2 GetCenterHotspot(Texture2D tex)
    {
        if (tex == null) return Vector2.zero;
        return new Vector2(tex.width * 0.5f, tex.height * 0.5f);
    }

    public void SetNormal()
    {
        SetCursor(normalCursor, GetCenterHotspot(normalCursor));
    }

    public void SetAttack()
    {
        SetCursor(attackCursor, GetCenterHotspot(attackCursor));
    }

    public void SetInteract()
    {
        SetCursor(interactCursor, GetCenterHotspot(interactCursor));
    }
}