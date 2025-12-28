using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class BulletControl : MonoBehaviour
{
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    protected virtual void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
    }
    protected virtual void OnDisable()
    {
        UniTaskCancel();
    }
    protected virtual void OnDestroy() { UniTaskCancel(); }
    void UniTaskCancel()
    {
        cts?.Cancel();
        try
        {
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion

    [System.Serializable]
    public struct BulletPatern
    {
        public Bullet bullet;
        public float startTime;
        public float force;
        public int count;
        public Vector2 heuristic;
    }

    public async UniTask PlayBullet(List<BulletPatern> bulletPaterns, Transform pivot, CancellationToken token)
    {
        float _time = Time.time;
        for (int i = 0; i < bulletPaterns.Count; i++)
        {
            float t = bulletPaterns[i].startTime;
            float sTime = Time.time - t;
            while (sTime < t)
            {
                sTime = Time.time - t;
                await UniTask.Yield(cancellationToken: token);
            }

            for (int k = 0; k < bulletPaterns[i].count; k++)
            {
                PoolBehaviour pb = PoolManager.I.Spawn(bulletPaterns[i].bullet, pivot.position, Quaternion.identity);
                Bullet bullet = pb as Bullet;
                bullet.rb.AddForce(bulletPaterns[i].force * Random.insideUnitCircle.normalized, ForceMode2D.Impulse);
            }


        }

    }











    // List<Bullet> currSpawnedList = new List<Bullet>();
    // public Bullet SpawnBullet(Bullet bullet, Vector3 pos, Vector3 speed, Quaternion rot)
    // {
    //     PoolBehaviour pb = bullet;
    //     PoolBehaviour clone = PoolManager.I?.Spawn(pb, pos, Quaternion.identity, this.transform);
    //     Bullet _clone = clone as Bullet;
    //     _clone.transform.position = pos;
    //     _clone.transform.rotation = rot;
    //     _clone.transform.SetParent(this.transform);
    //     currSpawnedList.Add(_clone);
    //     return _clone;
    // }
    // void OnEnable()
    // {
    //     GameManager.I.onSceneChange += SceneChangeHandler;
    // }
    // void OnDisable()
    // {
    //     GameManager.I.onSceneChange -= SceneChangeHandler;
    // }
    // void SceneChangeHandler()
    // {
    //     for (int i = 0; i < currSpawnedList.Count; i++)
    //     {
    //         if (currSpawnedList[i] != null && currSpawnedList[i].gameObject.activeSelf)
    //         {
    //             currSpawnedList[i].Despawn();
    //         }
    //     }
    //     currSpawnedList.Clear();
    // }






}
