using System.Threading.Tasks;
using UnityEngine;
public class DoorType1 : MonoBehaviour
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
    public async void Open()
    {
        if (isOpen) return;
        isOpen = true;
        animator.Play("Open");
        await Task.Delay(700);
        AudioManager.I.PlaySFX("DoorOpen", transform.position, spatialBlend: 0.5f);
    }
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        AudioManager.I.PlaySFX("DoorClose", transform.position, spatialBlend: 0.5f);
        animator.Play("Close");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        Open();
    }


}
