using UnityEngine;
public class CreatureStatus : MonoBehaviour
{
    CreatureControl control;
    CreatureData data;
    void Awake()
    {
        TryGetComponent(out control);
    }
    void OnEnable()
    {
        data = control.data;
    }

}
