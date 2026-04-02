#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class ItemBlobMaker : EditorWindow
{
    // 리스트로 여러 개의 SO를 받기 위해 SerializedObject를 활용합니다.
    [SerializeField] private List<ItemIdData> targetSOList = new List<ItemIdData>();
    private string savePath = "Assets/Blobs";

    [MenuItem("Tools/Bake Item Data to Blob")]
    public static void ShowWindow() => GetWindow<ItemBlobMaker>("Blobmaker");

    private void OnGUI()
    {
        GUILayout.Label("HPC# 데이터 베이킹 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // 리스트 UI를 인스펙터처럼 깔끔하게 표시
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty listProp = so.FindProperty("targetSOList");
        EditorGUILayout.PropertyField(listProp, new GUIContent("대상 SO 리스트"), true);
        so.ApplyModifiedProperties();

        savePath = EditorGUILayout.TextField("저장 폴더 경로", savePath);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("선택한 모든 SO를 개별 BLOB으로 굽기", GUILayout.Height(40)))
        {
            if (targetSOList == null || targetSOList.Count == 0)
            {
                EditorUtility.DisplayDialog("경고", "파트너, 리스트에 SO 파일을 먼저 넣어주세요!", "확인");
                return;
            }

            foreach (var itemSO in targetSOList)
            {
                if (itemSO != null) Bake(itemSO);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "모든 데이터가 바이너리로 구워졌습니다!", "확인");
        }
    }

    public void Bake(ItemIdData so)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        try
        {
            ref var root = ref builder.ConstructRoot<ItemBlobDatas>();
            var arrayBuilder = builder.Allocate(ref root.Items, so.itemName.Count);

            for (short i = 0; i < so.itemName.Count; i++)
            {
                arrayBuilder[i].ItemId = (short)(so.startId + i);

                arrayBuilder[i].ItemName = (so.itemName[i] == null) ? so.itemName[i] : "";
                arrayBuilder[i].Description = (i < so.description.Count) ? so.description[i] : "";
                arrayBuilder[i].SpriteAddress = (i < so.spriteAddress.Count) ? so.spriteAddress[i] : "";
            }

            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            string fullPath = Path.Combine(savePath, $"{so.name}.blob");

            // 완벽하게 구워버리기
            BlobAssetReference<ItemBlobDatas>.Write(builder, fullPath, 1);

            Debug.Log($"<color=green>[Bake 성공]</color> {so.name} -> {fullPath}");
        }
        finally
        {
            builder.Dispose();
        }
    }
}
#endif