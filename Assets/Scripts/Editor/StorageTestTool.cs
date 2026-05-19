//using System;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//public class StorageTestTool : EditorWindow
//{
//    private int itemID;
//    private FlowerGrade grade;
//    private int amount;
//    public static PlayerOwnItemDataManager targetStorage;
//    private ContainerType storageType;

//    [MenuItem("Tools/Storage Test Tool")]
//    public static void ShowWindow() => GetWindow<StorageTestTool>("Storage Tool");

//    private void OnGUI()
//    {
//        GUILayout.Label("Item Injection Tool", EditorStyles.boldLabel);
//        EditorGUILayout.Space(10);

//        // 대상 스토리지 선택 (직접 지정하거나 씬에서 찾기)
//        targetStorage = (PlayerOwnItemDataManager)EditorGUILayout.ObjectField("Target Storage", targetStorage, typeof(PlayerOwnItemDataManager), true);

//        if (targetStorage == null)
//        {
//            if (GUILayout.Button("Find Storage in Scene"))
//            {
//                targetStorage = GameObject.FindFirstObjectByType<PlayerOwnItemDataManager>();
//            }
//            EditorGUILayout.HelpBox("씬에 ItemStorageParent가 있어야 아이템을 추가할 수 있습니다.", MessageType.Warning);
//        }

//        EditorGUILayout.Space(5);

//        // 저장소 타입(인벤토리, 창고 중 선택)
//        storageType = (ContainerType)EditorGUILayout.EnumPopup("저장소 선택", storageType);

//        // 입력 필드
//        itemID = EditorGUILayout.IntField("Item ID", itemID);
//        grade = EditorGUILayout.EnumFlagsField("Grade", grade)  ;
//        amount = EditorGUILayout.IntField("Amount", amount);

//        EditorGUILayout.Space(10);

//        GUI.enabled = targetStorage != null;

//        GUILayout.BeginHorizontal();
//        GUILayout.BeginVertical();
//        if (GUILayout.Button("Add Item", GUILayout.Height(30)))
//        {
//            GenerateItem();
//        }
//        if(GUILayout.Button("Remove Item", GUILayout.Height(30)))
//        {
//            RemoveItem(storageType, itemID, grade, amount);
//        }
//        GUILayout.EndVertical();
//        GUILayout.BeginVertical();
//        if (GUILayout.Button("Clear Values", GUILayout.Height(30)))
//        {
//            ClearValue(ref itemID, ref grade, ref amount);
//        }

//        if(GUILayout.Button("Swap", GUILayout.Height(30)))
//        {
//            SwapWindow.ShowWindow();
//        }
//        GUILayout.EndVertical();
//        GUILayout.EndHorizontal();

//        GUI.enabled = true;
//    }
//    private void RemoveItem(ContainerType storageType, int itemID, FlowerGrade grade, int amount)
//    {
//        if (targetStorage.RemoveItem(storageType, itemID, grade, amount) == false)
//        {
//            Debug.LogError("아이템이 존재하지 않습니다.");
//            return;
//        }
//        Debug.Log("제거 성공");
//    }

//    private void ClearValue(ref int itemID, ref FlowerGrade grade, ref int amount)
//    {
//        itemID = 0;
//        grade = 0;
//        amount = 0;

//        GUI.FocusControl(null);

//        Repaint();
//    }

//    private void GenerateItem()
//    {
//        // ItemObjectData 생성 (ID, Amount, Duration, Grade)
//        GameItem newItem = new GameItem((int)itemID, amount );

//        // Undo 등록 (에디터에서 수행 시 되돌리기 가능하도록)
//        Undo.RecordObject(targetStorage, "Add Item via Tool");

//        // 인벤토리 또는 창고에 추가 (현재 선택된 타입에 맞게)
//        targetStorage.AddItem(storageType, newItem);

//        // 변경사항 저장 및 UI 갱신 (에디터 환경 대응)
//        EditorUtility.SetDirty(targetStorage);

//        Debug.Log($"<color=green>[Storage Tool]</color> 추가 완료: {itemID} (x{amount}), 등급: {grade}");
//    }
//}


//public class SwapWindow : EditorWindow
//{
//    private ContainerType startStorage;
//    private ContainerType endStorage;
//    private int startIndex;
//    private int endIndex;
//    private int startBoxIndex = 0;
//    private int endBoxIndex = 0;

//    public static void ShowWindow() => GetWindow<SwapWindow>("Swap Option");
//    private void OnGUI()
//    {
//        float originalLabelWidth = EditorGUIUtility.labelWidth;

//        EditorGUIUtility.labelWidth = 50;
//        EditorGUILayout.Space(10);

//        //출발&도착 설정 드롭다운
//        GUILayout.BeginHorizontal();

//        #region 시작점

//        GUILayout.BeginVertical();
//        startStorage = (ContainerType)EditorGUILayout.EnumPopup("시작지", startStorage);

//        //인덱스 설정 텍스트박스
//        if (startStorage == ContainerType.STORAGE)
//        {
//            GUILayout.Label("[대상 박스의 순번 입력]", EditorStyles.boldLabel);
//            startBoxIndex = EditorGUILayout.IntField(startBoxIndex);
//        }
//        GUILayout.Label("[대상 인덱스 입력]", EditorStyles.boldLabel);
//        startIndex = (int)EditorGUILayout.IntField(startIndex);

//        GUILayout.EndVertical();

//        #endregion
//        #region 도착점

//        GUILayout.BeginVertical();
//        endStorage = (ContainerType)EditorGUILayout.EnumPopup("목적지", endStorage);

//        //인덱스 설정 텍스트박스
//        if(endStorage == ContainerType.STORAGE)
//        {
//            GUILayout.Label("[대상 박스의 순번 입력]", EditorStyles.boldLabel);
//            endBoxIndex = EditorGUILayout.IntField(endBoxIndex);
//        }
//        GUILayout.Label("[대상 인덱스 입력]", EditorStyles.boldLabel);
//        endIndex = (int)EditorGUILayout.IntField(endIndex);
//        GUILayout.EndVertical();

//        #endregion

//        GUILayout.EndHorizontal();

//        if(GUILayout.Button("Start", GUILayout.Height(30)))
//        {
//            StorageTestTool.targetStorage.Swap(startStorage, endStorage, startIndex, endIndex, startBoxIndex, endBoxIndex);
//        }

//        EditorGUIUtility.labelWidth = originalLabelWidth;
//    }
//}