using UnityEngine;
using DG.Tweening;
public class GearTutorial : MonoBehaviour
{
    private float moveDistance = 0.25f;
    private float duration = 1.1f;
    Tween tween;
    void Start()
    {
        tween?.Kill();
        tween = transform.DOLocalMoveY(transform.localPosition.y + moveDistance, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Play();
    }
    void OnDisable()
    {
        tween?.Kill();
    }
    public void Complete()
    {
        gameObject.SetActive(false);
    }
    public void CompleteImmediately()
    {
        gameObject.SetActive(false);
    }


    
}
