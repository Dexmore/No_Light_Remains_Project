using UnityEngine;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;
public class NewMonoBehaviourScript : MonoBehaviour
{
    public Astar2DXYPathFinder astar;
    void Awake()
    {
        TryGetComponent(out astar);
    }

    public Vector2 vector2;

    [Button]
    public void Test()
    {
        astar.Find(vector2).Forget();
    }

}
