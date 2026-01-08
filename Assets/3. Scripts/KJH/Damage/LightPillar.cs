using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가
using DG.Tweening;    // DOTween 사용을 위해 추가

public class LightPillar : MonoBehaviour
{
    public float waitDuration = 0.7f;
    public float bombDuration = 1.3f;
    private Collider2D col;
    private Animator anim;

    [Header("UI References")]
    public GameObject waitCircleCanvas;
    CanvasGroup canvasGroup;
    public Image fillImage;   // 차오르는 Circle Fill 이미지
    public Transform pressFill; // 스케일 애니메이션용
    public Transform pressRing; // 스케일 애니메이션용

    private List<Collider2D> attackedColliders = new List<Collider2D>();
    SFX sfx1;
    void Awake()
    {
        TryGetComponent(out col);
        TryGetComponent(out anim);
    }

    void OnEnable()
    {
        attackedColliders.Clear();
        col.enabled = false;
        // UI 초기화
        if (waitCircleCanvas != null)
        {
            waitCircleCanvas.SetActive(true);
            fillImage.fillAmount = 0f;
            canvasGroup = waitCircleCanvas.GetComponent<CanvasGroup>();
            // 기존 런턴 로직의 DOTween 재활용
            DOTween.Kill(pressFill);
            DOTween.Kill(pressRing);
            DOTween.Kill(canvasGroup);
            pressFill.localScale = Vector3.zero;
            pressRing.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(0.23f, waitDuration).SetEase(Ease.InSine).SetLink(gameObject);
            pressFill.DOScale(1f, 0.2f).SetLink(gameObject);
            pressRing.DOScale(1f, 0.2f).SetLink(gameObject);
        }
        StartCoroutine(nameof(OnEnableAfter));
    }
    void OnDestroy()
    {
        sfx1?.Despawn();
        sfx1 = null;
    }

    IEnumerator OnEnableAfter()
    {
        // 1. Animator에서 Wait 재생
        anim.Play("Wait");
        sfx1 = AudioManager.I.PlaySFX("LightPilarWait", transform.position, null, 0.9f);
        yield return null;
        yield return null;
        // 2. waitDuration 동안 Fill 채우기
        float elapsed = 0f;
        while (elapsed < waitDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / waitDuration);

            if (fillImage != null)
                fillImage.fillAmount = progress;

            yield return null;
        }
        // 3. UI 끄기
        if (waitCircleCanvas != null) waitCircleCanvas.SetActive(false);
        // 4. Animator에서 Bomb 재생 및 공격 판정 활성화
        sfx1?.Despawn();
        sfx1 = null;
        anim.Play("Bomb");
        AudioManager.I.PlaySFX("LightPilarBomb", transform.position, null, 0.9f);
        // 5. 폭발 후 일정 시간 뒤에 오브젝트 비활성화 (필요 시)
        yield return new WaitForSeconds(bombDuration);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        if (!attackedColliders.Contains(coll))
        {
            attackedColliders.Add(coll);
            Damage(coll);

            Rigidbody2D rb = coll.GetComponentInChildren<Rigidbody2D>() ?? coll.GetComponentInParent<Rigidbody2D>();
            if (rb)
            {
                Vector2 pos = col.bounds.center;
                Vector2 direction = ((Vector2)coll.transform.position + 1.2f * Vector2.up) - pos;
                direction.y = 0f;
                rb.AddForce(7f * direction.normalized + 0.5f * Vector2.up, ForceMode2D.Impulse);
            }
        }
    }

    void Damage(Collider2D coll)
    {
        Vector2 hitPoint = 0.7f * coll.ClosestPoint(transform.position) + 0.3f * (Vector2)coll.transform.position + Vector2.up;
        Vector2 adjustPos = col.bounds.center;
        hitPoint = 0.3f * adjustPos + 0.7f * hitPoint;
        hitPoint.y = 0.3f * hitPoint.y + 0.7f * coll.transform.position.y;

        float damage = 10f;
        HitData hitData = new HitData
        {
            attackName = "LightPillar",
            attacker = transform,
            target = coll.transform,
            damage = Random.Range(0.9f, 1.1f) * damage,
            hitPoint = hitPoint,
            particleNames = new string[] { "ElectricHit1" },
            staggerType = HitData.StaggerType.Large,
            attackType = HitData.AttackType.Trap
        };
        hitData.isCannotParry = true;
        GameManager.I.onHit.Invoke(hitData);
    }
}