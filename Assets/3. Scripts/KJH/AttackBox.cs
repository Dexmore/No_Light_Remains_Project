using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(PolygonCollider2D))]
public class AttackBox : MonoBehaviour
{
    public Transform[] points;
    PolygonCollider2D pcoll2D;
    void Awake()
    {
        TryGetComponent(out pcoll2D);
    }
    void OnEnable()
    {
        pcoll2D.points = new Vector2[points.Length];
    }
    void Update()
    {
        Vector2[] buffer = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 vec = transform.InverseTransformPoint(points[i].position);
            buffer[i] = new Vector2(vec.x, vec.y);
        }
        pcoll2D.points = buffer;
    }
    public UnityAction<Collider> onTriggetStay = (x) => { };
    void OnTriggerStay(Collider other)
    {
        onTriggetStay.Invoke(other);
    }

    void OnDrawGizmos()
    {
        // points 배열이 비어있거나 null인 경우 오류 방지
        if (points == null || points.Length < 2)
        {
            return;
        }

        // 선의 색상을 녹색으로 설정합니다.
        Gizmos.color = Color.red;

        // 첫 번째 지점부터 마지막 지점까지 순차적으로 선을 그립니다.
        for (int i = 0; i < points.Length; i++)
        {
            // 현재 지점
            Vector3 currentPoint = points[i].position;
            // 다음 지점 (마지막 지점에서는 첫 번째 지점으로 돌아가 고리를 만듭니다)
            Vector3 nextPoint = (i == points.Length - 1) ? points[0].position : points[i + 1].position;

            // 두 지점 사이에 선을 그립니다.
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
    }


}
