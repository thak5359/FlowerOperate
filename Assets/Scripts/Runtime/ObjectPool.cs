using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // 추가
using UnityEngine.ResourceManagement.AsyncOperations; // 추가


public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [SerializeField]
    private AssetReference prefabReference; // GameObject 대신 AssetReference 사용

    private GameObject loadedPrefab; // 로드된 프리팹 저장용
    private Queue<Item> poolingObjectQueue = new Queue<Item>();

    async void Awake() // async로 변경하여 로딩을 기다립니다.
    {
        Instance = this;

        // 2. 어드레서블로 프리팹 로드 시작
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefabReference);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedPrefab = handle.Result;
            Initialize(10);
        }
        else
        {
            Debug.LogError("프리팹 로드 실패!");
        }
    }

    private Item CreateNewObject()
    {
        // 로드된 프리팹을 인스턴스화합니다.
        var obj = Instantiate(loadedPrefab, transform);
        var itemArea = obj.GetComponent<Item>();

        obj.SetActive(false);
        return itemArea;
    }

    private void Initialize(int count)
    {
        for (int i = 0; i < count; i++)
        {
            poolingObjectQueue.Enqueue(CreateNewObject());
        }
    }

    public static Item GetObject()
    {
        // 로딩이 아직 안 끝났을 경우를 대비한 방어 코드
        if (Instance.loadedPrefab == null)
        {
            Debug.LogWarning("아직 프리팹 로딩 중입니다!");
            return null;
        }

        if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObject();
            newObj.transform.SetParent(null);
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static void ReturnObject(Item item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(item);
    }

    private void OnDestroy()
    {
        // 3. 풀이 파괴될 때 어드레서블 핸들 해제 (메모리 관리)
        if (prefabReference.IsValid())
        {
            prefabReference.ReleaseAsset();
        }
    }
}