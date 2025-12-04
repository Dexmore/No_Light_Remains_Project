using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class PopupControl : MonoBehaviour
{
    [SerializeField] private InputActionReference cancelAction;
    GameObject canvasGo;
    GameObject[] allPopups;
    List<bool> isOpens = new List<bool>();
    [ReadOnlyInspector][SerializeField] int openPopCount;
    void Awake()
    {
        canvasGo = transform.Find("PopupCanvas").gameObject;
        canvasGo.SetActive(false);
        allPopups = new GameObject[canvasGo.transform.childCount - 1];
        isOpens.Clear();
        for (int i = 0; i < allPopups.Length; i++)
        {
            allPopups[i] = transform.Find("PopupCanvas").GetChild(i + 1).gameObject;
            allPopups[i].SetActive(false);
            isOpens.Add(false);
        }
        openPopCount = 0;
    }
    void OnEnable()
    {
        cancelAction.action.performed += InputESC;
        GameManager.I.onHitAfter += HandleHit;
    }
    void OnDisable()
    {
        cancelAction.action.performed -= InputESC;
        GameManager.I.onHitAfter -= HandleHit;
    }
    void HandleHit(HitData hitData)
    {
        if (hitData.target.Root().name != "Player") return;
        if (allPopups[1].activeSelf)
        {
            ClosePop(1);
        }
    }
    float coolTime = 0;
    void InputESC(InputAction.CallbackContext callbackContext)
    {
        if (Time.time - coolTime < 1.2f) return;
        coolTime = Time.time;
        for (int i = allPopups.Length - 1; i >= 0; i--)
        {
            if (allPopups[i].activeSelf)
            {
                ClosePop(i);
                return;
            }
        }
    }
    public void OpenPop(int index)
    {
        if (allPopups[index].activeSelf) return;
        canvasGo.SetActive(true);
        allPopups[index].SetActive(true);
        AudioManager.I.PlaySFX("OpenPopup");
        DOTween.Kill(allPopups[index].transform);
        allPopups[index].transform.localScale = 0.7f * Vector3.one;
        allPopups[index].transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        isOpens[index] = true;
        openPopCount++;
        GameManager.I.isOpenPop = true;
    }
    public void ClosePop(int index)
    {
        if (!allPopups[index].activeSelf) return;
        DOTween.Kill(allPopups[index].transform);
        AudioManager.I.PlaySFX("UIClick");
        allPopups[index].SetActive(false);
        isOpens[index] = false;
        int find = isOpens.FindIndex(x => x == true);
        if (find == -1)
        {
            canvasGo.SetActive(false);
            DOVirtual.DelayedCall(0.4f,() => GameManager.I.isOpenPop = false).Play();
        }
        openPopCount--;
        if (index == 0)
        {
            DBManager.I.GetComponent<LoginUI>().canvasGroup.enabled = true;
        }
    }
#if UNITY_EDITOR
    public int testIndex;
    [Button]
    public void TestOpenPopup()
    {
        OpenPop(testIndex);
    }
#endif




}
