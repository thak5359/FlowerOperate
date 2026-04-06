using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using static Constant;

public interface IUseItem
    {
    public void StartCharging();
    public void Fire();
}

public class UseAreamanager : IAsyncStartable, IDisposable, ITickable, IUseItem
{
    #region 영역범위 벡터 리스트

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

    #endregion

    [Inject] private HotbarManager _hotbar; // 현재 아이템 확인용
    [Inject] private PlayerController _player; // 위치 및 방향 확인용, 실시간 추적 필요

    private bool _isCharging;
    private float _chargeStartTime;
    float elapsed;

    //private int currentChargeLevel = 0; // 기본, 1, 2, 3, 4
    private List<GameObject> pool = new List<GameObject>();

    Vector3 defaultArea = new Vector3(1, 0, 0);

    // 오른쪽으로 바라보는 기준으로 작성한 차지타임별 사용 벡터.

    private GameObject _loadedPrefab;
    private readonly Stack<UseAreaFunction> _pool = new(80); // 인스턴스화된 객체를 풀링해서 관리!
    private readonly Stack<UseAreaFunction> _activeObjects = new(80); // 현재 활성화된 객체를 관리하는 스택



    #region 초기화 및 오브젝트 풀링
    
    public async UniTask StartAsync(CancellationToken cancellation)
    {
        _loadedPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_USEAREA);

        if (_loadedPrefab != null)
        {
            InitializePool(80);
        }
        _activeObjects.Clear();
    }

    // pool에 객체 생성해서 UseAreFunction 컴포넌트로 관리.
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
        GameObject go = UnityEngine.Object.Instantiate(_loadedPrefab, _player.gameObject.transform);
        go.SetActive(false);
        return go.GetComponent<UseAreaFunction>();
    }

    #endregion

    public void OnUse(InputAction.CallbackContext context)
    {
        // 1. 버튼을 누르기 시작했을 때 (Started)
        if (context.started)
        {
            if(_isCharging) return; // 이미 차징 중이면 무시
            StartCharging();
        }

        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {
            Fire();
        }
    }

   
    public void StartCharging()  // context.started일때 모으기 시작
    {
        if (_isCharging) return; // 이미 차징 중이면 무시
        _isCharging = true;
        _chargeStartTime = Time.time; // 차징 시작 시간 기록 

    }
   
    void ITickable.Tick()  // 모았다가...
    {
        if (!_isCharging) return;

        elapsed = Time.time - _chargeStartTime;
        // 1. 차징 레벨 계산 (현재 아이템의 데이터 기반)
        // level = CalculateLevel(elapsed);

        // 2. 영역 표시 업데이트
        //UpdateTargetLockOn(level);

    }
   
    public void Fire() // Context.canceled일때 발사!
    {
        if (!_isCharging) return; // 차징이 시작되지 않았으면 무시
        
        FireUseAreaFunction(_hotbar.PointingItemId); // 현재 아이템 ID에 따라 발사 함수 호출


        _isCharging = false;

    }


    //외부에서 아이템을 사용하기 전에 영역을 미리 표시하는 함수. 조준!
    public void targetLockON(List<Vector3> vecList)
    {
        for (int i = 0; i < vecList.Count; i++)
        {
            UseAreaFunction obj;
            if (_pool.Count > 0)
            {
                 obj = _pool.Pop();
                obj.gameObject.SetActive(true);
                obj.gameObject.transform.position = vecList[i];
            }
            else
            {
                obj = CreateNewObject();
                obj.gameObject.SetActive(true);
                obj.gameObject.transform.position = vecList[i];
            }
            _activeObjects.Push(obj);
        }
    }

    // 3Vec을 회전시키는 용도의 함수
    private Vector3 RotateOffset(Vector3 offset, Vector2 heading)
    {
        if (heading == Vector2.right) return new Vector3(offset.x, 0, offset.y);
        if (heading == Vector2.left) return new Vector3(-offset.x, 0, -offset.y);
        if (heading == Vector2.up) return new Vector3(-offset.y, 0, offset.x);
        if (heading == Vector2.down) return new Vector3(offset.y, 0, -offset.x);
        return offset;
    }

    //발사!!
    public void FireUseAreaFunction(int itemId)
    {
        while (_activeObjects.Count > 0)
        {
            UseAreaFunction obj = _activeObjects.Pop();
            if (obj != null)
            {


                ReturnObject(obj);
            }
        }
    }


    public void ReturnObject(UseAreaFunction returned)
    {
        returned.gameObject.SetActive(false);
        returned.transform.SetParent(_player.transform);
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

        AddressableManager.ReleaseAsset(_loadedPrefab);
        Debug.Log("[UseAreaSpawner] 메모리에서 정상적으로 해제되었음!");
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
