
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

public class UIAppearHoverZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CanvasGroup appearGroup;
    Tween currentTween;
    private Sequence exitSequence;
    void Awake()
    {
        appearGroup.interactable = false;
        appearGroup.blocksRaycasts = false;
        appearGroup.alpha = 0f;
    }
    bool isZoneEnter;
    bool isGroupOver;
    public void OnPointerEnter(PointerEventData eventData)
    {
        isZoneEnter = true;
        if (!isRaycastLoop)
        {
            isRaycastLoop = true;
            StartCoroutine(nameof(RaycastLoop));
            currentTween?.Kill(true);
            currentTween = appearGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                appearGroup.blocksRaycasts = true;
                appearGroup.interactable = true;
            });
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isZoneEnter = false;
    }
    bool isRaycastLoop;
    List<RaycastResult> buffers = new List<RaycastResult>();
    IEnumerator RaycastLoop()
    {
        while (true)
        {
            yield return YieldInstructionCache.WaitForSeconds(0.2f);

            // Old Input System :
            // PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

            // New Input System :
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            if (Mouse.current == null) yield break;
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };

            buffers.Clear();
            EventSystem.current.RaycastAll(eventData, buffers);
            isGroupOver = false;
            foreach (RaycastResult result in buffers)
            {
                if (HasParent(result.gameObject.transform, appearGroup.transform))
                {
                    isGroupOver = true;
                    break;
                }
            }
            //
            if (!isGroupOver && !isZoneEnter)
            {
                yield return YieldInstructionCache.WaitForSeconds(0.5f);
                if (!isGroupOver && !isZoneEnter)
                {
                    currentTween?.Kill();
                    exitSequence?.Kill();
                    exitSequence = DOTween.Sequence()
                        .AppendInterval(0.1f)
                        .AppendCallback(() =>
                        {
                            appearGroup.blocksRaycasts = false;
                            appearGroup.interactable = false;
                        })
                        .Append(appearGroup.DOFade(0f, 1.3f).SetEase(Ease.OutQuad))
                        .OnKill(() => exitSequence = null);
                    currentTween = exitSequence;
                }
                isRaycastLoop = false;
                yield break;
            }
        }
    }

    public bool HasParent(Transform child, Transform parent)
    {
        if (parent == null) return false;
        Transform current = child.parent;
        while (current != null)
        {
            if (current == parent)
                return true;
            current = current.parent;
        }
        return false;
    }















}