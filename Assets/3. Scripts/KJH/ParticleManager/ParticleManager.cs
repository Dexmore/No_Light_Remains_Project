using System.Collections.Generic;
using UnityEngine;
public class ParticleManager : SingletonBehaviour<ParticleManager>
{
    protected override bool IsDontDestroy() => true;
    [SerializeField] List<Particle> particleList = new List<Particle>();
    Transform canvas;
    public Particle PlayParticle(string Name, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        int find = -1;
        for (int i = 0; i < particleList.Count; i++)
        {
            if (Name == particleList[i].name)
            {
                find = i;
                break;
            }
        }
        if (find == -1) return null;
        if (parent == null) parent = transform;
        PoolBehaviour pb = particleList[find];
        PoolBehaviour clone = PoolManager.I?.Spawn(pb, pos, Quaternion.identity, canvas);
        Particle _clone = clone as Particle;
        _clone.transform.position = pos;
        _clone.transform.rotation = rot;
        _clone.transform.SetParent(parent);
        _clone.Play();
        return _clone;
    }
    public Transform PlayTextParticle(string Name, string text, Vector3 pos, Vector3 scale, Color color, float time = 1f, Transform parent = null)
    {

        //구현 예정. 주로 데미지 이펙트라던가. Miss ! Avoid ! 같은 파티클을 만들때 쓸것임 
        return null;
    }

    public RectTransform PlayEffectSprite(string Name, Vector2 screenPosition, Quaternion rot, int FPS)
    {
        // 구현 예정. 주로 메인메뉴나 아이템창 같은 UI 캔버스에서 스프라이트로 된것을 재생하려고 만듬. 
        return null;
    }



}