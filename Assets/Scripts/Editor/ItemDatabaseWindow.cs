using UnityEditor;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.IO;
using System;
using static Constant;

public class ItemDatabaseWindow : EditorWindow
{
    private BlobAssetReference<ItemBlobDatas> _itemDB;
    private BlobAssetReference<FlowerItemBlobDatas> _flowerDB;
    private BlobAssetReference<ItemBlobDatas> _usableDB;
    private BlobAssetReference<FlowerDetailBlobDatas> _flowerDetail;
    private BlobAssetReference<UsableDetailBlobDatas> _usableDetail;

    private Vector2 _scrollPos;
    private string _searchQuery = "";

    [MenuItem("Tools/Item Database Viewer")]
    public static void ShowWindow() => GetWindow<ItemDatabaseWindow>("Item Database");

    private void OnEnable() => LoadBlobs();
    private void OnDisable() => DisposeBlobs();

    private void LoadBlobs()
    {
        DisposeBlobs();
        string blobPath = Path.Combine(Application.streamingAssetsPath, BLOB_FOLDER);

        // 분석 결과: 36바이트를 건너뛰어야 정확한 BlobArray Header(Offset 8, Length XX)가 시작됨
        _itemDB = LoadBlobWithSkip<ItemBlobDatas>(Path.Combine(blobPath, ITEM_BLOB));
        _flowerDB = LoadBlobWithSkip<FlowerItemBlobDatas>(Path.Combine(blobPath, FLOWER_BLOB));
        _usableDB = LoadBlobWithSkip<ItemBlobDatas>(Path.Combine(blobPath, USABLE_BLOB));
        _flowerDetail = LoadBlobWithSkip<FlowerDetailBlobDatas>(Path.Combine(blobPath, FLOWER_DETAIL_BLOB));
        _usableDetail = LoadBlobWithSkip<UsableDetailBlobDatas>(Path.Combine(blobPath, USABLE_DETAIL_BLOB));
        
        Repaint();
    }

    private BlobAssetReference<T> LoadBlobWithSkip<T>(string path) where T : unmanaged
    {
        if (!File.Exists(path)) return default;
        try {
            byte[] fullData = File.ReadAllBytes(path);
            if (fullData.Length <= 36) return default;

            // 정밀 분석 결과에 따라 36바이트 헤더를 제거합니다.
            byte[] pureData = new byte[fullData.Length - 36];
            Array.Copy(fullData, 36, pureData, 0, pureData.Length);

            return BlobAssetReference<T>.Create(pureData);
        }
        catch { return default; }
    }

    private void DisposeBlobs()
    {
        if (_itemDB.IsCreated) _itemDB.Dispose();
        if (_flowerDB.IsCreated) _flowerDB.Dispose();
        if (_usableDB.IsCreated) _usableDB.Dispose();
        if (_flowerDetail.IsCreated) _flowerDetail.Dispose();
        if (_usableDetail.IsCreated) _usableDetail.Dispose();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Reload Data", EditorStyles.toolbarButton)) LoadBlobs();
        _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        if (_usableDB.IsCreated) DrawSection("Usable Items", ref _usableDB.Value.Items, USABLE_START_ID);
        if (_itemDB.IsCreated) DrawSection("Common Items", ref _itemDB.Value.Items, COMMON_START_ID);
        if (_flowerDB.IsCreated) DrawSection("Flower Items", ref _flowerDB.Value.Items, FLOWER_START_ID);

        EditorGUILayout.EndScrollView();
    }

    private unsafe void DrawSection<T>(string title, ref BlobArray<T> items, int startId) where T : unmanaged
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{title} (Count: {items.Length})", EditorStyles.boldLabel);
        
        for (int i = 0; i < items.Length; i++)
        {
            ref var item = ref items[i];
            string itemName = GetName(item);
            int itemId = GetIndex(item);
            
            if (!string.IsNullOrEmpty(_searchQuery)) {
                if (!itemName.ToLower().Contains(_searchQuery.ToLower()) && !(startId + i).ToString().Contains(_searchQuery))
                    continue;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"ID: {itemId} | {itemName}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
        }
    }

    private string GetName<T>(T item)
    {
        try {
            var field = typeof(T).GetField("ItemName");
            if (field != null) return field.GetValue(item).ToString();
        } catch {}
        return "Unknown";
    }

    private int GetIndex<T>(T item)
    {
        try
        {
            var field = typeof(T).GetField("ItemId");
            if (field != null) return int.Parse(field.GetValue(item).ToString());
        }
        catch {}
        return -1;
    }
}
