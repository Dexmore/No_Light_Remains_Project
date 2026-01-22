using UnityEngine;
using System.Collections;

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
            if (playerControl == null)
                playerControl = collision.gameObject.GetComponent<PlayerControl>();
            platform.rotationalOffset = 0f;
        }
    }

    IEnumerator DelayedResetRotationalOffset()
    {
        yield return YieldInstructionCache.WaitForSeconds(0.1f);
        platform.rotationalOffset = 0f;
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (playerControl == null) return;
        if (playerControl.fallThroughPlatform)
        {
            platform.rotationalOffset = 180f;
        }
        else
        {
            //platform.rotationalOffset = 0f;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        platform.rotationalOffset = 0f;
        StopCoroutine(nameof(DelayedResetRotationalOffset));
        StartCoroutine(nameof(DelayedResetRotationalOffset));
    }

}
