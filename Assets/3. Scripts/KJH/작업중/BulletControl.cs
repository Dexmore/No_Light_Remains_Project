using System.Collections.Generic;
using UnityEngine;
public class BulletControl : MonoBehaviour
{
    List<Bullet> currSpawnedList = new List<Bullet>();
    public Bullet SpawnBullet(Bullet bullet, Vector3 pos, Vector3 speed, Quaternion rot)
    {
        PoolBehaviour pb = bullet;
        PoolBehaviour clone = PoolManager.I?.Spawn(pb, pos, Quaternion.identity, this.transform);
        Bullet _clone = clone as Bullet;
        _clone.transform.position = pos;
        _clone.transform.rotation = rot;
        _clone.transform.SetParent(this.transform);
        currSpawnedList.Add(_clone);
        return _clone;
    }
    void OnEnable()
    {
        GameManager.I.onSceneChange += SceneChangeHandler;
    }
    void OnDisable()
    {
        GameManager.I.onSceneChange -= SceneChangeHandler;
    }
    void SceneChangeHandler()
    {
        for (int i = 0; i < currSpawnedList.Count; i++)
        {
            if (currSpawnedList[i] != null && currSpawnedList[i].gameObject.activeSelf)
            {
                currSpawnedList[i].Despawn();
            }
        }
        currSpawnedList.Clear();
    }



    


}
