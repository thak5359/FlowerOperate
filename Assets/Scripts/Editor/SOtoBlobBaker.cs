#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class ItemBlobMaker : EditorWindow
{
    // ItemIdData와 ItemDetailData가 서로 상속관계가 아니므로 ScriptableObject로 관리합니다.
    [SerializeField] private List<ScriptableObject> targetSOList = new List<ScriptableObject>();
    private string savePath = "Assets/StreamingAssets/Blobs";

    [MenuItem("Tools/Bake Item Data to Blob")]
    public static void ShowWindow() => GetWindow<ItemBlobMaker>("Blobmaker");

    private void OnGUI()
    {
        GUILayout.Label("HPC# 데이터 베이킹 도구 (ID/Flower/Usable 지원)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty listProp = so.FindProperty("targetSOList");
        EditorGUILayout.PropertyField(listProp, new GUIContent("대상 SO 리스트"), true);
        so.ApplyModifiedProperties();

        savePath = EditorGUILayout.TextField("파일 저장 경로", savePath);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("리스트의 모든 SO를 각각의 BLOB으로 굽기", GUILayout.Height(40)))
        {
            if (targetSOList == null || targetSOList.Count == 0)
            {
                EditorUtility.DisplayDialog("경고", "리스트에 SO 데이터를 넣어주세요!", "확인");
                return;
            }

            foreach (var itemSO in targetSOList)
            {
                if (itemSO == null) continue;

                // 상속 구조에 따른 타입 체크 및 베이킹 분기
                if (itemSO is FlowerIdData flowerId) Bake(flowerId);
                else if (itemSO is UsableIdData usableId) Bake(usableId);
                else if (itemSO is FlowerDetailData flowerDetail) BakeDetail(flowerDetail);
                else if (itemSO is UsableDetailData usableDetail) BakeDetail(usableDetail);
                else if (itemSO is ItemIdData itemId) Bake(itemId);
                else Debug.LogWarning($"[Bake] 지원하지 않는 SO 타입입니다: {itemSO.name} ({itemSO.GetType()})");
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("완료", "모든 데이터가 바이너리로 저장되었습니다!", "확인");
        }
    }

    #region Base Item ID Baking (ItemIdData / FlowerIdData / UsableIdData)
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
                arrayBuilder[i].ItemName = so.itemName[i];
                arrayBuilder[i].Description = (i < so.description.Count) ? so.description[i] : default;
                arrayBuilder[i].SpriteAddress = (i < so.spriteAddress.Count) ? so.spriteAddress[i] : default;
            }

            SaveToBlob<ItemBlobDatas>(builder, so.name);
        }
        finally { builder.Dispose(); }
    }
    #endregion

    #region Flower Detail Baking
    public void BakeDetail(FlowerDetailData so)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        try
        {
            ref var root = ref builder.ConstructRoot<FlowerDetailBlobDatas>();
            int count = so.speciesList.Count; // 세 리스트의 크기가 동일하다고 가정
            var arrayBuilder = builder.Allocate(ref root.flowerDetails, count);

            for (int i = 0; i < count; i++)
            {
                arrayBuilder[i].species = so.speciesList[i];
                arrayBuilder[i].color = (i < so.colorList.Count) ? so.colorList[i] : default;
                arrayBuilder[i].floro = (i < so.floroList.Count) ? so.floroList[i] : default;
            }

            SaveToBlob<FlowerDetailBlobDatas>(builder, so.name);
        }
        finally { builder.Dispose(); }
    }
    #endregion

    #region Usable Detail Baking
    public void BakeDetail(UsableDetailData so)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        try
        {
            ref var root = ref builder.ConstructRoot<UsableDetailBlobDatas>();
            int count = so.durationList.Count;
            var arrayBuilder = builder.Allocate(ref root.usableDetails, count);

            for (int i = 0; i < count; i++)
            {
                arrayBuilder[i].index = (byte)i;
                arrayBuilder[i].duration = so.durationList[i];
                arrayBuilder[i].power = (i < so.powerList.Count) ? so.powerList[i] : (short)0;
                arrayBuilder[i].chargeInfo = (i < so.chargeInfoList.Count) ? so.chargeInfoList[i] : default;
            }

            SaveToBlob<UsableDetailBlobDatas>(builder, so.name);
        }
        finally { builder.Dispose(); }
    }
    #endregion

    private void SaveToBlob<T>(BlobBuilder builder, string fileName) where T : unmanaged
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        string fullPath = Path.Combine(savePath, $"{fileName}.blob");

        BlobAssetReference<T>.Write(builder, fullPath, 1);
        Debug.Log($"<color=green>[Bake 완료]</color> {fileName} -> {fullPath}");
    }
}
#endif
