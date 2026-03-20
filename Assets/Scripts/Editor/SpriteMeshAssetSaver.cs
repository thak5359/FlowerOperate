using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering; // IndexFormat 설정을 위해 필요합니다.

public class SpriteMeshAssetSaver : EditorWindow
{
    private Sprite targetSprite;
    private string savePath = "Assets/Art/Materials";

    [MenuItem("Tools/Sprite Mesh Asset Saver")]
    public static void ShowWindow()
    {
        GetWindow<SpriteMeshAssetSaver>("Mesh Saver");
    }

    private void OnGUI()
    {
        GUILayout.Label("대용량 스프라이트 -> 메쉬 파일 저장 도구 (Pro)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        targetSprite = (Sprite)EditorGUILayout.ObjectField("대상 스프라이트", targetSprite, typeof(Sprite), false);
        savePath = EditorGUILayout.TextField("저장 폴더 경로", savePath);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("고해상도 메쉬 생성 및 저장", GUILayout.Height(40)))
        {
            if (targetSprite == null)
            {
                EditorUtility.DisplayDialog("경고", "파트너, 스프라이트를 먼저 넣어주세요!", "확인");
                return;
            }

            SaveSpriteMeshAsAsset();
        }
    }

    private void SaveSpriteMeshAsAsset()
    {
        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }

            Mesh mesh = new Mesh();
            mesh.name = targetSprite.name + "_GeneratedMesh";

            // ★ 핵심 수정 사항: 32비트 인덱스 포맷 설정 (65,535개 이상의 정점 지원)
            mesh.indexFormat = IndexFormat.UInt32;

            Vector2[] spriteVertices = targetSprite.vertices;

            // 정점 수 체크 및 사용자 알림
            if (spriteVertices.Length > 60000)
            {
                Debug.Log($"<color=orange>[Project JR]</color> 파트너, 정점 수가 {spriteVertices.Length}개입니다. 고해상도 처리를 시작할게요.");
            }

            Vector3[] meshVertices = new Vector3[spriteVertices.Length];
            for (int i = 0; i < spriteVertices.Length; i++)
            {
                meshVertices[i] = new Vector3(spriteVertices[i].x, spriteVertices[i].y, 0);
            }

            ushort[] spriteTriangles = targetSprite.triangles;
            int[] meshTriangles = new int[spriteTriangles.Length];
            for (int i = 0; i < spriteTriangles.Length; i++)
            {
                meshTriangles[i] = (int)spriteTriangles[i];
            }

            mesh.vertices = meshVertices;
            mesh.triangles = meshTriangles;
            mesh.uv = targetSprite.uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            string fullPath = Path.Combine(savePath, mesh.name + ".asset");
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            AssetDatabase.CreateAsset(mesh, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", $"고해상도 메쉬 저장 완료!\n정점 수: {spriteVertices.Length}", "확인");
            EditorGUIUtility.PingObject(mesh);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[에러]</color> 메쉬 생성 중 문제가 발생했습니다: {e.Message}");
            EditorUtility.DisplayDialog("에러", "메쉬 생성 실패! 콘솔창을 확인해 주세요.", "확인");
        }
    }
}