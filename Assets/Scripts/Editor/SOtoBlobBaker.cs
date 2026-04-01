#if UNITY_EDITOR
using Fungus;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class ItemBlobMaker : EditorWindow
{
    private List<ScriptableObject> targetSOList;
    private string savePath = "Assets/Blobs";

    [MenuItem("Tools/Bake Item Data to Blob")]
    public static void ShowWindow()
    {
        GetWindow<ItemBlobMaker>("Blobmaker");
    }


    private void OnGUI()
    {
        GUILayout.Label("아이템 SO -> 메쉬 파일 저장 도구 (Pro)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty listProp = so.FindProperty("targetSOList");
        EditorGUILayout.PropertyField(listProp, new  GUIContent("대상 SO 리스트"),true);
        so.ApplyModifiedProperties();

        savePath = EditorGUILayout.TextField("저장 폴더 경로", savePath);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("ScriptableObject를 BLOB 형식으로 굽습니다", GUILayout.Height(40)))
        {
            if (targetSOList.Count == 0)
            {
                EditorUtility.DisplayDialog("경고", "파트너, SO 파일을 먼저 넣어주세요!", "확인");
                return;
            }
            foreach (var target in targetSOList)
            { Bake(target); }
        }
    }



    public static void Bake(ScriptableObject SO)
    {
        if (SO is ItemIdData)
        {
            if (SO is UsableIdData)
            {
                UsableIdData targetSO1 = (UsableIdData)SO;
                using (var builder = new BlobBuilder(Allocator.Temp))
                {
                    // 2. 컨테이너 생성
                    ref var root = ref builder.ConstructRoot<ItemBlobDatas>();
                    var arrayBuilder = builder.Allocate(ref root.Items, targetSO1.itemName.Count);

                    for (int i = 0; i < targetSO1.itemName.Count; i++)
                    {
                        builder.AllocateString(ref arrayBuilder[i].ItemName, targetSO1.itemName[i]);
                        builder.AllocateString(ref arrayBuilder[i].Description, targetSO1.description[i]);
                        builder.AllocateString(ref arrayBuilder[i].SpriteAddress, targetSO1.spriteAddress[i]);
                    }

                    // 3. 파일로 저장 (바이너리 데이터 생성)
                    var blobAsset = builder.CreateBlobAssetReference<ItemBlobDatas>(Allocator.Persistent);

                    // 이 blobAsset을 파일로 저장하거나, 전역 매니저에 들고 있게 합니다.
                    // (실제 프로젝트에서는 ScriptableObject에 Reference를 담아 저장하는 방식을 씁니다.)
                }
            }
            else if (SO is FlowerIdData)
            {
                FlowerIdData targetSO2 = (FlowerIdData)SO;
                using (var builder = new BlobBuilder(Allocator.Temp))
                {
                    // 2. 컨테이너 생성
                    ref var root = ref builder.ConstructRoot<ItemBlobDatas>();
                    var arrayBuilder = builder.Allocate(ref root.Items, targetSO2.itemName.Count);

                    for (int i = 0; i < targetSO2.itemName.Count; i++)
                    {
                        builder.AllocateString(ref arrayBuilder[i].ItemName, targetSO2.itemName[i]);
                        builder.AllocateString(ref arrayBuilder[i].Description, targetSO2.description[i]);
                        builder.AllocateString(ref arrayBuilder[i].SpriteAddress, targetSO2.spriteAddress[i]);
                    }

                    // 3. 파일로 저장 (바이너리 데이터 생성)
                    var blobAsset = builder.CreateBlobAssetReference<ItemBlobDatas>(Allocator.Persistent);

                    // 이 blobAsset을 파일로 저장하거나, 전역 매니저에 들고 있게 합니다.
                    // (실제 프로젝트에서는 ScriptableObject에 Reference를 담아 저장하는 방식을 씁니다.)
                }
            }
            else
            {
                ItemIdData targetSO3 = (ItemIdData)SO;
                using (var builder = new BlobBuilder(Allocator.Temp))
                {
                    // 2. 컨테이너 생성
                    ref var root = ref builder.ConstructRoot<ItemBlobDatas>();
                    var arrayBuilder = builder.Allocate(ref root.Items, targetSO3.itemName.Count);

                    for (int i = 0; i < targetSO3.itemName.Count; i++)
                    {
                        builder.AllocateString(ref arrayBuilder[i].ItemName, targetSO3.itemName[i]);
                        builder.AllocateString(ref arrayBuilder[i].Description, targetSO3.description[i]);
                        builder.AllocateString(ref arrayBuilder[i].SpriteAddress, targetSO3.spriteAddress[i]);
                    }

                    // 3. 파일로 저장 (바이너리 데이터 생성)
                    var blobAsset = builder.CreateBlobAssetReference<ItemBlobDatas>(Allocator.Persistent);

                    // 이 blobAsset을 파일로 저장하거나, 전역 매니저에 들고 있게 합니다.
                    // (실제 프로젝트에서는 ScriptableObject에 Reference를 담아 저장하는 방식을 씁니다.)
                }
            }
        }
        else if (SO is ItemDetailData)
        {
            if (SO is UsableDetailData)
            {

            }
            else if (SO is FlowerDetailData)
            {

            }
            else
            {

            }
        }
        else { EditorUtility.DisplayDialog("경고", "파트너, Item 관련 데이터가 아닌거 같은데요?!", "확인"); }
    }
}
#endif