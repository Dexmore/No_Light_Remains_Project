using UnityEngine;
public abstract class Interactable : MonoBehaviour
{
    [System.Serializable]
    [System.Flags]
    public enum Type
    {
        Portal = 1<<0,
        Item = 1<<1,
        NPC = 1<<2,
        Object = 1<<3,
        LightObject = 1<<4,
    }
    public abstract Type type { get; }
}
