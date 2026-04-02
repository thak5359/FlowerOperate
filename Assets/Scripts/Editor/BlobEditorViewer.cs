using Unity.Entities;
using UnityEditor;
using UnityEngine;
using System.IO;

public class BlobDataViewer : EditorWindow
{
    [MenuItem("Tools/Blob Data Viewer")]
    public static void ShowWindow() => GetWindow<BlobDataViewer>("Blob Viewer");

    private void OnGUI()
    {
        if (GUILayout.Button("마지막으로 구운 Blob 파일 읽어보기", GUILayout.Height(40)))
        {
            string path = "Assets/Blobs/IdData.blob"; // 파트너의 파일 경로
            if (File.Exists(path))
            {
                // 버전 번호는 베이킹할 때 넣었던 것과 맞춰주세요 (예: 1)
                if (BlobAssetReference<ItemBlobDatas>.TryRead(path, 1, out var blobRef))
                {
                    Debug.Log($"<color=yellow>--- Blob 데이터 검사 시작 ---</color>");
                    for (int i = 0; i < blobRef.Value.Items.Length; i++)
                    {
                        ref var item = ref blobRef.Value.Items[i];
                        Debug.Log($"[{i}] ID: {item.ItemId} | 이름: {item.ItemName.ToString()} | 설명: {item.Description.ToString()} | 설명: {item.SpriteAddress.ToString()}");
                    }
                    blobRef.Dispose();
                }
            }
            else { Debug.LogError("파일을 찾을 수 없어요, 파트너!"); }
        }
    }
}