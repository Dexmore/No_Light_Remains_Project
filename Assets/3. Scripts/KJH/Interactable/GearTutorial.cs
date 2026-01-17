using UnityEngine;
public class GearTutorial : MonoBehaviour
{
    public ChestInteractable_LSH chest;
    public void ForceOpen()
    {
        if (chest.isReady)
            chest.Run();
    }







}
