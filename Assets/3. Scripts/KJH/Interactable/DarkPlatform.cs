using UnityEngine;
public class DarkPlatform : DarkObject
{
    [SerializeField] GameObject platform;
    protected override void Start()
    {
        base.Start();
        platform?.SetActive(true);
    }
    public override void Run()
    {
        base.Run();
        platform?.SetActive(false);
    }
    


    
}
