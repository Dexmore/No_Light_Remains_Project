using UnityEngine;
public abstract class Lanternable : MonoBehaviour
{

    [HideInInspector] public float promptFill;
    public abstract bool isReady { get; set; }
    public abstract bool isAuto { get; }
    public abstract void Run();
    public abstract void PromptFill();
    public abstract void PromptCancel();
    public abstract ParticleSystem particle {get;}
    public abstract SpriteRenderer lightPoint {get;}


}
