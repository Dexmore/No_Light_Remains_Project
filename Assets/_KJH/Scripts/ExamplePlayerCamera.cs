using UnityEngine;
public class ExamplePlayerCamera : MonoBehaviour
{
    public Transform target;
    void LateUpdate()
    {
        transform.position = Vector3.Slerp(transform.position, target.position, 1.5f * Time.deltaTime);
    }
}
