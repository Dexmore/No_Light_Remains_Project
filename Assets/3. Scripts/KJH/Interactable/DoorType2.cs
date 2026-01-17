using System.Collections;
using UnityEngine;
public class DoorType2 : MonoBehaviour
{
    Collider2D col;
    public void SetCompletedImmediately()
    {
        if(col) col.enabled = false;
        animator.Play("Open2");
    }
    Animator animator;
    public bool isOneWay;
    void Awake()
    {
        TryGetComponent(out col);
        TryGetComponent(out animator);
        playerLayer = LayerMask.NameToLayer("Player");
        isOpen = false;
    }
    bool isOpen;
    int playerLayer;
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        AudioManager.I.PlaySFX("DoorOpen", transform.position, spatialBlend: 0.5f);
        animator.Play("Open");
    }
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        AudioManager.I.PlaySFX("DoorOpen", transform.position, spatialBlend: 0.5f);
        animator.Play("Close");
    }


}
