using System.Collections;
using UnityEngine;
public class ElectricCableType2 : MonoBehaviour, ISavable
{
    #region ISavable Complement
    Transform ISavable.transform => transform;
    bool ISavable.IsComplete { get { return isComplete; } set { isComplete = value; } }
    bool isComplete;
    bool ISavable.CanReplay => true;
    int ISavable.ReplayWaitTimeSecond => 103680;
    public void SetCompletedImmediately()
    {
        isComplete = true;
        col.enabled = false;
        Rigidbody2D[] _rbs = GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in _rbs)
        {
            if (rb.transform.name == "ElectricCableB") rb.gameObject.SetActive(false);
        }
    }
    Collider2D col;
    #endregion
    [SerializeField] private HingeJoint2D firstBreakJoint;
    [SerializeField] private HingeJoint2D secondBreakJoint;
    [SerializeField] private HingeJoint2D centerJoint;
    float secondBreakDelay = 1f;
    void OnEnable()
    {
        if (col == null) TryGetComponent(out col);
        GameManager.I.onHit += HitHandler;
    }
    void OnDisable()
    {
        GameManager.I.onHit -= HitHandler;
    }
    void HitHandler(HitData hitData)
    {
        if (isBreak) return;
        if (hitData.target != transform) return;
        Debug.Log("이 오브젝트가 공격당함");
        if (!isBreak)
        {
            isBreak = true;
            StartCoroutine(nameof(BreakJoint));
        }
    }
    bool isBreak;
    [SerializeField] DropItem dropItem;
    IEnumerator BreakJoint()
    {
        yield return null;
        AudioManager.I.PlaySFX("Hit8bit1", transform.position);
        if (firstBreakJoint != null)
        {
            firstBreakJoint.enabled = false;
            Debug.Log(firstBreakJoint);
        }
        yield return new WaitForSeconds(secondBreakDelay);
        isComplete = true;
        col.enabled = false;
        AudioManager.I.PlaySFX("HitLittle", transform.position, null, spatialBlend: 0.3f);
        if (secondBreakJoint != null)
        {
            secondBreakJoint.enabled = false;
            Debug.Log(secondBreakJoint);
        }
        yield return new WaitForSeconds(secondBreakDelay * 0.3f);
        AudioManager.I.PlaySFX("Tick1");
        DropItem _dropItem = Instantiate(dropItem);
        _dropItem.transform.position = centerJoint.transform.position;
        Rigidbody2D rigidbody2D = _dropItem.GetComponentInChildren<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, Random.Range(5f, 15f)) * Vector2.up;
            if (Random.value <= 0.5f) dir.x = -dir.x;
            rigidbody2D.AddForce(Random.Range(4f, 8.5f) * dir, ForceMode2D.Impulse);
        }
        Rigidbody2D[] _rbs = GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in _rbs)
        {
            if (rb.transform.name == "ElectricCableB") rb.gameObject.SetActive(false);
        }
        SetCompletedImmediately();
    }



}
