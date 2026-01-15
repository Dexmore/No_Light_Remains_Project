using UnityEngine;
public class GearChest : MonoBehaviour
{
    public ChestInteractable_LSH chest;
    public void ForceOpen()
    {
        if (chest.isReady)
            chest.Run();
    }







}
