using System.IO;
using UnityEngine;

public static class FileDataHandler
{
    public static void SaveJson<T>(T data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"[저장 성공] 경로: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[저장 실패] {e.Message}");
        }
    }

    public static T LoadJson<T>(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[로드] 저장 파일이 존재하지 않습니다: {path}");
            return default;
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[로드 실패] {e.Message}");
            return default;
        }
    }
}
