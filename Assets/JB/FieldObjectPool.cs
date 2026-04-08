using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PoolManager가 관리할 각 프리팹의 설정 정보를 담는 클래스
[System.Serializable]
public class Pool
{
    public string tag;          // 이 풀을 식별할 태그 (예: "Bullet", "Enemy")
    public GameObject prefab;   // 이 풀에서 관리할 프리팹
    public int size;            // 초기 풀 크기
}

public class FieldObjectPool : MonoBehaviour
{
    public static FieldObjectPool Instance { get; private set; }

    [SerializeField] private List<Pool> pools; // 관리할 풀들의 목록 (인스펙터에서 설정)

    // 실제 오브젝트 풀들을 저장할 딕셔너리
    // Key: 풀 태그 (string), Value: 해당 태그의 오브젝트를 담는 Queue
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePools(); // 모든 풀 초기화
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스 방지
        }
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // 설정된 크기만큼 미리 오브젝트를 생성하여 풀에 추가
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // 일단 비활성화 상태로 풀에 보관
                objectPool.Enqueue(obj);
            }
            // 딕셔너리에 새로운 풀 추가
            poolDictionary.Add(pool.tag, objectPool);
        }
        Debug.Log("Object Pools Initialized.");
    }

    /// <summary>
    /// 지정된 태그의 오브젝트를 풀에서 가져옵니다.
    /// </summary>
    /// <param name="tag">가져올 오브젝트의 풀 태그</param>
    /// <param name="position">오브젝트가 활성화될 위치</param>
    /// <param name="rotation">오브젝트가 활성화될 회전값</param>
    ///<returns>풀에서 가져온 GameObject</returns>
    public GameObject SpawnObject(string tag, Vector3 position, Quaternion rotation)
    {
        // 해당 태그의 풀이 딕셔너리에 없으면 에러
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[tag];

        // 풀에 오브젝트가 남아있지 않으면 새로 생성 (옵션: 경고 메시지 출력)
        if (objectPool.Count == 0)
        {
            Debug.LogWarning("Pool for tag " + tag + " is empty. Instantiating new object.");
            // 여기서 새로운 오브젝트를 Instantiate하고 풀에 추가할지, 바로 반환할지 결정
            // 이 예시에서는 새로 만들어서 반환합니다.
            // 주의: 이 경우 풀의 size 설정을 넘어가게 됩니다.
            // 더 엄격하게 관리하려면 여기서 null을 반환하거나,
            // 풀 크기를 자동으로 늘리는 로직을 추가할 수 있습니다.
            GameObject newObj = Instantiate(GetPoolPrefab(tag));
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            newObj.SetActive(true);
            return newObj;
        }
        
        GameObject objectToSpawn = objectPool.Dequeue(); // 풀에서 하나 꺼내옴

        // 오브젝트 활성화 및 위치/회전 설정
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 부모 설정 초기화 (씬의 루트에 놓음)
        objectToSpawn.transform.SetParent(null); 
        
        // --- 여기에서 오브젝트의 상태를 초기화하는 로직을 추가해야 합니다. ---
        // 예: objectToSpawn.GetComponent<Bullet>().ResetBulletState();
        // 예: objectToSpawn.GetComponent<Enemy>().Initialize(someData);

        return objectToSpawn;
    }

    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// </summary>
    /// <param name="tag">반환할 오브젝트의 풀 태그</param>
    /// <param name="obj">반환할 GameObject</param>
    public void ReturnObjectToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            Destroy(obj); // 풀이 없으면 그냥 파괴
            return;
        }
        
        // --- 여기에서 오브젝트의 상태를 초기화하고 비활성화하는 로직을 추가해야 합니다. ---
        // 예: obj.GetComponent<Bullet>().OnReturnToPool();
        obj.SetActive(false); // 비활성화
        obj.transform.SetParent(this.transform); // FieldObjectPool 하위로 이동하여 관리 용이

        poolDictionary[tag].Enqueue(obj); // 풀에 다시 넣음
    }

    // 풀 태그에 해당하는 프리팹을 찾기 위한 헬퍼 함수
    private GameObject GetPoolPrefab(string tag)
    {
        foreach (Pool pool in pools)
        {
            if (pool.tag == tag)
            {
                pool.prefab.SetActive(true);
                return pool.prefab;
            }
        }
        return null;
    }
}