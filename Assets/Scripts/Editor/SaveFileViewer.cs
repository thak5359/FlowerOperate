using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveFileViewer : EditorWindow
{
    private SaveDatas saveData;
    private Vector2 scrollPosition;
    private string fileName = "save.bytes";

    [MenuItem("Tools/Save File Viewer")]
    public static void ShowWindow()
    {
        GetWindow<SaveFileViewer>("Save File Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save File Viewer", EditorStyles.boldLabel);

        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Load Save File"))
        {
            LoadSaveFile();
        }

        if (saveData != null)
        {
            if (GUILayout.Button("Export to JSON"))
            {
                ExportToJson();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Save Time", saveData.GetSaveTime);
            EditorGUILayout.LabelField("Play Day", saveData.GetPlayDay.ToString());
            EditorGUILayout.LabelField("Money", saveData.GetMoney.ToString());
            EditorGUILayout.LabelField("Reputation", saveData.GetReputation.ToString());

            EditorGUILayout.Space();
            DrawItemData(saveData.GetItemData);

            EditorGUILayout.Space();
            DrawPlotData(saveData.GetPlotData);

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No save data loaded. Click 'Load Save File' to load data from persistentDataPath.", MessageType.Info);
        }
    }

    private void LoadSaveFile()
    {
        try
        {
            saveData = FileDataHandler.LoadBinary<SaveDatas>(fileName);
            if (saveData == null)
            {
                Debug.LogError($"Failed to load {fileName} or file does not exist.");
            }
            else
            {
                Debug.Log($"Successfully loaded {fileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading save file: {e.Message}");
            saveData = null;
        }
    }

    private void ExportToJson()
    {
        if (saveData == null) return;

        string json = JsonUtility.ToJson(saveData, true);
        string path = EditorUtility.SaveFilePanel("Save JSON", "", "save_debug.json", "json");
        
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"Exported to {path}");
        }
    }

    private void DrawItemData(ItemInstantData data)
    {
        EditorGUILayout.LabelField("Item Data", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Money", data.GetMoney.ToString());
        EditorGUILayout.LabelField("Reputation", data.GetReputation.ToString());

        var storageBoxes = data.GetStorageBoxes;
        if (storageBoxes != null && storageBoxes.Count > 0)
        {
            EditorGUILayout.LabelField("Storage Boxes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < storageBoxes.Count; i++)
            {
                var box = storageBoxes[i];
                EditorGUILayout.LabelField($"Box {i}: {box.BoxName}");
                if (box.BoxSlots != null)
                {
                    EditorGUI.indentLevel++;
                    foreach (var slot in box.BoxSlots)
                    {
                        if (slot.Id != 0)
                        {
                            EditorGUILayout.LabelField($"Item ID: {slot.Id}, Amount: {slot.Count}, Name: {slot.ItemName}");
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
    }

    private void DrawPlotData(IDictionary<int, PlotData> plotData)
    {
        EditorGUILayout.LabelField("Plot Data", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        if (plotData != null && plotData.Count > 0)
        {
            foreach (var kvp in plotData)
            {
                EditorGUILayout.LabelField($"Plot ID: {kvp.Key}");
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Flower ID: {kvp.Value.Id}");
                EditorGUILayout.LabelField($"Growth: {kvp.Value.Growth}");
                EditorGUILayout.LabelField($"State: {kvp.Value.State}");
                EditorGUILayout.LabelField($"Grade: {kvp.Value.grade}");
                EditorGUILayout.LabelField($"Position: {kvp.Value.Position}");
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUILayout.LabelField("No plot data.");
        }
        EditorGUI.indentLevel--;
    }
}
