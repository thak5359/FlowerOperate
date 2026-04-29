using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AllStorageTestTool : EditorWindow
{
    private ushort itemID;
    private byte grade;
    private short amount;
    public static ItemStorageParent targetStorage;
    private ContainerType storageType;

    [MenuItem("Tools/All Storage Test Tool")]
    public static void ShowWindow() => GetWindow<AllStorageTestTool>("Storage Tool");

    private void OnGUI()
    {
        GUILayout.Label("Item Injection Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // 대상 스토리지 선택 (직접 지정하거나 씬에서 찾기)
        targetStorage = (ItemStorageParent)EditorGUILayout.ObjectField("Target Storage", targetStorage, typeof(ItemStorageParent), true);

        if (targetStorage == null)
        {
            if (GUILayout.Button("Find Storage in Scene"))
            {
                targetStorage = GameObject.FindFirstObjectByType<ItemStorageParent>();
            }
            EditorGUILayout.HelpBox("씬에 ItemStorageParent가 있어야 아이템을 추가할 수 있습니다.", MessageType.Warning);
        }

        EditorGUILayout.Space(5);

        // 저장소 타입(인벤토리, 창고 중 선택)
        storageType = (ContainerType)EditorGUILayout.EnumPopup("저장소 선택", storageType);

        // 입력 필드
        itemID = (ushort)EditorGUILayout.IntField("Item ID", (int)itemID);
        grade = (byte)EditorGUILayout.IntField("Grade", (int)grade);
        amount = (short)EditorGUILayout.IntField("Amount", (int)amount);

        EditorGUILayout.Space(10);

        GUI.enabled = targetStorage != null;

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Add Item", GUILayout.Height(30)))
        {
            GenerateItem();
        }
        if(GUILayout.Button("Remove Item", GUILayout.Height(30)))
        {
            RemoveItem(storageType, itemID, grade, amount);
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Clear Values", GUILayout.Height(30)))
        {
            ClearValue(ref itemID, ref grade, ref amount);
        }

        if(GUILayout.Button("Swap", GUILayout.Height(30)))
        {
            SwapWindow.ShowWindow();
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }
    private void RemoveItem(ContainerType storageType, ushort itemID, byte grade, short amount)
    {
        if (targetStorage.RemoveItem(storageType, itemID, grade, amount) == false)
        {
            Debug.LogError("아이템이 존재하지 않습니다.");
            return;
        }
        Debug.Log("제거 성공");
    }

    private void ClearValue(ref ushort itemID, ref byte grade, ref short amount)
    {
        itemID = 0;
        grade = 0;
        amount = 0;

        GUI.FocusControl(null);

        Repaint();
    }

    private void GenerateItem()
    {
        // ItemObjectData 생성 (ID, Amount, Duration, Grade)
        ItemObjectData newItem = new ItemObjectData((ushort)itemID, (short)amount, 0, (byte)grade);

        // Undo 등록 (에디터에서 수행 시 되돌리기 가능하도록)
        Undo.RecordObject(targetStorage, "Add Item via Tool");

        // 인벤토리에 추가
        List<ItemObjectData> inven = targetStorage.GetComponent<ItemStorageParent>().GetData.GetList(storageType);
        inven.Add(newItem);
        // 변경사항 저장 및 UI 갱신 (에디터 환경 대응)
        EditorUtility.SetDirty(targetStorage);

        Debug.Log($"<color=green>[Storage Tool]</color> 추가 완료: {itemID} (x{amount}), 등급: {grade}");
    }
}


public class SwapWindow : EditorWindow
{
    private ContainerType startStorage;
    private ContainerType endStorage;
    private int startIndex;
    private int endIndex;

    public static void ShowWindow() => GetWindow<SwapWindow>("Swap Option");
    private void OnGUI()
    {
        float originalLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 50;
        EditorGUILayout.Space(10);

        //출발&도착 설정 드롭다운
        GUILayout.BeginHorizontal();

        #region 시작점

        GUILayout.BeginVertical();
        startStorage = (ContainerType)EditorGUILayout.EnumPopup("시작지", startStorage);

        //인덱스 설정 텍스트박스
        GUILayout.Label("[인덱스 설정]", EditorStyles.boldLabel);
        startIndex = (int)EditorGUILayout.IntField(startIndex);

        GUILayout.EndVertical();

        #endregion
        #region 도착점

        GUILayout.BeginVertical();
        endStorage = (ContainerType)EditorGUILayout.EnumPopup("목적지", endStorage);

        //인덱스 설정 텍스트박스
        GUILayout.Label("[인덱스 설정]", EditorStyles.boldLabel);
        endIndex = (int)EditorGUILayout.IntField(endIndex);

        GUILayout.EndVertical();

        #endregion

        GUILayout.EndHorizontal();

        if(GUILayout.Button("Start", GUILayout.Height(30)))
        {
            AllStorageTestTool.targetStorage.Swap(startStorage, endStorage, startIndex, endIndex);
        }

        EditorGUIUtility.labelWidth = originalLabelWidth;
    }
}