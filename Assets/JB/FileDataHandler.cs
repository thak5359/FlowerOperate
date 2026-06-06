using MemoryPack;
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

    public static void SaveBinary<T>(T data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        byte[] bytes = MemoryPackSerializer.Serialize(data);
        File.WriteAllBytes(path, bytes);

        Debug.Log($"[FileDataHandler] Binary 저장 완료: {path}");
    }

    public static T LoadBinary<T>(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[FileDataHandler] Binary 저장 파일 없음: {path}");
            return default;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            return MemoryPackSerializer.Deserialize<T>(bytes);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileDataHandler] Binary 로드 중 에러 발생(버전 불일치 또는 데이터 손상): {e.Message}");
            return default;
        }
    }
}
