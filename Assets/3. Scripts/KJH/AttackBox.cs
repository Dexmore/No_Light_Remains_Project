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
    public UnityAction<Collider2D> onTriggetStay2D = (x) => { };
    void OnTriggerStay2D(Collider2D collider2D)
    {
        onTriggetStay2D.Invoke(collider2D);
    }
    void OnDrawGizmos()
    {
        if (points == null || points.Length < 2) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 currentPoint = points[i].position;
            Vector3 nextPoint = (i == points.Length - 1) ? points[0].position : points[i + 1].position;
            Gizmos.DrawLine(currentPoint, nextPoint);
        }
    }
}
