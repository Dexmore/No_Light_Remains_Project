using UnityEngine;
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private int interactionPriority = 0;
    public virtual int Priority => interactionPriority;

    [System.Serializable]
    [System.Flags]
    public enum Type
    {
        Portal = 1 << 0,
        DropItem = 1 << 1,
        Normal = 1 << 2,
    }
    public abstract Type type { get; }
    public abstract bool isReady { get; set; }
    public abstract bool isAuto { get; }
    public abstract void Run();
}
