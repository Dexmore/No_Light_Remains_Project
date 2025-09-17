using UnityEngine;
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T I
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if(_instance == null)
                {
                   GameObject o = new GameObject(typeof(T).Name);
                   _instance = o.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
    protected abstract bool IsDontDestroy();
    protected virtual void Awake()
    {
        if(IsDontDestroy())
        DontDestroyOnLoad(this.gameObject);
    }
}
