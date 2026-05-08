using UnityEditor;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;
using System;
using static Constant;

public class ItemDatabaseWindow : EditorWindow
{
    private BlobAssetReference<ItemBlobDatas> _itemDB;
    private BlobAssetReference<FlowerItemBlobDatas> _flowerDB;
    private BlobAssetReference<UsableItemBlobDatas> _usableDB;
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

        _itemDB = LoadBlob<ItemBlobDatas>(Path.Combine(blobPath, ITEM_BLOB));
        _flowerDB = LoadBlob<FlowerItemBlobDatas>(Path.Combine(blobPath, FLOWER_BLOB));
        _usableDB = LoadBlob<UsableItemBlobDatas>(Path.Combine(blobPath, USABLE_BLOB));
        _flowerDetail = LoadBlob<FlowerDetailBlobDatas>(Path.Combine(blobPath, FLOWER_DETAIL_BLOB));
        _usableDetail = LoadBlob<UsableDetailBlobDatas>(Path.Combine(blobPath, USABLE_DETAIL_BLOB));
        
        Repaint();
    }

    private BlobAssetReference<T> LoadBlob<T>(string path) where T : unmanaged
    {
        if (!File.Exists(path)) return default;
        
        if (BlobAssetReference<T>.TryRead(path, 1, out var blobRef))
        {
            return blobRef;
        }
        
        Debug.LogError($"Blob 파일을 읽는데 실패했습니다 (버전 불일치 가능성): {path}");
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

        if (_usableDB.IsCreated) DrawSection("Usable Items", ref _usableDB.Value.Items);
        if (_itemDB.IsCreated) DrawSection("Common Items", ref _itemDB.Value.Items);
        if (_flowerDB.IsCreated) DrawSection("Flower Items", ref _flowerDB.Value.Items);

        EditorGUILayout.EndScrollView();
    }

    private unsafe void DrawSection<T>(string title, ref BlobArray<T> items) where T : unmanaged
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"{title} (Count: {items.Length})", EditorStyles.boldLabel);
        
        for (int i = 0; i < items.Length; i++)
        {
            ref var item = ref items[i];
            
            fixed (T* ptr = &item)
            {
                short itemId = 0;
                string itemName = "";

                // 타입을 명확히 구분하여 ID와 이름을 먼저 추출 (ItemHeader 대신 실제 타입 사용)
                if (typeof(T) == typeof(FlowerItemBlobData)) {
                    var p = (FlowerItemBlobData*)ptr;
                    itemId = p->ItemId;
                    itemName = p->ItemName.ToString();
                } else if (typeof(T) == typeof(UsableItemBlobData)) {
                    var p = (UsableItemBlobData*)ptr;
                    itemId = p->ItemId;
                    itemName = p->ItemName.ToString();
                } else if (typeof(T) == typeof(ItemBlobData)) {
                    var p = (ItemBlobData*)ptr;
                    itemId = p->ItemId;
                    itemName = p->ItemName.ToString();
                }
                
                if (!string.IsNullOrEmpty(_searchQuery)) {
                    if (!itemName.ToLower().Contains(_searchQuery.ToLower()) && !itemId.ToString().Contains(_searchQuery))
                        continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"ID: {itemId} | {itemName}", EditorStyles.boldLabel);
                
                // 타입별 상세 정보 출력 (이미 각 타입에 맞는 오프셋을 컴파일러가 계산함)
                if (typeof(T) == typeof(FlowerItemBlobData)) DrawFlowerFields((FlowerItemBlobData*)ptr);
                else if (typeof(T) == typeof(UsableItemBlobData)) DrawUsableFields((UsableItemBlobData*)ptr);
                else if (typeof(T) == typeof(ItemBlobData)) DrawCommonFields((ItemBlobData*)ptr);
                
                EditorGUILayout.EndVertical();
            }
        }
    }

    private unsafe void DrawCommonFields(ItemBlobData* item)
    {
        string sprite = item->SpriteAddress.IsEmpty ? "<color=yellow>(Empty)</color>" : item->SpriteAddress.ToString();
        string desc = item->Description.IsEmpty ? "<color=yellow>(Empty)</color>" : item->Description.ToString();

        EditorGUILayout.LabelField($"Price: {item->Price} | Sprite: {sprite}", new GUIStyle(EditorStyles.label) { richText = true });
        EditorGUILayout.LabelField($"Desc: {desc}", new GUIStyle(EditorStyles.label) { richText = true });
    }

    private unsafe void DrawFlowerFields(FlowerItemBlobData* item)
    {
        string sprite = item->SpriteAddress.IsEmpty ? "<color=yellow>(Empty)</color>" : item->SpriteAddress.ToString();
        string desc = item->Description.IsEmpty ? "<color=yellow>(Empty)</color>" : item->Description.ToString();

        EditorGUILayout.LabelField($"Price: {item->Price} | Sprite: {sprite}", new GUIStyle(EditorStyles.label) { richText = true });
        
        string speciesName = "Unknown";
        if (_flowerDetail.IsCreated && item->speciesIndex < _flowerDetail.Value.flowerDetails.Length)
            speciesName = _flowerDetail.Value.flowerDetails[item->speciesIndex].species.ToString();

        EditorGUILayout.LabelField($"Species: {speciesName} ({item->speciesIndex}) | Color: {_flowerDetail.Value.flowerDetails[item->colorIndex].color.ToString()}({item->colorIndex})");
        EditorGUILayout.LabelField($"Floro: {_flowerDetail.Value.flowerDetails[item->floroIndex].floro.ToString()}({item->floroIndex}) / {((item->floroIndex2 != -1) ? _flowerDetail.Value.flowerDetails[item->floroIndex2].floro.ToString() : "None")}({item->floroIndex2}) | Growth: {item->growthDuration} | Harvest: {item->harvestAmount}");
        EditorGUILayout.LabelField($"Desc: {desc}", new GUIStyle(EditorStyles.label) { richText = true });
    }

    private unsafe void DrawUsableFields(UsableItemBlobData* item)
    {
        string sprite = item->SpriteAddress.IsEmpty ? "<color=yellow>(Empty)</color>" : item->SpriteAddress.ToString();
        string desc = item->Description.IsEmpty ? "<color=yellow>(Empty)</color>" : item->Description.ToString();

        EditorGUILayout.LabelField($"Price: {item->Price} | Sprite: {sprite}", new GUIStyle(EditorStyles.label) { richText = true });
        EditorGUILayout.LabelField($"Stats -> DurIdx: {item->durationIndex} | PowerIdx: {item->powerIndex} | ChargeIdx: {item->chargeIndex}");
        EditorGUILayout.LabelField($"Desc: {desc}", new GUIStyle(EditorStyles.label) { richText = true });
    }
}

