#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

public class GlobalItemViewer : EditorWindow
{
    private enum Tab { Base, Flower, Gear, Fertilizer }
    private Tab _currentTab = Tab.Base;

    private Vector2 _scrollPos;
    private string _searchString = "";

    [MenuItem("Tools/Item/Global Item DB Viewer")]
    public static void Open()
    {
        GetWindow<GlobalItemViewer>("Global Item DB Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Global Item DB Viewer", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);
        EditorGUILayout.HelpBox("이 도구는 '런타임(Play Mode)' 중에 GlobalItemDB에 로드된 데이터를 표시합니다.", MessageType.Info);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("현재 에디터 모드입니다. 게임을 실행해야 데이터를 확인할 수 있습니다.", MessageType.Warning);
        }

        if (!GlobalItemDB.IsInitialized)
        {
            EditorGUILayout.HelpBox("GlobalItemDB가 초기화되지 않았습니다. (데이터 로드 전)", MessageType.Warning);
            if (GUILayout.Button("새로고침 (Repaint)")) { Repaint(); }
            return;
        }

        // 상단 탭
        _currentTab = (Tab)GUILayout.Toolbar((int)_currentTab, Enum.GetNames(typeof(Tab)));

        // 검색바
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label("검색 (ID/Name):", GUILayout.Width(100));
        _searchString = EditorGUILayout.TextField(_searchString);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        switch (_currentTab)
        {
            case Tab.Base: DrawBaseItems(); break;
            case Tab.Flower: DrawFlowerItems(); break;
            case Tab.Gear: DrawGearItems(); break;
            case Tab.Fertilizer: DrawFertilizerItems(); break;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawBaseItems()
    {
        var accessor = GetAccessor();
        if (!accessor.ItemBaseDB.IsCreated) return;

        ref var items = ref accessor.ItemBaseDB.Value.Items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            string name = item.ItemName.ToString();
            if (!MatchesSearch(item.ItemId.ToString(), name)) continue;

            BeginItemBox(item.ItemId, name);
            EditorGUILayout.LabelField("MainType", item.MainType.ToString());
            EditorGUILayout.LabelField("SubType", item.SubType.ToString());
            EditorGUILayout.LabelField("StackLimit", item.StackLimit.ToString());
            EditorGUILayout.LabelField("Price", item.Price.ToString());
            EditorGUILayout.LabelField("Sprite", item.SpriteAddress.ToString());
            EndItemBox();
        }
    }

    private void DrawFlowerItems()
    {
        var accessor = GetAccessor();
        if (!accessor.FlowerDB.IsCreated) return;

        ref var items = ref accessor.FlowerDB.Value.Items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (!MatchesSearch(item.ItemId.ToString(), "")) continue;

            BeginItemBox(item.ItemId, "Flower Data");
            EditorGUILayout.LabelField("Species", item.Species.ToString());
            EditorGUILayout.LabelField("Color", item.Color.ToString());
            EditorGUILayout.LabelField("Florio1", item.Florio1.ToString());
            EditorGUILayout.LabelField("Florio2", item.Florio2.ToString());
            EditorGUILayout.LabelField("Duration", item.GrowthDuration.ToString());
            EditorGUILayout.LabelField("Harvest", item.HarvestAmount.ToString());
            EndItemBox();
        }
    }

    private void DrawGearItems()
    {
        var accessor = GetAccessor();
        if (!accessor.GearDB.IsCreated) return;

        ref var items = ref accessor.GearDB.Value.Items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (!MatchesSearch(item.ItemId.ToString(), "")) continue;

            BeginItemBox(item.ItemId, "Gear Data");
            EditorGUILayout.LabelField("Type", item.GearType.ToString());
            EditorGUILayout.LabelField("Durability", item.MaxDuration.ToString());
            EditorGUILayout.LabelField("Efficiency", item.Efficiency.ToString());
            EditorGUILayout.LabelField("ChargeTime", item.ChargeTime.ToString());
            EditorGUILayout.LabelField("MaxCharge", item.MaxCharge.ToString());
            EndItemBox();
        }
    }

    private void DrawFertilizerItems()
    {
        var accessor = GetAccessor();
        if (!accessor.FertilizerDB.IsCreated) return;

        ref var items = ref accessor.FertilizerDB.Value.Items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (!MatchesSearch(item.ItemId.ToString(), "")) continue;

            BeginItemBox(item.ItemId, "Fertilizer Data");
            EditorGUILayout.LabelField("Type", item.FertilizerType.ToString());
            EditorGUILayout.LabelField("Level", item.Level.ToString());
            EndItemBox();
        }
    }

    private void BeginItemBox(int id, string name)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"<b>[{id}] {name}</b>", new GUIStyle(EditorStyles.label) { richText = true });
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
    }

    private void EndItemBox()
    {
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private bool MatchesSearch(string id, string name)
    {
        if (string.IsNullOrEmpty(_searchString)) return true;
        
        // StringComparison 오버로드 호환성 문제 해결 (IndexOf 사용)
        return id.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0 || 
               name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private ItemDatabaseAccessor GetAccessor()
    {
        // Reflection을 사용하여 GlobalItemDB의 private field인 _accessor에 접근
        FieldInfo field = typeof(GlobalItemDB).GetField("_accessor", BindingFlags.Static | BindingFlags.NonPublic);
        return (ItemDatabaseAccessor)field.GetValue(null);
    }
}

#endif
