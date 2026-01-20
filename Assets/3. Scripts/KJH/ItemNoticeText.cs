using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween 네임스페이스 필요

public class ItemNoticeText : MonoBehaviour
{
    private TMP_Text textMesh;
    private CanvasGroup canvasGroup;

    public void Setup(string message)
    {
        textMesh = GetComponent<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        textMesh.text = message;

        // --- 연출 시작 ---
        // 1. 시작 위치 설정 (화면 오른쪽 밖)
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 targetPos = rect.anchoredPosition; // 레이아웃 그룹에 의해 결정된 위치
        rect.anchoredPosition = new Vector2(targetPos.x + 500f, targetPos.y); // 오른쪽으로 500 유닛 밀기

        // 2. 안으로 들어오는 트윈
        rect.DOAnchorPosX(targetPos.x, 1f).SetEase(Ease.OutBack).SetLink(gameObject);

        // 3. 2초 대기 후 페이드 아웃하며 파괴
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(2f); // 2초 유지
        seq.Append(canvasGroup.DOFade(0f, 0.5f).SetLink(gameObject)); // 0.5초간 페이드 아웃
        seq.OnComplete(() => Destroy(gameObject)); // 완료 후 오브젝트 삭제
        seq.SetLink(gameObject);
    }
}