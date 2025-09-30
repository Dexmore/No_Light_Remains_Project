using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;
public class MonsterSensor : MonoBehaviour
{
    void Awake()
    {
        TryGetComponent(out control);
        eye = transform.GetChild(0).Find("Eye");
    }
    #region UniTask Setting
    [HideInInspector] public CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
        PlayerSensor(cts.Token).Forget();
    }
    void OnDisable() => UniTaskCancel();
    void OnDestroy() => UniTaskCancel();
    void UniTaskCancel()
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
        cts = null;
    }
    #endregion
    Collider2D[] nearPlayers = new Collider2D[80];
    public Dictionary<Collider2D, float> visibilites = new Dictionary<Collider2D, float>();
    public Dictionary<Collider2D, float> memories = new Dictionary<Collider2D, float>();
    [ReadOnlyInspector] public float findRadius;
    [ReadOnlyInspector] public float closeRadius;
    MonsterControl control;
    Transform eye;
    async UniTask PlayerSensor(CancellationToken token)
    {
        await UniTask.Yield(token);
        findRadius = 15f * ((control.width + control.height) * 0.61f + 0.7f);
        closeRadius = 1.2f * (control.width * 0.61f + 0.7f);
        int count = 0;
        while (!token.IsCancellationRequested)
        {
            int timeDelta = Random.Range(150, 500);
            await UniTask.Delay(timeDelta, cancellationToken: token);
            count = Physics2D.OverlapCircleNonAlloc
            (
                (Vector2)transform.position,
                findRadius,
                nearPlayers,
                LayerMask.GetMask("Player")
            );
            // visibilites
            float minDist = 999;
            for (int i = 0; i < count; i++)
            {
                if (!visibilites.ContainsKey(nearPlayers[i]))
                {
                    float visibility = await CheckVisibility(nearPlayers[i], token);
                    visibilites.Add(nearPlayers[i], visibility);
                }
                else
                {
                    float visibility = await CheckVisibility(nearPlayers[i], token);
                    visibilites[nearPlayers[i]] = visibility;
                }
                if (!memories.ContainsKey(nearPlayers[i]))
                {
                    memories.Add(nearPlayers[i], Time.time);
                }
                else
                {
                    memories[nearPlayers[i]] = Time.time;
                }
                float dist = Vector2.Distance((Vector2)nearPlayers[i].transform.position, (Vector2)transform.position);
                if (minDist > dist)
                    minDist = dist;
                if (dist <= closeRadius)
                    if (!control.HasCondition(MonsterControl.Condition.ClosePlayer))
                        control.AddCondition(MonsterControl.Condition.ClosePlayer);
            }
            if (minDist > closeRadius)
                if (control.HasCondition(MonsterControl.Condition.ClosePlayer))
                    control.RemoveCondition(MonsterControl.Condition.ClosePlayer);
            Dictionary<Collider2D, float> copy = visibilites.ToDictionary(x => x.Key, x => x.Value);
            foreach (var element in visibilites)
            {
                int find = -1;
                for (int i = 0; i < count; i++)
                {
                    if (nearPlayers[i] == element.Key)
                    {
                        find = i;
                        break;
                    }
                }
                if (find == -1)
                {
                    copy.Remove(element.Key);
                }
            }
            visibilites = copy;
            // memories
            copy = memories.ToDictionary(x => x.Key, x => x.Value);
            foreach (var element in memories)
            {
                if (Time.time - element.Value > 20f)
                {
                    copy.Remove(element.Key);
                }
            }
            memories = copy;
            // Change Condition
            if (control.HasCondition(MonsterControl.Condition.FindPlayer))
            {
                if (memories.Count == 0)
                {
                    control.RemoveCondition(MonsterControl.Condition.FindPlayer);
                    control.AddCondition(MonsterControl.Condition.Peaceful);
                }
            }
            else
            {
                if (memories.Count > 0)
                    control.AddCondition(MonsterControl.Condition.FindPlayer);
            }
            // aggressive --> Fight 상태 변화
            if (control.HasCondition(MonsterControl.Condition.Peaceful))
            {
                bool find = false;
                foreach (var element in visibilites)
                {
                    if (element.Value > 0.75f)
                    {
                        float distance = Vector3.Distance(element.Key.transform.position, transform.position);
                        if (distance < 0.75f * findRadius)
                        {
                            float pow = Mathf.Pow(control.aggressive, 2.6f);
                            if (Random.value <= pow)
                            {
                                //Debug.Log(pow);
                                control.RemoveCondition(MonsterControl.Condition.Peaceful);
                            }
                        }
                    }
                }
            }

        }
    }
    // 이 아래부터 2D로 수정된 코드
    async UniTask<float> CheckVisibility(Collider2D target, CancellationToken token)
    {
        float sum = 0f;
        int rayCount = 0;
        Vector2 eyePos = (Vector2)eye.position;
        Vector2 targetCenter = (Vector2)target.bounds.center;
        Vector2 directionToTarget = targetCenter - eyePos;
        float sqrDistance = directionToTarget.sqrMagnitude;
        // 원래 Job System의 로직을 2D Raycast에 맞게 재구현
        if (sqrDistance < 4f * 4f)
            rayCount = 12;
        else if (sqrDistance < 6f * 6f)
            rayCount = 11;
        else if (sqrDistance < 8f * 8f)
            rayCount = 10;
        else if (sqrDistance < 10f * 10f)
            rayCount = 9;
        else if (sqrDistance < 12f * 12f)
            rayCount = 8;
        else if (sqrDistance < 15f * 15f)
            rayCount = 6;
        else if (sqrDistance < 18f * 18f)
            rayCount = 5;
        else if (sqrDistance < 23f * 23f)
            rayCount = 4;
        else if (sqrDistance < 33f * 33f)
            rayCount = 2;
        else
            rayCount = 1;
        // 2D 시야각 계산 (Y축을 기준으로 2D 평면에서 각도 계산)
        float angleToTarget = Vector2.Angle(transform.right, directionToTarget);
        if (angleToTarget > 120f)
        {
            return 0f;
        }
        for (int i = 0; i < rayCount; i++)
        {
            // 방향 벡터 계산 (랜덤성을 부여하여 흩뿌림)
            Vector2 randomDirection = Quaternion.Euler(0, 0, Random.Range(-10f, 10f)) * directionToTarget.normalized;
            float distance = directionToTarget.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(eyePos, randomDirection, distance, control.groundLayer);
            if (hit.collider == null)
            {
                sum++;
            }
            else if (hit.collider.gameObject.GetInstanceID() == target.gameObject.GetInstanceID())
            {
                sum++;
            }
        }
        if (rayCount == 0) return 0f;
        float result = sum / rayCount;
        return result;
    }
}