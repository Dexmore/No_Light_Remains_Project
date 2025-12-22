using UnityEngine;
using UnityEditor;
public class SelectObjectsChangeName : EditorWindow
{
    private string baseName = "NewName"; // 사용자가 입력할 이름
    private string namingRule = "(n)";   // 규칙 (현재는 (n)을 숫자로 치환)
    private int startNumber = 0;         // 시작 번호
    [MenuItem("MyMenu/SelectObjectsChangeName")]
    public static void ShowWindow()
    {
        // 윈도우 생성 및 초기 사이즈 설정
        SelectObjectsChangeName window = GetWindow<SelectObjectsChangeName>("이름 일괄 변경");
        window.minSize = new Vector2(300, 150);
    }
    void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("이름 규칙 설정", EditorStyles.boldLabel);
        baseName = EditorGUILayout.TextField("Name", baseName);
        namingRule = EditorGUILayout.TextField("Rule (n = 숫자)", namingRule);
        startNumber = EditorGUILayout.IntField("Start Number", startNumber);
        GUILayout.Space(20);
        GUI.backgroundColor = Color.cyan; // 버튼 색상 강조
        if (GUILayout.Button("Change Names", GUILayout.Height(30)))
        {
            ChangeNames();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox($"결과 예시: {baseName}{namingRule.Replace("n", startNumber.ToString())}", MessageType.Info);
    }
    void ChangeNames()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("경고", "선택된 게임 오브젝트가 없습니다!", "확인");
            return;
        }
        // Undo 그룹화 (한 번의 Ctrl+Z로 모두 되돌리기 위함)
        Undo.RecordObjects(selectedObjects, "Bulk Name Change");
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            // 규칙의 'n' 문자를 실제 숫자로 치환
            string currentNumber = (startNumber + i).ToString();
            string suffix = namingRule.Replace("n", currentNumber);
            
            selectedObjects[i].name = baseName + suffix;
        }
        //Debug.Log($"{selectedObjects.Length}개의 오브젝트 이름을 변경했습니다.");
    }
}