using UnityEngine;
using System.Collections;

public class RightComplete : MonoBehaviour
{
    ISavable savable;

    void Awake()
    {
        TryGetComponent(out savable);
    }

    IEnumerator Start()
    {
        yield return YieldInstructionCache.WaitForSeconds(1f);
        if(savable.IsComplete) yield break;

        PlayerControl playerControl = FindAnyObjectByType<PlayerControl>();
        // 플레이어가 오른쪽에서 씬 이동해온 경우
        if (playerControl.transform.position.x > transform.position.x + 3)
        {
            savable.SetCompletedImmediately();
            yield break;
        }

    }

}
