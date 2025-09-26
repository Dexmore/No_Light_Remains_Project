using System.Collections;
using UnityEngine;
public class LogoControl : MonoBehaviour
{
    public GameObject logo;
    public Vector2[] targetPositions;
    IEnumerator Start()
    {
        yield return null;
        #region Logo Animation
        
        #endregion
        yield return YieldInstructionCache.WaitForSeconds(0.5f);
        GameManager.I.FadeOut(1.2f);
        yield return YieldInstructionCache.WaitForSeconds(1.2f);
        GameManager.I.LoadSceneAsync(1);
    }
    
}
