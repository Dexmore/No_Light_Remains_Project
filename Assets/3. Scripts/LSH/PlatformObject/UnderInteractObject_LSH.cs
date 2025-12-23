using UnityEngine;

public class UnderInteractObject_LSH : MonoBehaviour
{
    PlayerControl playerControl;
    PlatformEffector2D platform;

    void Awake()
    {
        platform = GetComponent<PlatformEffector2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerControl = collision.gameObject.GetComponent<PlayerControl>();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (playerControl == null) return;
        if (playerControl.fallThroughPlatform)
        {
            platform.rotationalOffset = 180f;
            playerControl = null;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        playerControl = null;
        platform.rotationalOffset = 0f;
    }
}
