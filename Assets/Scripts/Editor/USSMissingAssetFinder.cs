using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class USSMissingAssetFinder : EditorWindow
{
    [MenuItem("Tools/USS & UXML Missing Asset Finder")]
    public static void ShowWindow()
    {
        GetWindow<USSMissingAssetFinder>("USS & UXML Finder");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "UXML 및 USS 파일 내부의 에셋 참조(GUID) 중 존재하지 않는 깨진 에셋(MissingAssetReference)을 찾아냅니다.",
            MessageType.Info);

        if (GUILayout.Button("UXML / USS 깨진 참조 스캔", GUILayout.Height(40)))
        {
            ScanUIAssets();
        }
    }

    private static void ScanUIAssets()
    {
        // Assets 폴더 내의 모든 .uxml, .uss, .asset 파일을 직접 가져와 검사합니다.
        string[] allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);

        int errorCount = 0;
        int scannedCount = 0;
        Debug.Log("[UI Toolkit Validator] 스캔을 시작합니다...");

        foreach (var file in allFiles)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext != ".uxml" && ext != ".uss" && ext != ".asset") continue;

            // 경로 구분자 통일 (역슬래시와 슬래시 혼용 방지)
            string cleanFile = file.Replace('\\', '/');
            string cleanDataPath = Application.dataPath.Replace('\\', '/');
            string relativePath = "Assets" + cleanFile.Replace(cleanDataPath, "");

            errorCount += ValidateFile(file, relativePath);
            scannedCount++;
        }

        Debug.Log($"[UI Toolkit Validator] 총 {scannedCount}개의 UI 관련 파일을 스캔했습니다.");

        if (errorCount == 0)
        {
            Debug.Log("<color=green>[UI Toolkit Validator] 스캔 완료: 깨진 에셋 참조가 없습니다!</color>");
        }
        else
        {
            Debug.LogWarning($"[UI Toolkit Validator] 스캔 완료: 총 {errorCount}개의 깨진 에셋 참조를 검출했습니다. 콘솔창의 에러 로그를 확인해 주세요.");
        }
    }

    private static int ValidateFile(string fullPath, string relativePath)
    {
        int errors = 0;
        string[] lines = File.ReadAllLines(fullPath);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Match guid=xxxx 또는 guid: xxxx (32자리 16진수)
            var matches = Regex.Matches(line, @"guid[=:]\s*[""']?([a-fA-F0-9]{32})");
            foreach (Match match in matches)
            {
                string guid = match.Groups[1].Value;

                // Unity 내장 리소스 관련 특수 GUID(00000...)는 제외합니다.
                if (guid.StartsWith("00000000")) continue;

                // Unity 에셋 데이터베이스에서 해당 GUID가 유효한지 확인하고, 파일이 실제로 존재하는지도 체크합니다.
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 캐시 문제로 AssetDatabase가 이전 경로를 리턴할 수 있으므로, 실제 파일이 존재하는지 검사합니다.
                if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
                {
                    string cleanLine = line.Trim();
                    Debug.LogError(
                        $"<color=red><b>[UI Toolkit Validator]</b></color> 깨진 참조(MissingAssetReference) 발견!\n" +
                        $"<b>파일:</b> <a href=\"{relativePath}\">{relativePath}</a> (Line {i + 1})\n" +
                        $"<b>깨진 GUID:</b> {guid}\n" +
                        $"<b>줄 내용:</b> {cleanLine}"
                    );
                    errors++;
                }
            }
        }

        return errors;
    }
}