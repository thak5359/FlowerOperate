using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Constant;



public static class AddressableManager
{

    private static Dictionary<string, List<AsyncOperationHandle>> labelHandles
        = new Dictionary<string, List<AsyncOperationHandle>>();


    public static async Task<T> LoadAssetAsync<T>(string address, string label = null) where T : Object
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (!string.IsNullOrEmpty(label))
            {
                if (!labelHandles.ContainsKey(label))
                    labelHandles[label] = new List<AsyncOperationHandle>();

                labelHandles[label].Add(handle);
            }
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[Addressables] 로드 실패: {address}");
            Addressables.Release(handle);
            return null;
        }
    }

    // 메모리 해제 
    public static void ReleaseAsset<T>(T asset)
    {
        if (asset != null) Addressables.Release(asset);
    }

    // 라벨 단위의 메모리 해제
    public static void ReleaseAllByLabel(string label)
    {
        if (labelHandles.TryGetValue(label, out List<AsyncOperationHandle> handles))
        {
            foreach (var handle in handles)
            {
                if (handle.IsValid()) // 핸들 유효성 체크
                {
                    Addressables.Release(handle);
                }
            }
            handles.Clear();
            labelHandles.Remove(label);
            Debug.Log($"[Addressables] 라벨 '{label}'의 모든 에셋이 메모리에서 해제되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[Addressables] 해제할 라벨 '{label}'을 찾을 수 없습니다.");
        }
    }
}