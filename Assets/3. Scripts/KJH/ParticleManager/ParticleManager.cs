using System.Collections.Generic;
using UnityEngine;
public class ParticleManager : SingletonBehaviour<ParticleManager>
{
    protected override bool IsDontDestroy() => true;
    [SerializeField] List<Particle> particleList = new List<Particle>();
    [SerializeField] List<UIParticle> uiParticleList = new List<UIParticle>();
    Transform canvas;
    protected override void Awake()
    {
        base.Awake();
        canvas = transform.GetChild(0);
    }
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
        PoolBehaviour clone = PoolManager.I?.Spawn(pb, pos, Quaternion.identity, parent);
        Particle _clone = clone as Particle;
        _clone.transform.position = pos;
        _clone.transform.rotation = rot;
        _clone.transform.SetParent(parent);
        _clone.Play();
        return _clone;
    }
    public UIParticle PlayUIParticle(string Name, Vector2 screenPosition_on_1920x1080, Quaternion rot)
    {
        int find = -1;
        for (int i = 0; i < uiParticleList.Count; i++)
        {
            if (Name == uiParticleList[i].name)
            {
                find = i;
                break;
            }
        }
        if (find == -1) return null;
        PoolBehaviour pb = uiParticleList[find];
        PoolBehaviour clone = PoolManager.I?.Spawn(pb, Vector2.zero, Quaternion.identity, canvas);
        UIParticle _clone = clone as UIParticle;
        _clone.transform.localPosition = Vector3.zero;
        _clone.transform.localScale = Vector3.one;
        RectTransform rect = _clone.transform as RectTransform;
        rect.anchoredPosition = screenPosition_on_1920x1080;
        _clone.transform.rotation = rot;
        _clone.transform.SetParent(canvas);
        _clone.Play();
        return _clone;
    }
    [SerializeField] TextEffect damageText;
    [SerializeField] TextEffect playerNoticeText;
    public enum TextType
    {
        Damage,
        CiriticalDamage,
        PlayerNotice,
    }
    public TextEffect PlayText(string text, Vector3 pos, TextType type)
    {
        if (type == TextType.PlayerNotice)
        {
            TextEffect _clone = PoolManager.I?.Spawn(playerNoticeText, pos, Quaternion.identity, canvas) as TextEffect;
            _clone.txt.text = text;
            _clone.transform.position = pos + 0.2f * Vector3.up;
            _clone.transform.SetParent(transform);
            _clone.Play();
            return _clone;
        }
        else if (type == TextType.Damage)
        {
            TextEffect _clone = PoolManager.I?.Spawn(damageText, pos, Quaternion.identity, canvas) as TextEffect;
            _clone.txt.text = text;
            _clone.transform.position = pos + new Vector3(Random.Range(0f, 0.2f), Random.Range(0.7f, 0.9f), 0f);
            _clone.transform.SetParent(transform);
            _clone.Play();
            return _clone;
        }
        return null;
    }




}