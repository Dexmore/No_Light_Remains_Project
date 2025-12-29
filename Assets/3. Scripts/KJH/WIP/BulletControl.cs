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
        public float damageMultiplier;
        public int count;
        public Vector2 heuristic;
    }

    public async UniTask PlayBullet(List<BulletPatern> bulletPaterns, Transform attacker, Transform target, CancellationToken token, float damage = 50)
    {
        float elapsed = 0f;
        Vector2 lineDirection = target.position - attacker.position;
        lineDirection.Normalize();
        for (int i = 0; i < bulletPaterns.Count; i++)
        {
            float targetTime = bulletPaterns[i].startTime;

            while (true)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield(cancellationToken: token);
                if (elapsed >= targetTime) break;
            }

            for (int k = 0; k < bulletPaterns[i].count; k++)
            {
                if (bulletPaterns[i].bullet.bulletType == Bullet.BulletType.Line)
                {
                    PoolBehaviour pb = PoolManager.I.Spawn(bulletPaterns[i].bullet, attacker.position + Vector3.up, Quaternion.identity);
                    Bullet bullet = pb as Bullet;
                    bullet.damage = bulletPaterns[i].damageMultiplier * damage;
                    bullet.rb.AddForce(bulletPaterns[i].force * lineDirection, ForceMode2D.Impulse);
                }

            }
            await UniTask.Yield(cancellationToken: token);



        }
        return;
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
