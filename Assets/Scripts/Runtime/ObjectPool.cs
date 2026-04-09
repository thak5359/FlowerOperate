using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class PoolConfig
    {
        public string key;
        public AssetReference prefabReference;
        public int initialCount;
    }

    [SerializeField]
    private List<PoolConfig> poolConfigs = new List<PoolConfig>();

    private Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            await InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializePools()
    {
        List<Task> loadTasks = new List<Task>();

        foreach (var config in poolConfigs)
        {
            loadTasks.Add(LoadAndInitialize(config));
        }

        await Task.WhenAll(loadTasks);
    }

    private async Task LoadAndInitialize(PoolConfig config)
    {
        if (string.IsNullOrEmpty(config.key))
        {
            Debug.LogError("Pool key is null or empty!");
            return;
        }

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(config.prefabReference);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedPrefabs[config.key] = handle.Result;
            poolDictionary[config.key] = new Queue<GameObject>();
            
            for (int i = 0; i < config.initialCount; i++)
            {
                poolDictionary[config.key].Enqueue(CreateNewObject(config.key));
            }
        }
        else
        {
            Debug.LogError($"Failed to load prefab for key: {config.key}");
        }
    }

    private GameObject CreateNewObject(string key)
    {
        if (!loadedPrefabs.TryGetValue(key, out GameObject prefab))
        {
            return null;
        }

        var obj = Instantiate(prefab, transform);
        
        // Ensure PoolableObject exists to track the key
        var poolable = obj.GetComponent<PoolableObject>();
        if (poolable == null)
        {
            poolable = obj.AddComponent<PoolableObject>();
        }
        poolable.PoolKey = key;

        obj.SetActive(false);
        return obj;
    }

    public static GameObject GetObject(string key)
    {
        if (Instance == null) return null;

        if (!Instance.poolDictionary.TryGetValue(key, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"Pool with key {key} not found!");
            return null;
        }

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
        }
        else
        {
            obj = Instance.CreateNewObject(key);
        }

        if (obj != null)
        {
            obj.transform.SetParent(null);
            obj.SetActive(true);
        }
        return obj;
    }

    public static void ReturnObject(GameObject obj)
    {
        if (Instance == null || obj == null) return;

        var poolable = obj.GetComponent<PoolableObject>();
        if (poolable == null)
        {
            Debug.LogWarning($"Trying to return an object that is not poolable: {obj.name}. Destroying.");
            Destroy(obj);
            return;
        }

        string key = poolable.PoolKey;

        if (!string.IsNullOrEmpty(key) && Instance.poolDictionary.TryGetValue(key, out Queue<GameObject> queue))
        {
            obj.SetActive(false);
            obj.transform.SetParent(Instance.transform);
            queue.Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"Trying to return object with invalid key: {key}. Destroying.");
            Destroy(obj);
        }
    }

    private void OnDestroy()
    {
        foreach (var config in poolConfigs)
        {
            if (config.prefabReference.IsValid())
            {
                config.prefabReference.ReleaseAsset();
            }
        }
    }
}
