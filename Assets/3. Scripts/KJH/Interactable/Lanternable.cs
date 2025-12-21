using UnityEngine;
public abstract class Lanternable : MonoBehaviour
{

    [HideInInspector] public float promptFill;
    public abstract bool isReady { get; set; }
    public abstract bool isAuto { get; }
    public abstract void Run();


}
