using UnityEngine;
public abstract class Interactable : MonoBehaviour
{
    [System.Serializable]
    [System.Flags]
    public enum Type
    {
        Portal = 1 << 0,
        DropItem = 1 << 1,
        LightObject = 1 << 2,
        DarkObject = 1 << 3,
        NormalObject = 1 << 4,
    }
    public abstract Type type { get; }
    public abstract bool isReady { get; set;}
}
