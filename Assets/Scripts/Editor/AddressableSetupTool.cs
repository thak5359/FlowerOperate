using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AddressableSetupTool : EditorWindow
{
    private int selectedGroupIndex = 0;
    private string labelToApply = "";
    private List<AddressableAssetGroup> groups = new List<AddressableAssetGroup>();
    private string[] groupNames = new string[0];

    [MenuItem("Tools/Addressable Quick Setup")]
    public static void ShowWindow()
    {
        GetWindow<AddressableSetupTool>("Addressable Setup");
    }

    private void OnFocus()
    {
        RefreshGroups();
    }

    private void RefreshGroups()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        groups = settings.groups.Where(g => g != null).ToList();
        groupNames = groups.Select(g => g.Name).ToArray();
    }

    private void OnGUI()
    {
        GUILayout.Label("선택 항목 Addressable 일괄 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (groupNames.Length == 0)
        {
            EditorGUILayout.HelpBox("Addressable Settings를 먼저 생성해주세요.", MessageType.Warning);
            if (GUILayout.Button("설정 새로고침")) RefreshGroups();
            return;
        }

        selectedGroupIndex = EditorGUILayout.Popup("대상 그룹", selectedGroupIndex, groupNames);

        labelToApply = EditorGUILayout.TextField("적용할 라벨 (선택)", labelToApply);

        EditorGUILayout.Space();

        if (GUILayout.Button("선택한 항목 일괄 적용", GUILayout.Height(30)))
        {
            ApplyAddressableSettings();
        }

        EditorGUILayout.HelpBox("팁: 프로젝트 창에서 파일을 선택한 후 버튼을 누르세요.\n주소는 파일명(확장자 제외)으로 자동 설정됩니다.", MessageType.Info);
    }

    private void ApplyAddressableSettings()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        var targetGroup = groups[selectedGroupIndex];
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("[AddressableSetup] 선택된 항목이 없습니다.");
            return;
        }

        int count = 0;
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || Directory.Exists(path)) continue;

            string guid = AssetDatabase.AssetPathToGUID(path);

            // 에셋 항목 생성 또는 가져오기
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, targetGroup);

            if (entry != null)
            {
                // 주소를 파일 이름으로 설정 (확장자 제외)
                entry.address = Path.GetFileNameWithoutExtension(path);

                // 라벨 적용
                if (!string.IsNullOrEmpty(labelToApply))
                {
                    // 세팅에 라벨이 없으면 먼저 추가
                    if (!settings.GetLabels().Contains(labelToApply))
                    {
                        settings.AddLabel(labelToApply);
                    }
                    entry.SetLabel(labelToApply, true);
                }

                count++;
            }
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AddressableSetup] {count}개의 항목이 '{targetGroup.Name}' 그룹에 설정되었습니다.");
    }
}