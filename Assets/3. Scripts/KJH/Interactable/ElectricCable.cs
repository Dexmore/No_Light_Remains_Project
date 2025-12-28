using System.Linq;
using System.Collections;
using UnityEngine;
public class ElectricCable : MonoBehaviour
{
    private Rigidbody2D[] rbs;
    private bool useIdleSway = true;
    private float swayForce = 0.7f;
    private float swayInterval = 1.5f;
    private float impactForce = 3.0f;
    void Awake()
    {
        Rigidbody2D[] _rbs = GetComponentsInChildren<Rigidbody2D>();
        rbs = _rbs.Where(x => !x.transform.name.Contains("Anchor")).ToArray();
    }
    void Start()
    {
        if (useIdleSway)
        {
            // 일정 시간마다 미세하게 흔드는 루틴 시작
            StartCoroutine(IdleSwayRoutine());
        }
    }
    IEnumerator IdleSwayRoutine()
    {
        while (true)
        {
            Vector2 totalVelo = Vector2.zero;
            foreach (Rigidbody2D rb in rbs)
            {
                totalVelo += rb.linearVelocity;
            }
            if (totalVelo.magnitude < 0.1f)
            {
                float randomDirection = Random.Range(-1f, 1f);
                foreach (Rigidbody2D rb in rbs)
                {
                    rb.AddForce(new Vector2(randomDirection * swayForce, 0), ForceMode2D.Impulse);
                }
            }
            yield return new WaitForSeconds(swayInterval + Random.Range(0f, 1f));
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            Vector2 pushDir = (transform.position - collision.transform.position).normalized;
            foreach (Rigidbody2D rb in rbs)
            {
                rb.AddForce(new Vector2(pushDir.x * impactForce, 0.5f), ForceMode2D.Impulse);
            }
        }
    }
}