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
    private BlobAssetReference<UsableItemBlobDatas> _usableDB; // 타입 수정
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

        // 표준 TryRead 방식으로 변경 (버전 1)
        _itemDB = LoadBlob<ItemBlobDatas>(Path.Combine(blobPath, ITEM_BLOB));
        _flowerDB = LoadBlob<FlowerItemBlobDatas>(Path.Combine(blobPath, FLOWER_BLOB));
        _usableDB = LoadBlob<UsableItemBlobDatas>(Path.Combine(blobPath, USABLE_BLOB)); // 타입 수정
        _flowerDetail = LoadBlob<FlowerDetailBlobDatas>(Path.Combine(blobPath, FLOWER_DETAIL_BLOB));
        _usableDetail = LoadBlob<UsableDetailBlobDatas>(Path.Combine(blobPath, USABLE_DETAIL_BLOB));
        
        Repaint();
    }

    private BlobAssetReference<T> LoadBlob<T>(string path) where T : unmanaged
    {
        if (!File.Exists(path)) return default;
        
        // 유니티 표준 Blob 읽기 방식 사용
        if (BlobAssetReference<T>.TryRead(path, 1, out var blobRef))
        {
            return blobRef;
        }
        
        Debug.LogError($"Blob 파일을 읽는데 실패했습니다: {path}");
        return default;
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

        // Value.Items를 넘겨줌으로써 타입에 유연하게 대응
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
                if (!itemName.ToLower().Contains(_searchQuery.ToLower()) && !itemId.ToString().Contains(_searchQuery))
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
            if (field != null) return (short)field.GetValue(item);
        }
        catch {}
        return -1;
    }
}
