using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Constant;

using Cysharp.Threading.Tasks;

public static class AddressableManager
{
    // [핵심 최적화 1] 딕셔너리의 Key를 FixedString으로 변경하여 검색 시 GC 발생 원천 차단
    private static Dictionary<FixedString64Bytes, List<AsyncOperationHandle>> labelHandles
        = new Dictionary<FixedString64Bytes, List<AsyncOperationHandle>>();

    // [핵심 최적화 2] 반환형을 UniTask로 변경하고, label 매개변수도 FixedString으로 통일
    public static async UniTask<T> LoadAssetAsync<T>(FixedString64Bytes address, FixedString64Bytes label = default) where T : Object
    {
        // Addressables API 자체는 string을 요구하므로 여기서만 ToString()을 허용합니다.
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address.ToString());

        await handle.Task; // UniTask 환경에서도 일반 Task를 await 할 수 있습니다.

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // label이 비어있지 않은(IsEmpty == false) 경우에만 그룹으로 묶습니다.
            if (!label.IsEmpty)
            {
                if (!labelHandles.ContainsKey(label))
                {
                    labelHandles[label] = new List<AsyncOperationHandle>();
                }
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

    // 단일 에셋 메모리 해제 
    public static void ReleaseAsset<T>(T asset)
    {
        if (asset != null) Addressables.Release(asset);
    }

    // 라벨 단위의 메모리 일괄 해제
    public static void ReleaseAllByLabel(FixedString64Bytes label)
    {
        if (label.IsEmpty) return; // 방어 로직

        // ToString() 없이 구조체 자체로 고속 검색!
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
            labelHandles.Remove(label); // 딕셔너리에서 제거
            Debug.Log($"[Addressables] 라벨 '{label}'의 모든 에셋이 메모리에서 해제되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[Addressables] 해제할 라벨 '{label}'을 찾을 수 없습니다.");
        }
    }
}