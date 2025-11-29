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
        // 1. 인스턴스가 이미 초기화되었는지 확인 (Awake()가 호출되기 전에 'I'를 통해 초기화되었거나, 
        //    다른 씬에서 이미 존재하여 FindFirstObjectByType<T>()로 찾은 경우)
        if (_instance != null && _instance != this)
        {
            // 이미 존재하는 인스턴스가 현재 인스턴스가 아닐 경우, 
            // 현재 인스턴스(나중에 생성된 것)를 제거합니다.
            // 나중에 생성된 게임 오브젝트를 즉시 파괴
            Destroy(this.gameObject);
            //Debug.LogWarning($"[Singleton] 중복 인스턴스 감지: {typeof(T).Name}. 나중에 생성된 객체를 제거합니다.");
            return; // 이후 로직 실행 중단
        }
        
        // 2. 현재 인스턴스가 유일한 인스턴스인 경우, 자신을 _instance로 설정
        if (_instance == null)
        {
            _instance = this as T;
        }

        // 3. DontDestroyOnLoad 설정
        if (IsDontDestroy())
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
