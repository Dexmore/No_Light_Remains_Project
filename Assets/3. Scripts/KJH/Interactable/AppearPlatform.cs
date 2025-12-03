using UnityEngine;
public class AppearPlatform : LightObject
{
    [SerializeField] GameObject platform;
    protected override void Start()
    {
        base.Start();
        platform?.SetActive(false);
    }
    public override void Run()
    {
        base.Run();
        platform?.SetActive(true);
    }
    


    
}
