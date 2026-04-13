#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

class AutoBuildPipeline
{
    [MenuItem("Build/Perform Windows Build")]
    static void PerformBuild()
    {
        Debug.Log("Starting Automated Build Pipeline...");

        // 1. Addressables 빌드 실행
        Debug.Log("Step 1: Building Addressables Content...");
        AddressableAssetSettings.BuildPlayerContent();

        // 2. 빌드 출력 경로 설정 및 디렉토리 생성
        string buildPath = "Builds/Windows/MyGame.exe";
        string buildDir = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        // 3. 플레이어 빌드 실행 (64비트 권장)
        Debug.Log("Step 2: Building Player (Windows 64-bit)...");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = FindEnabledEditorScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"<color=green>Build Succeeded!</color> Output: {buildPath} (Size: {summary.totalSize / 1024 / 1024} MB)");
        }
        else
        {
            Debug.LogError($"Build Failed! Result: {summary.result}");
        }
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }
}
#endif