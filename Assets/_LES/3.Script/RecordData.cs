using UnityEngine;
using UnityEngine.Localization; // 필수
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "NewRecordData", menuName = "Project Data/Record")]
public class RecordData : ScriptableObject
{
    public enum Type { StoryRecord, MonsterRecord }
    public Type type;
    // [중요] [TextArea] 속성이 있으면 Localization UI가 깨집니다. 반드시 지워주세요.
    public LocalizedString recordTitle; 
    public LocalizedString recordContent; 
    
    [Header("Runtime Localized Strings")]
    // [추가] 실제 UI에서 사용할 번역된 문자열들입니다.
    // 인스펙터에 보일 필요가 없으므로 NonSerialized를 붙입니다.
    [System.NonSerialized] public string localizedName;
    [System.NonSerialized] public string localizedDescription;

    public bool isNew;

    [System.NonSerialized] public string localizedrecordTitle;
    [System.NonSerialized] public string localizedrecordContent;

    public void LoadStrings()
    {
        // 1. 이름 로드
        if (!recordTitle.IsEmpty)
        {
            recordTitle.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedName = handle.Result;
            };
        }

        // 2. 설명 로드
        if (!recordContent.IsEmpty)
        {
            recordContent.GetLocalizedStringAsync().Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    localizedDescription = handle.Result;
            };
        }
    }
}

// //이 스크립트는 게임 오브젝트에 붙이지 않습니다.
// //데이터 구조를 정의하는 순수 C# 클래스입니다.
// using UnityEngine;
// using UnityEngine.Localization;

// [CreateAssetMenu(fileName = "NewRecordData", menuName = "Project Data/Record")]
// public class RecordData : ScriptableObject
// {
//     public string recordTitle; //기록물 제목
    
//     //[TextArea(5, 15)] //Inspector에서 여러 줄로 입력받기 편하도록
//     public string recordContent; //기록물 상세 내용
    
//     public bool isNew; //새로 습득했는지 여부 (느낌표 띄울 때 사용)

//     [System.NonSerialized] public string localizedrecordTitle;
//     [System.NonSerialized] public string localizedrecordContent;
// }