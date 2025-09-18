using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
public class PoolManager : SingletonBehaviour<PoolManager>
{
    protected override bool IsDontDestroy() => true;
    Dictionary<PoolBehaviour, ObjectPool<PoolBehaviour>> prefabs;
    Dictionary<PoolBehaviour, ObjectPool<PoolBehaviour>> instances;
    protected override void Awake()
    {
        base.Awake();
        prefabs = new Dictionary<PoolBehaviour, ObjectPool<PoolBehaviour>>();
        instances = new Dictionary<PoolBehaviour, ObjectPool<PoolBehaviour>>();
    }
    public void ClearPool()
    {
        prefabs.ToList().ForEach(x => x.Value.Clear());
        prefabs.Clear();
    }
    public void WarmPool(PoolBehaviour pb, int size = 20)
    {
        if (prefabs.ContainsKey(pb))
        {
            Debug.LogWarning($"이미 생성한 프리팹 {pb.name}");
            return;
        }
        var pool = new ObjectPool<PoolBehaviour>
        (
            createFunc: () =>
            {
                PoolBehaviour p = Instantiate(pb);
                p.poolManager = this;
                return p;
            },
            actionOnGet: (v) =>
            {
                v.gameObject.SetActive(true);
            },
            actionOnRelease: (v) =>
            {
                v.gameObject.SetActive(false);
            },
            actionOnDestroy: (v) =>
            {
                Destroy(v.gameObject);
            },
            maxSize: size
        );
        prefabs.Add(pb, pool);
    }
    public PoolBehaviour Spawn(PoolBehaviour pb, Vector3 pos, Quaternion rot, Transform parent = null, int size = 20)
    {
        if(prefabs.ContainsKey(pb) == false)
        {
            WarmPool(pb, size);
        }
        var pool = prefabs[pb];
        PoolBehaviour clone = pool.Get();
        clone.transform.SetPositionAndRotation(pos,rot);
        if(parent == null) clone.transform.SetParent(transform);
        else clone.transform.SetParent(parent);
        clone.transform.position = pos;
        instances.Add(clone, pool);
        return clone;
    }
    public void Despawn(PoolBehaviour pb)
    {
        if(instances.ContainsKey(pb) == false)
        {
            return;
        }
        instances[pb].Release(pb);
        instances.Remove(pb);
    }
}
