using UnityEngine;
public class DialogTrigger : MonoBehaviour
{
    public int index;
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.I.onDialogTriggerEnter.Invoke(index, this);
    }
    
}
