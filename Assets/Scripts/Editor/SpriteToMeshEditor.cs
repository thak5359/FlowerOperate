// SpriteToMeshEditor.cs (반드시 Editor 폴더 안에 넣으세요!)
using UnityEngine;
using UnityEditor; // 에디터 기능을 쓰기 위해 필수

[CustomEditor(typeof(SpriteToMesh))] // 어떤 스크립트를 꾸밀지 지정
public class SpriteToMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 내용(변수 리스트 등)을 먼저 그립니다.
        base.OnInspectorGUI();

        // 대상 스크립트 참조 가져오기
        SpriteToMesh script = (SpriteToMesh)target;

        GUILayout.Space(10); // 여백 주기
        GUI.backgroundColor = Color.green; // 버튼 색상 변경 (기분 전환용!)

        // 버튼을 만들고, 클릭했을 때 실행할 로직 작성
        if (GUILayout.Button("자동 컴포넌트 세팅", GUILayout.Height(30)))
        {
            // 실제 세팅 함수 호출
            script.SetupComponents();

            // 변경 사항을 저장 (Ctrl+S를 누르지 않아도 반영되게 함)
            EditorUtility.SetDirty(script);
        }
    }
}