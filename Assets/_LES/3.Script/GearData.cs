using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

[CreateAssetMenu(fileName = "NewGearData", menuName = "Project Data/Gear")]
public class GearData : ScriptableObject
{
    public LocalizedString gearName;
    public Sprite gearIcon;
    
    [Header("설명 및 효과")]
    [Tooltip("작성법: 기본 효과 텍스트 || 강화 후 효과 텍스트")]
    public LocalizedString gearDescription;

    [Header("Runtime Strings")]
    [System.NonSerialized] public string localizedName;
    
    // [분리된 텍스트]
    [System.NonSerialized] public string localizedNormalEffect;   // || 앞부분 (0강 효과)
    [System.NonSerialized] public string localizedEnhancedEffect; // || 뒷부분 (1강 효과)
    
    [Range(1, 3)]
    public int cost;
    public bool isEquipped;
    public bool isNew;

    // 1회 강화이므로 배열 대신 단일 설정 사용 가능하지만, 
    // 기존 구조 유지를 위해 리스트 형태는 유지하되 매니저에서 0번만 씁니다.
    public EnhancementManager.LevelInfo[] specificEnhancementSettings;

    public void LoadStrings()
    {
        // 1. 이름 로드
        if (!gearName.IsEmpty)
        {
            gearName.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        // 2. 설명 로드 및 분리 (Split)
        if (!gearDescription.IsEmpty)
        {
            gearDescription.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    string fullText = handle.Result;

                    // "||" 구분자 기준 분리
                    string[] parts = fullText.Split(new string[] { "||" }, StringSplitOptions.None);

                    if (parts.Length > 1)
                    {
                        localizedNormalEffect = parts[0].Trim();
                        localizedEnhancedEffect = parts[1].Trim();
                    }
                    else
                    {
                        // 구분자가 없으면 그냥 기본 효과에 다 넣고, 강화 효과는 비움
                        localizedNormalEffect = fullText;
                        localizedEnhancedEffect = "강화 효과 없음"; 
                    }
                }
            };
        }
    }
}