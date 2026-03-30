using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; //  ? 
using UnityEngine.ResourceManagement.AsyncOperations; //  ? 


public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [SerializeField]
    private AssetReference prefabReference; // GameObject     AssetReference    

    private GameObject loadedPrefab; //  ε               
    private Queue<ItemDataContainer> poolingObjectQueue = new Queue<ItemDataContainer>();

    async void Awake() // async        ?   ε      ?  ? .
    {
        Instance = this;

        // 2.   ?               ε      
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefabReference);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedPrefab = handle.Result;
            Initialize(10);
        }
        else
        {
            Debug.LogError("        ε      !");
        }
    }

    private void Initialize(int count)
    {
        for (int i = 0; i < count; i++)
        {
            poolingObjectQueue.Enqueue(CreateNewObject());
        }
    }

    private ItemDataContainer CreateNewObject()
    {
        //  ε             ν  ? ? ?? .
        var obj = Instantiate(loadedPrefab, transform);
        var itemArea = obj.GetComponent<ItemDataContainer>();

        obj.SetActive(false);
        return itemArea;
    }

    

    public static ItemDataContainer GetObject()
    {
        //  ε                     츦            ? 
        if (Instance.loadedPrefab == null)
        {
            Debug.LogWarning("             ε     ?? !");
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

    public static void ReturnObject(ItemDataContainer item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(item);
    }

    private void OnDestroy()
    {
        // 3. ?    ı         ?      ?       ( ?      )
        if (prefabReference.IsValid())
        {
            prefabReference.ReleaseAsset();
        }
    }
}