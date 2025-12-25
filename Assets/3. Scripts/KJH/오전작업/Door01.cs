using UnityEngine;
using DG.Tweening;
public class Door01 : MonoBehaviour
{

    Sequence doorSeq;
    float xRange = 0.09f;
    float duration = 0.053f;
    public void Lock()
    {
        doorSeq?.Kill();
        doorSeq = DOTween.Sequence();
        doorSeq.Append(transform.DOBlendableLocalMoveBy(new Vector3(xRange, 0, 0), duration)) 
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(-xRange * 2, 0, 0), duration))
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(xRange, 0, 0), duration))
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(xRange, 0, 0), duration)) 
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(-xRange * 2, 0, 0), duration))
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(xRange, 0, 0), duration))
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(-xRange * 2, 0, 0), duration))
               .Append(transform.DOBlendableLocalMoveBy(new Vector3(xRange, 0, 0), duration));
        AudioManager.I.PlaySFX("Locked");
    }


}
