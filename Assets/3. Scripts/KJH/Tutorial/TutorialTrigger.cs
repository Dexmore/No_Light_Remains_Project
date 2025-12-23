using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialControl tutorialControl;
    int playerLayer;
    void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != playerLayer) return;
        tutorialControl.TutorialTriggerEnter(transform.name);
    }
    
    


}
