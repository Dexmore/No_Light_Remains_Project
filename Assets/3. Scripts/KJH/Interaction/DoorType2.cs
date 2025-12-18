using UnityEngine;
public class DoorType2 : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
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
