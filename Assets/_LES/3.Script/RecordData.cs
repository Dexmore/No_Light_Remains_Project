//이 스크립트는 게임 오브젝트에 붙이지 않습니다.
//데이터 구조를 정의하는 순수 C# 클래스입니다.
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecordData", menuName = "Project Data/Record")]
public class RecordData : ScriptableObject
{
    public string recordTitle; //기록물 제목
    
    [TextArea(5, 15)] //Inspector에서 여러 줄로 입력받기 편하도록
    public string recordContent; //기록물 상세 내용
    
    public bool isNew; //새로 습득했는지 여부 (느낌표 띄울 때 사용)
}