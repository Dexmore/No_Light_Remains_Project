using UnityEngine;
public class PlayerCamera : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    void FixedUpdate()
    {
        if (target != null)
        {
            // 플레이어의 위치에 오프셋을 더해 카메라가 원하는 위치를 계산
            Vector3 desiredPosition = target.position + offset;
            // 현재 카메라 위치를 목표 위치로 부드럽게 이동
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        }
    }
}