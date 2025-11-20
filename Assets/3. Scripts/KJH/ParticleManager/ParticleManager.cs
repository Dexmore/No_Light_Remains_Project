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
    [SerializeField] TextEffect damageText;
    public enum TextType
    {
        Damage,
        CiriticalDamage,
        Miss,
        Notice,
    }
    public TextEffect PlayText(string text, Vector3 pos, TextType type)
    {
        TextEffect _clone = PoolManager.I?.Spawn(damageText, pos, Quaternion.identity, canvas) as TextEffect;
        _clone.transform.position = pos;
        _clone.transform.SetParent(transform);
        _clone.Play();
        return null;
    }




}