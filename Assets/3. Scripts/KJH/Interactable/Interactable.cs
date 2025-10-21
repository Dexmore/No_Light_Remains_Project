using UnityEngine;
public class Interactable : MonoBehaviour
{
    [System.Serializable]
    [System.Flags]
    public enum Type
    {
        Portal = 1<<0,
        Item = 1<<1,
        NPC = 1<<2,
        Object = 1<<3,
        Object_Light = 1<<4,
        Object_Dark = 1<<5,
    }
    public Type type;
}
