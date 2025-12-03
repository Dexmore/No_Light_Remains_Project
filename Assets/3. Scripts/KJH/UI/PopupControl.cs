using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
public class PopupControl : MonoBehaviour
{
    GameObject canvasGo;
    GameObject[] allPopups;
    List<bool> isOpens = new List<bool>();
    [ReadOnlyInspector][SerializeField] int popCount;
    void Awake()
    {
        canvasGo = transform.Find("PopupCanvas").gameObject;
        allPopups = new GameObject[canvasGo.transform.childCount - 1];
        isOpens.Clear();
        for (int i = 0; i < allPopups.Length; i++)
        {
            allPopups[i] = transform.Find("PopupCanvas").GetChild(i + 1).gameObject;
            isOpens.Add(false);
        }
        popCount = 0;
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
        popCount++;
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
        }
        popCount--;
        if(index == 0)
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
