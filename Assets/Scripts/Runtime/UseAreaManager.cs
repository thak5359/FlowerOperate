using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using static Constant;

public class UseAreamanager : IAsyncStartable, IDisposable
{

    private int currentChargeLevel = 0; // 기본, 1, 2, 3, 4
    private List<GameObject> pool = new List<GameObject>();

    Vector3 defaultArea = new Vector3(1, 0, 0);

    // 오른쪽으로 바라보는 기준으로 영역 전개

    #region 괭이, 물뿌리개, 망치, 소모품 영역범위
    List<Vector3> AreaA1 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(2f, 0f, 0f),
};

    List<Vector3> AreaA2 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaA3 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 1f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f)
};

    List<Vector3> AreaA4 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),

    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
};

    List<Vector3> AreaA5 = new List<Vector3>()
{
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),


    new Vector3(4f, 2f, 0f),
    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f),
    new Vector3(4f, -2f, 0f),


    new Vector3(5f, 2f, 0f),
    new Vector3(5f, 1f, 0f),
    new Vector3(5f, 0f, 0f),
    new Vector3(5f, -1f, 0f),
    new Vector3(5f, -2f, 0f)
};
    #endregion

    #region 낫 영역범위
    List<Vector3> AreaB1 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaB2 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

};

    List<Vector3> AreaB3 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),
};
    #endregion

    #region 도끼 영역범위
    List<Vector3> AreaC1 = new List<Vector3>()
{
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),

    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),

    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaC2 = new List<Vector3>()
{
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),

    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),

    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),

    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f)
};

    List<Vector3> AreaC3 = new List<Vector3>()
{
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),

    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),

    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),

    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),

    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),

    new Vector3(2f, -2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f)
};

    List<Vector3> AreaC4 = new List<Vector3>()
{
    new Vector3(-4f, 1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, -1f, 0f),

    new Vector3(-3f, 2f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, -2f, 0f),

    new Vector3(-2f, 3f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -3f, 0f),

    new Vector3(-1f, 4f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -4f, 0f),

    new Vector3(0f, 4f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -4f, 0f),

    new Vector3(1f, 4f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -4f, 0f),

    new Vector3(2f, 3f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -3f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),

    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f)
};

    List<Vector3> AreaC5 = new List<Vector3>()
{

    new Vector3(-5f, 1f, 0f),
    new Vector3(-5f, 0f, 0f),
    new Vector3(-5f, -1f, 0f),

    new Vector3(-4f, 2f, 0f),
    new Vector3(-4f, 1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, -1f, 0f),
    new Vector3(-4f, -2f, 0f),

    new Vector3(-3f, 3f, 0f),
    new Vector3(-3f, 2f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, -2f, 0f),
    new Vector3(-3f, 3f, 0f),

    new Vector3(-2f, 4f, 0f),
    new Vector3(-2f, 3f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -3f, 0f),
    new Vector3(-2f, 4f, 0f),

    new Vector3(-1f, 5f, 0f),
    new Vector3(-1f, 4f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -4f, 0f),
    new Vector3(-1f, -5f, 0f),

    new Vector3(0f, 5f, 0f),
    new Vector3(0f, 4f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -4f, 0f),
    new Vector3(0f, -5f, 0f),

    new Vector3(1f, 5f, 0f),
    new Vector3(1f, 4f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -4f, 0f),
    new Vector3(1f, -5f, 0f),

    new Vector3(2f, 4f, 0f),
    new Vector3(2f, 3f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -3f, 0f),
    new Vector3(2f, -4f, 0f),

    new Vector3(3f, 3f, 0f),
    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),
    new Vector3(3f, -3f, 0f),

    new Vector3(4f, 2f, 0f),
    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f),
    new Vector3(4f, -2f, 0f),

    new Vector3(5f, 1f, 0f),
    new Vector3(5f, 0f, 0f),
    new Vector3(5f, -1f, 0f),
};
    #endregion


    private readonly Transform playerTransform; // ���Թ��� �θ� Ʈ������
    private GameObject _loadedPrefab;
    private readonly Stack<UseAreaFunction> _pool = new(80); // ����Ʈ���� ������ Ǯ���� �����ؿ�!

    // VContainer�� ���� �θ� �� Transform�� ���Թ޽��ϴ�.
    //[Inject]
    //private UseAreamanager(PlayerController pc)
    //{
    //    playerTransform = pc.gameObject.transform;
    //}

    //�񵿱�� Prefab ��������
    public async UniTask StartAsync(CancellationToken cancellation)
    {
        // ��巹���� �ε�
        _loadedPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_USEAREA);

        if (_loadedPrefab != null)
        {
            InitializePool(80);
        }
    }

    //Ǯ���ٰ� ����ǰ ���ųֱ� x80
    private void InitializePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _pool.Push(CreateNewObject());
        }
    }


    private UseAreaFunction CreateNewObject()
    {
        // MonoBehaviour�� �ƴϹǷ� UnityEngine.Object.Instantiate�� ��������� ȣ��
        GameObject go = UnityEngine.Object.Instantiate(_loadedPrefab, playerTransform);
        go.SetActive(false);
        return go.GetComponent<UseAreaFunction>();
    }

    public void RyoikiTenkai(List<Vector3> vecList)
    {
        for (int i = 0; i < vecList.Count; i++)
        {

            if (_pool.Count > 0)
            {
                var obj = _pool.Pop();
                obj.gameObject.SetActive(true);
                obj.gameObject.transform.position = vecList[i];
            }
            // Ǯ�� ���ڶ�� ���� ����
            var newObj = CreateNewObject();
            newObj.gameObject.SetActive(true);
            newObj.gameObject.transform.position = vecList[i];
        }
    }

    //���⼭ ����
    public void ReturnObject(UseAreaFunction returned)
    {
        returned.gameObject.SetActive(false);
        returned.transform.SetParent(playerTransform);
        _pool.Push(returned);
    }

    // OnDestroy ��� IDisposable.Dispose���� �޸� ����
    public void Dispose()
    {
        while (_pool.Count > 0)
        {
            var obj = _pool.Pop();
            if (obj != null) UnityEngine.Object.Destroy(obj.gameObject);
        }

        // ��巹���� ���� ���� (AddressableManager �̿�)
        AddressableManager.ReleaseAsset(_loadedPrefab);
        Debug.Log("[UseAreaSpawner] Ǯ�� �����Ǿ����ϴ�, ��Ʈ��!");
    }

    public enum Type
    {
        TypeA, TypeB, TypeC
    }

    //ItemObjectData
    public void DimensionExpansion()
    {
        //ItemObjectData에서 데이터를 출력




        foreach (var obj in pool) obj.SetActive(false);




    }


    private List<Vector3> GetAreaList(int itemId, int level)
    {



        return level switch
        {
            1 => AreaC1,
            2 => AreaC2,
            3 => AreaC3,
            4 => AreaC4,
            5 => AreaC5,
            _ => null
        };
    }

}
