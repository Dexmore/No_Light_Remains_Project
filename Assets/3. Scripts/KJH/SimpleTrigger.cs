using UnityEngine;
public class SimpleTrigger : MonoBehaviour
{
    [SerializeField] int index;
    [SerializeField] LayerMask targetLayerMask;
    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     GameManager.I.onSimpleTriggerEnter.Invoke(index, this);
    // }
    // void OnTriggerExit2D(Collider2D collision)
    // {
    //     GameManager.I.onSimpleTriggerExit.Invoke(index, this);
    // }

}
