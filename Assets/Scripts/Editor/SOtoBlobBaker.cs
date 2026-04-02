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
    public static void ShowWindow() => GetWindow<ItemBlobMaker>("Blobmaker (Partner Pro)");

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
            // 2. 루트 구조체(ItemBlobDatas) 생성
            ref var root = ref builder.ConstructRoot<ItemBlobDatas>();

            // 3. 내부 배열 할당 (SO의 아이템 개수만큼)
            var arrayBuilder = builder.Allocate(ref root.Items, so.itemName.Count);

            for (int i = 0; i < so.itemName.Count; i++)
            {
                // ID 기록 (시작 ID + 인덱스)
                arrayBuilder[i].ItemId = so.startId + i;

                // [중요] AllocateString은 반드시 builder를 통해 주소값을 할당해야 합니다.
                // 텍스트가 null인 경우를 대비해 ""(빈 문자열) 처리를 해주는 것이 안전합니다.
                builder.AllocateString(ref arrayBuilder[i].ItemName, so.itemName[i] ?? "");
                builder.AllocateString(ref arrayBuilder[i].Description, so.description[i] ?? "");
                builder.AllocateString(ref arrayBuilder[i].SpriteAddress, so.spriteAddress[i] ?? "");
            }

            // 4. 메모리에 구워진 데이터를 참조 형태로 추출
            var blobAssetPtr = builder.CreateBlobAssetReference<ItemBlobDatas>(Allocator.Persistent);

            // 5. 파일로 물리적 저장
            SaveBlobToFile(blobAssetPtr, so.name);

            // 6. 사용한 네이티브 메모리 해제
            blobAssetPtr.Dispose();
        }
        finally
        {
            builder.Dispose(); // 메모리 해제 보장
        }
    }

    private void SaveBlobToFile<T>(BlobAssetReference<T> blobRef, string fileName) where T : unmanaged
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string fullPath = Path.Combine(savePath, $"{fileName}.blob");

        // 바이너리 파일 쓰기
        using (var stream = new FileStream(fullPath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            unsafe
            {
                // 블롭의 실제 메모리 크기만큼 바이트 단위로 씁니다.
                byte* ptr = (byte*)blobRef.GetUnsafePtr();
                int size = blobRef.Value.Header.Length;

                for (int i = 0; i < size; i++)
                {
                    writer.Write(ptr[i]);
                }
            }
        }
        Debug.Log($"<color=green>[Bake 성공]</color> {fileName} -> {fullPath}");
    }
}
#endif