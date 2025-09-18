using UnityEngine;
public abstract class PoolBehaviour : MonoBehaviour
{
    [HideInInspector] public PoolManager poolManager;
    public void Despawn()
    {
        try
        {
            poolManager.Despawn(this);
        }
        catch
        {
            Destroy(gameObject);
        }
    }
}
