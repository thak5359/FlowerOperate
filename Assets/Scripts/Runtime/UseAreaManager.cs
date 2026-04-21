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
    public void StartCharging(in Transform playerTransform,in Vector2 heading);
    public void Fire();
}

    

public class UseAreaManager : IAsyncStartable, IDisposable, ITickable, IUseItem
{
    #region 영역범위 벡터 리스트

    #region 괭이, 물뿌리개, 망치, 소모품 영역범위
    static readonly List<Vector3> AreaA1 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(2f, 0f, 0f),
};

    static readonly List<Vector3> AreaA2 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    static readonly List<Vector3> AreaA3 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 1f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f)
};

    static readonly List<Vector3> AreaA4 = new List<Vector3>()
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

    static readonly List<Vector3> AreaA5 = new List<Vector3>()
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
    static readonly List<Vector3> AreaB1 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    static readonly List<Vector3> AreaB2 = new List<Vector3>()
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

    static readonly List<Vector3> AreaB3 = new List<Vector3>()
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
    static readonly List<Vector3> AreaC1 = new List<Vector3>()
{
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 1f, 0f),

    new Vector3(0f, -1f, 0f),
    //new Vector3(0f, 0f, 0f), // 캐릭터 위치는 제외
    new Vector3(0f, 1f, 0f),


    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, 1f, 0f)
};

    static readonly List<Vector3> AreaC2 = new List<Vector3>()
{
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, 1f, 0f),

    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 2f, 0f),

    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -1f, 0f),
    //new Vector3(0f, 0f, 0f), // 캐릭터 위치는 제외
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, 2f, 0f),

    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 2f, 0f),

    new Vector3(2f, -1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, 1f, 0f)
};

    static readonly List<Vector3> AreaC3 = new List<Vector3>()
{
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, 1f, 0f),

    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 2f, 0f),

    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 3f, 0f),

    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -1f, 0f),
    //new Vector3(0f, 0f, 0f), // 캐릭터 위치는 제외
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 3f, 0f),


    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 3f, 0f),

    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 2f, 0f),

    new Vector3(3f, -1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, 1f, 0f)
};

    static readonly List<Vector3> AreaC4 = new List<Vector3>()
{
    new Vector3(-4f, -1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, 1f, 0f),

    new Vector3(-3f, -2f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 2f, 0f),

    new Vector3(-2f, -3f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 3f, 0f),

    new Vector3(-1f, -4f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 4f, 0f),

    new Vector3(0f, -4f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -1f, 0f),
    //new Vector3(0f, 0f, 0f), // 캐릭터 위치는 제외
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 4f, 0f),

    new Vector3(1f, -4f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 4f, 0f),

    new Vector3(2f, -3f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 3f, 0f),

    new Vector3(3f, -2f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 2f, 0f),

    new Vector3(4f, -1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, 1f, 0f)
};

    static readonly List<Vector3> AreaC5 = new List<Vector3>()
{

    new Vector3(-5f, -1f, 0f),
    new Vector3(-5f, 0f, 0f),
    new Vector3(-5f, 1f, 0f),

    new Vector3(-4f, -2f, 0f),
    new Vector3(-4f, -1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, 1f, 0f),
    new Vector3(-4f, 2f, 0f),

    new Vector3(-3f, -3f, 0f),
    new Vector3(-3f, -2f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 2f, 0f),
    new Vector3(-3f, 3f, 0f),

    new Vector3(-2f, -4f, 0f),
    new Vector3(-2f, -3f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 3f, 0f),
    new Vector3(-2f, 4f, 0f),

    new Vector3(-1f, -5f, 0f),
    new Vector3(-1f, -4f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 4f, 0f),
    new Vector3(-1f, 5f, 0f),

    new Vector3(0f, -5f, 0f),
    new Vector3(0f, -4f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -1f, 0f),
    //new Vector3(0f, 0f, 0f), // 캐릭터 위치는 제외
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 4f, 0f),
    new Vector3(0f, 5f, 0f),

    new Vector3(1f, -5f, 0f),
    new Vector3(1f, -4f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 4f, 0f),
    new Vector3(1f, 5f, 0f),

    new Vector3(2f, -4f, 0f),
    new Vector3(2f, -3f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 3f, 0f),
    new Vector3(2f, 4f, 0f),

    new Vector3(3f, -3f, 0f),
    new Vector3(3f, -2f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 3f, 0f),

    new Vector3(4f, -2f, 0f),
    new Vector3(4f, -1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 2f, 0f),

    new Vector3(5f, -1f, 0f),
    new Vector3(5f, 0f, 0f),
    new Vector3(5f, 1f, 0f)
};
    #endregion

    #endregion

    [Inject] private HotbarManager _hotbar; // 현재 아이템 확인용

    private Transform _originTransform;
    private Vector2 _currentHeading;


    public float charTimePerPhase = 1.75f;
    private bool _isCharging;
    private float _chargeStartTime;
    float elapsed;

    //private int currentChargeLevel = 0; // Charing >> default, 1, 2, 3, 4
    private List<GameObject> pool = new List<GameObject>();

    Vector3 defaultArea = new Vector3(1, 0, 0);

    // 오른쪽으로 바라보는 기준으로 작성한 차지타임별 사용 벡터.


    private GameObject _plotPrefab;
    private GameObject _useAreaPrefab;
    
    bool isPlotPrefabLoaded = false;
    bool isUseAreaPrefabLoaded = false;
    bool isPoolInitialized = false;
    public bool IsReady => isPlotPrefabLoaded && isUseAreaPrefabLoaded && isPoolInitialized;

    private readonly Stack<UseAreaFunction> _pool = new(80); // 인스턴스화된 객체를 풀링해서 관리!
    private readonly Stack<UseAreaFunction> _activeObjects = new(80); // 현재 활성화된 객체를 관리하는 스택

    #region 초기화 및 오브젝트 풀링
    
    public async UniTask StartAsync(CancellationToken cancellation)
    {

        _plotPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_PLOT);
        if (_plotPrefab != null) isPlotPrefabLoaded = true; 

        _useAreaPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_USEAREA);
        if (_useAreaPrefab != null ) isUseAreaPrefabLoaded = true;

        if (_useAreaPrefab != null)
        {
            InitializePool(80);
        }
        _activeObjects.Clear();

        if(_pool.Count > 0) isPoolInitialized = true;

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
        if (_useAreaPrefab == null)
        {
            Debug.LogError("Addressable Prefab이 아직 로드되지 않았습니다!");
            return null;
        }

        GameObject go = UnityEngine.Object.Instantiate(_useAreaPrefab);
        go.SetActive(false);

        var component = go.GetComponent<UseAreaFunction>();
        if (component == null)
        {
            Debug.LogError("Prefab에 UseAreaFunction 컴포넌트가 없습니다!");
        }
        return component;
    }
    #endregion


    public void StartCharging( in Transform playerTransform, in Vector2 heading)
    {
        if (_isCharging) return;
        if ( !IsReady)
        {
            Debug.Log($"차징 시작 불가: 준비 상태 \n plot 프리펩 로드 = {isPlotPrefabLoaded} \n UseArea 프리펩 로드 = {isUseAreaPrefabLoaded} \n 오브젝트 풀 로드 = {isPoolInitialized}");
            return;
        }


        _isCharging = true;

        _originTransform = playerTransform; // 참조 저장

        if (heading == Vector2.zero) // 방향이 없는 경우 기본값으로 정면으로 설정
        {
            _currentHeading = Vector2.down;
        }
        else
        {
            _currentHeading = heading;
        }
        
        _chargeStartTime = Time.time;
    }

    void ITickable.Tick()  // 모았다가...
    {
        if (_useAreaPrefab == null || _isCharging == false) return;

        elapsed = Time.time - _chargeStartTime;

        int level = Mathf.Min((int)(elapsed / charTimePerPhase) + 1, 5);

        // 2. 현재 아이템 종류와 레벨에 맞는 데이터 가져오기 ( 현재는 핫슬롯이 가리키는 것의 아이템 데이터를 가져옴.
        List<Vector3> rawOffsets = GetAreaList(_hotbar.PointingSlot+1, level);

        if (rawOffsets != null)
        {
            // 3. 캐릭터 방향(Heading)에 맞춰 좌표 회전 및 월드 좌표 계산
            List<Vector3> worldPositions = new List<Vector3>();
            foreach (var offset in rawOffsets)
            {
                Vector3 rotated = RotateOffset(offset, _currentHeading);
                // 소수점 반올림으로 그리드 스냅 적용
                Vector3 snapPos = new Vector3(
                    Mathf.Round(_originTransform.position.x + rotated.x),
                    0.15f,
                    Mathf.Round(_originTransform.position.z + rotated.z)
                );
                worldPositions.Add(snapPos);
            }

            // 4. 화면에 영역 표시
            UpdateVisualArea(worldPositions);
        }


    }
    private void UpdateVisualArea(List<Vector3> worldPositions)
    {
        // 기존에 켜져있던 애들을 일단 다 끄고 다시 배치 (비효율적일 수 있으나 현재 구조에서 가장 확실함)
        while (_activeObjects.Count > 0)
        {
            ReturnObject(_activeObjects.Pop());
        }

        foreach (var pos in worldPositions)
        {
            UseAreaFunction obj = (_pool.Count > 0) ? _pool.Pop() : CreateNewObject();
            obj.gameObject.SetActive(true);
            obj.transform.position = pos;
            _activeObjects.Push(obj);
        }
    }




    public void Fire() // Context.canceled, 버튼을 땠을 때 발사!
    {
        if (!_isCharging) return; // 차징이 시작되지 않았으면 무시

        try
        {
            FireUseAreaFunction(_hotbar.PointingSlot + 1);
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red><b>[CRITICAL ERROR]</b></color> {e.StackTrace}");
        }
        finally
        {
            _isCharging = false;
            ClearActiveArea();
        }
    }

    private void ClearActiveArea()
    {
        while (_activeObjects.Count > 0)
        {
            ReturnObject(_activeObjects.Pop());
        }

        Debug.Log("영역 청소!");
    }

    public void CancelCharging()
    {
        _isCharging = false;
        ClearActiveArea();
        Debug.Log("캐릭터가 메모리에서 해제됨. 강제로 차징이 취소되었습니다.");
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
                obj.FireFuncTest(itemId, _plotPrefab);

                ReturnObject(obj);
            }
        }
    }


    public void ReturnObject(UseAreaFunction returned)
    {
        if (returned == null) return;

        returned.gameObject.SetActive(false);

        // _originTransform이 있을 때만 부모로 설정 (없으면 최상위로)
        if (_originTransform != null)
        {
            returned.transform.SetParent(_originTransform);
        }
        _pool.Push(returned);
    }

    public void Dispose()
    {
        while (_pool.Count > 0)
        {
            var obj = _pool.Pop();
            if (obj != null) UnityEngine.Object.Destroy(obj.gameObject);
        }

        AddressableManager.ReleaseAsset(_useAreaPrefab);
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
        if(itemId ==1) // 괭이, 물뿌리개, 망치, 소모품
        {
            return level switch
            {
                1 => AreaA1,
                2 => AreaA2,
                3 => AreaA3,
                4 => AreaA4,
                5 => AreaA5,
                _ => null
            };
        }
        else if(itemId == 2) // 낫
        {
            return level switch
            {
                1 => AreaB1,
                2 => AreaB2,
                3 => AreaB3,
                _ => null
            };
        }
        else if (itemId == 3) // 도끼
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
        else
        {
            Debug.LogWarning($"아이템 ID {itemId}에 대한 영역 데이터가 없습니다.");
            return null;
        }

    }

}
