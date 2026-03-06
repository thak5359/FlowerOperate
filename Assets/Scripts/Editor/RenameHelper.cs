using UnityEngine;
using UnityEditor;

public class RenameTool : EditorWindow
{
    private string baseName = "Slot"; // 기본 이름
    private int startNumber = 1;      // 시작 번호

    // 상단 메뉴 Tools -> Object Renamer를 누르면 창이 뜹니다.
    [MenuItem("Tools/Object Renamer")]
    public static void ShowWindow()
    {
        GetWindow<RenameTool>("Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("오브젝트 이름 일괄 변경", EditorStyles.boldLabel);

        // 여기서 유저가 원하는 이름을 입력받습니다.
        baseName = EditorGUILayout.TextField("앞에 붙을 이름", baseName);
        startNumber = EditorGUILayout.IntField("시작 번호", startNumber);

        if (GUILayout.Button("선택한 오브젝트 이름 바꾸기"))
        {
            RenameSelected();
        }
    }

    private void RenameSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다!");
            return;
        }

        // 하이어라키 순서(Sibling Index)대로 정렬하여 순서대로 번호를 매깁니다.
        System.Array.Sort(selectedObjects, (a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        // 실행 취소(Undo)가 가능하도록 기록합니다.
        Undo.RecordObjects(selectedObjects, "Batch Rename");

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].name = $"{baseName}_{startNumber + i}";
        }

        Debug.Log($"{selectedObjects.Length}개의 오브젝트 이름을 변경했습니다.");
    }
}