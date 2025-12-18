using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector2 xBound = new Vector2(-999, 999);
    public Vector2 yBound = new Vector2(-999, 999);
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    IEnumerator Start()
    {
        isReady = false;
        var ucd = Camera.main.GetUniversalAdditionalCameraData();
        yield return new WaitForSeconds(1f);
        float startTime = Time.time;
        // 시작후 페이드 약간 동안은 즉시 이동 처리
        while (Time.time - startTime < 5f)
        {
            yield return null;
            if (!GameManager.I.isSceneWaiting) break;
            transform.position = target.position + offset;
        }
        isReady = true;
    }
    bool isReady;
    void FixedUpdate()
    {
        if (!isReady) return;
        if (target != null)
        {
            // 플레이어의 위치에 오프셋을 더해 카메라가 원하는 위치를 계산
            Vector3 desiredPosition = target.position + offset;

            if(desiredPosition.x < xBound.x) desiredPosition.x = xBound.x;
            if(desiredPosition.x > xBound.y) desiredPosition.x = xBound.y;
            if(desiredPosition.y < yBound.x) desiredPosition.y = yBound.x;
            if(desiredPosition.y > yBound.y) desiredPosition.y = yBound.y;

            // 현재 카메라 위치를 목표 위치로 부드럽게 이동
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        }
    }
}