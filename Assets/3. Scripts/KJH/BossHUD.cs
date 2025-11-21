using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossHUD : MonoBehaviour
{
    [ReadOnlyInspector][SerializeField] MonsterControl _target;
    Transform canvas;
    TMP_Text textName;
    void Awake()
    {
        canvas = transform.GetChild(0);
        transform.Find("Canvas/Bottom/Text(BossName)").TryGetComponent(out textName);
    }
    public void SetTarget(MonsterControl target)
    {
        if(target == null)
        {
            canvas.gameObject.SetActive(false);
        }
        if (_target != target)
        {
            _target = target;
            canvas.gameObject.SetActive(true);
            textName.text = target.data.Name;


        }
    }




}
