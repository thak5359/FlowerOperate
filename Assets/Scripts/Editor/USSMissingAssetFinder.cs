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
            "UXML 및 USS 파일 내부의 에셋 참조(GUID) 중 존재하지 않는 깨진 에셋 및 폰트 참조 누락/오류를 찾아냅니다.", 
            MessageType.Info);

        if (GUILayout.Button("UXML / USS 깨진 참조 및 폰트 누락 스캔", GUILayout.Height(40)))
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
            Debug.Log("<color=green>[UI Toolkit Validator] 스캔 완료: 깨진 참조 및 폰트 오류가 없습니다!</color>");
        }
        else
        {
            Debug.LogWarning($"[UI Toolkit Validator] 스캔 완료: 총 {errorCount}개의 에러/경고가 검출되었습니다. 콘솔창의 로그를 확인해 주세요.");
        }
    }

    private static int ValidateFile(string fullPath, string relativePath)
    {
        int errors = 0;
        string[] lines = File.ReadAllLines(fullPath);
        
        bool hasKoreanText = false;
        bool hasFontDefinition = false;
        
        string ext = Path.GetExtension(fullPath).ToLower();
        bool isUxml = (ext == ".uxml");

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // 1. Broken GUID Reference 검사
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

                // 한국어 폰트 에셋(SDF) 참조 여부 확인 (경로에 font 또는 sdf가 포함된 .asset 파일인 경우 인식)
                if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".asset") && 
                    (assetPath.ToLower().Contains("font") || assetPath.ToLower().Contains("sdf")))
                {
                    hasFontDefinition = true;
                }
            }

            // 2. Raw TTF/OTF 폰트 사용 검출 (m_AtlasTextures null 버그 유발)
            if (Regex.IsMatch(line, @"-unity-font:\s*url\([^)]*\.(ttf|otf)[^)]*\)"))
            {
                string cleanLine = line.Trim();
                Debug.LogError(
                    $"<color=orange><b>[UI Toolkit Validator]</b></color> 폰트 참조 오류 발견! raw .ttf/.otf 직접 사용\n" +
                    $"<b>파일:</b> <a href=\"{relativePath}\">{relativePath}</a> (Line {i + 1})\n" +
                    $"<b>상세:</b> UI Toolkit에서 raw 폰트를 직접 참조하면 런타임에 m_AtlasTextures null 에러가 발생합니다. 대신 SDF Font Asset을 생성하여 -unity-font-definition을 설정하세요.\n" +
                    $"<b>줄 내용:</b> {cleanLine}"
                );
                errors++;
            }

            // 3. 폰트 정의 초기화 검출 (initial/none/null 설정으로 인해 기본 폰트로 폴백되어 깨짐 유발)
            if (Regex.IsMatch(line, @"-unity-font-definition:\s*(initial|none|null)\b"))
            {
                string cleanLine = line.Trim();
                Debug.LogError(
                    $"<color=red><b>[UI Toolkit Validator]</b></color> 폰트 초기화 설정 검출 (-unity-font-definition: initial/none/null)\n" +
                    $"<b>파일:</b> <a href=\"{relativePath}\">{relativePath}</a> (Line {i + 1})\n" +
                    $"<b>상세:</b> 폰트 정의가 초기화되어 기본 폰트(LiberationSans)로 폴백됩니다. 한글이 포함된 엘리먼트라면 글자가 깨지거나 null 에러가 발생할 수 있습니다.\n" +
                    $"<b>줄 내용:</b> {cleanLine}"
                );
                errors++;
            }

            // 4. 한글 텍스트 존재 여부 검사 (속성값에 한글이 있는 경우)
            if (isUxml && Regex.IsMatch(line, @"text\s*=\s*""[^""]*[가-힣]+[^""]*"""))
            {
                hasKoreanText = true;
            }
        }

        // 5. UXML 파일 내부에 한글 텍스트는 있고 한국어 폰트 정의(SDF)가 설정되지 않은 경우 경고
        if (isUxml && hasKoreanText && !hasFontDefinition)
        {
            Debug.LogWarning(
                $"<color=yellow><b>[UI Toolkit Validator]</b></color> 한글용 폰트 정의 누락 위험 검출!\n" +
                $"<b>파일:</b> <a href=\"{relativePath}\">{relativePath}</a>\n" +
                $"<b>상세:</b> UXML 내부에 한글 텍스트가 존재하지만, 한국어 SDF 폰트 정의가 인라인 스타일로 설정되지 않았습니다. 외부 USS 파일에서 폰트를 정의했는지 확인해 주세요."
            );
        }

        return errors;
    }
}                            