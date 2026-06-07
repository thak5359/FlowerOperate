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
    public void StartCharging(in Transform playerTransform, in Vector2 heading);
    public void Fire();
}

public class UseAreaManager : IAsyncStartable, IDisposable, ITickable, IUseItem
{
    #region 영역범위 벡터 리스트


    static readonly List<Vector3> AreaOrigin = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
};

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

    private PlayerOwnItemDataManager _itemDataManager;
    private PlayerStateManager _playerState;
    private PlotManager _plotManager;


    private Transform _originTransform;
    private Vector2 _currentHeading;


    public float charTimePerPhase = 1.75f;


    private GameItem _cachedSelectedItem;
    private float _chargeStartTime;
    float elapsed;

    //private int currentChargeLevel = 0; // Charing >> default, 1, 2, 3, 4
    private List<GameObject> pool = new List<GameObject>();


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


    [Inject]
    void Constuct(PlayerOwnItemDataManager input_itemDataManager, PlayerStateManager input_playerStateManager, PlotManager input_plotManager)
    {
        _itemDataManager = input_itemDataManager;
        _playerState = input_playerStateManager;
        _plotManager = input_plotManager;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {

        _plotPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_PLOT);
        if (_plotPrefab != null) isPlotPrefabLoaded = true;

        _useAreaPrefab = await AddressableManager.LoadAssetAsync<GameObject>(ADDRESSABLE_USEAREA);
        if (_useAreaPrefab != null) isUseAreaPrefabLoaded = true;

        if (_useAreaPrefab != null)
        {
            InitializePool(80);
        }
        _activeObjects.Clear();

        if (_pool.Count > 0) isPoolInitialized = true;
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

    private GameItem GetCurrentSelectedItem()
    {
        int layer = _playerState.CurrentHotbarLayer.Value;
        int slot = _playerState.CurrentHotbarSlot.Value;

        var segment = _itemDataManager.GetInventorySegment(layer);
        if (slot < 0 || slot >= segment.Count) return null;

        return segment[slot];

        
    }

    // 수정할 위치: UseAreaManager.cs 내부의 StartCharging 메서드 수정
    // 변경 이유: 아이템이 없을 때 차징 스위치(_isCharging)가 켜진 채 리턴되는 버그를 수정하고, 플레이어 전광판의 상태도 함께 꺼지도록 안전하게 방어해요.

    public void StartCharging(in Transform playerTransform, in Vector2 heading)
    {
        if (_playerState.IsCharging.Value) return;
        if (!IsReady)
        {
            Debug.Log($"차징 시작 불가: 준비 상태 \n " +
                $"plot 프리펩 로드 = {isPlotPrefabLoaded} \n " +
                $"UseArea 프리펩 로드 = {isUseAreaPrefabLoaded} \n " +
                $"오브젝트 풀 로드 = {isPoolInitialized}");
            return;
        }

        // 💡 [수정 위치] 현재 가리키는 아이템 정보를 '먼저' 가져와 유효성 검사를 합니다.
        var selectedItem = GetCurrentSelectedItem();

        if (selectedItem == null || selectedItem.Count <= 0)
        {
            Debug.Log("사용할 수 있는 아이템이 없습니다.");

            return;
        }

        // 💡 아이템이 확실히 있을 때만 진짜 차징 프로세스를 시작해요!
        _playerState.IsCharging.Value = true;
        _cachedSelectedItem = selectedItem;

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
        try
        {
            if (_useAreaPrefab == null || !_playerState.IsCharging.Value) return;

            if (_cachedSelectedItem == null)
            {
                Debug.LogWarning("아이템이 없는데 차징 로직이 돌았습니다. 강제 종료합니다.");
                _playerState.IsCharging.Value = false;
                ClearActiveArea();
                return;
            }



            elapsed = Time.time - _chargeStartTime;

            // 아이템 종류에 따라 적절한 영역 데이터를 가져옵니다.
            List<Vector3> rawOffsets = null;

            if (_cachedSelectedItem is GearItem)
            {
                rawOffsets = GetAreaList();
            }
            else if (_cachedSelectedItem is FertilizerItem || _cachedSelectedItem.SubType == ItemSubType.Seed)
            {
                rawOffsets = GetHandAreaList();
            }
            else
            {
                // 도구나 소모품이 아닌 일반 아이템을 들고 차징할 경우 캔슬합니다!
                _playerState.IsCharging.Value = false;
                ClearActiveArea();
                return;
            }

            if (rawOffsets != null)
            {
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
        catch (Exception e)
        {
            Debug.LogError($"<color=red><b>[CRITICAL ERROR]</b></color> {e.StackTrace}");
            Debug.LogError($"<color=red><b>[CRITICAL ERROR]</b></color> {e.Message}");
        }
    }
    public void Fire() // Context.canceled, 버튼을 땠을 때 발사!
    {
        if (!_playerState.IsCharging.Value) return; // 차징이 시작되지 않았으면 무시

        if (_cachedSelectedItem == null || _cachedSelectedItem.Count <= 0)
        {
            Debug.Log("차징 도중 아이템이 사라졌습니다.");
            _playerState.IsCharging.Value = false;
            ClearActiveArea();
            return;
        }

        try
        {
            FireUseAreaFunction();
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red><b>[CRITICAL ERROR]</b></color> {e.StackTrace}");
        }
        finally
        {
            _playerState.IsCharging.Value = false;
            ClearActiveArea();
            _itemDataManager.NotifyDataChanged();
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
        _playerState.IsCharging.Value = false;
        ClearActiveArea();
        Debug.Log("캐릭터가 메모리에서 해제됨. 강제로 차징을 해제합니다.");
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
    public void FireUseAreaFunction()
    {

        if (_cachedSelectedItem == null)
        {
            Debug.LogError("발사 시도 중 아이템 정보가 없습니다!");
            return;
        }

        while (_activeObjects.Count > 0)
        {
            UseAreaFunction obj = _activeObjects.Pop();
            if (obj != null)
            {
                obj.FireFunc(ref _cachedSelectedItem, _plotManager, _plotPrefab);



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


    /// <summary>
    /// 해당 장비에 맞는 아이템 차지 레벨에 따른 영역 리스트를 반환합니다. Ctrl로 스왑이 활성화된 경우 첫번째는 한칸 고정입니다.
    /// </summary>
    /// <returns></returns>
    private List<Vector3> GetAreaList()
    {
        if (_cachedSelectedItem is not GearItem gear) return null;


        float chargeTime = gear.ChargeInfo.ChargeTime > 0 ? gear.ChargeInfo.ChargeTime : 0.1f; // Zero Split 방지
        int maxLevel = Mathf.Max(0, gear.ChargeInfo.ChargeAreas.Length - 1);

        int chargeLv = (int)Mathf.Clamp(elapsed / chargeTime, 0, maxLevel);
        if (chargeLv == 0 && _playerState.IsSwappingGearDefaultArea.Value)
        {
            return AreaOrigin; // Ctrl 누르고 있으면 첫 번째 레벨은 무조건 1칸
        }


        ChargeArea area = GetTargetArea(gear, chargeLv);

        switch (area)
        {
            case ChargeArea.Unknown:
                {
                    Debug.LogWarning($"알 수 없는 영역 타입입니다. 아이템 ID: {gear.Id}, 충전 레벨: {chargeLv}");
                    return null;
                }
            case ChargeArea.Default: return AreaOrigin;
            case ChargeArea.A1: return AreaA1;
            case ChargeArea.A2: { return AreaA2; }
            case ChargeArea.A3: return AreaA3;
            case ChargeArea.A4: return AreaA4;
            case ChargeArea.A5: return AreaA5;

            case ChargeArea.B1: return AreaB1;
            case ChargeArea.B2: return AreaB2;
            case ChargeArea.B3: return AreaB3;

            case ChargeArea.C1: return AreaC1;
            case ChargeArea.C2: return AreaC2;
            case ChargeArea.C3: return AreaC3;
            case ChargeArea.C4: return AreaC4;
            case ChargeArea.C5: return AreaC5;

            default:
                {
                    Debug.LogWarning($"알 수 없는 영역 타입입니다. " +
                        $"아이템 ID: {gear.Id}, 충전 레벨: {chargeLv}, ChargeArea : {gear.ChargeInfo.ChargeAreas}");
                    return null;
                }
        }
    }

    private List<Vector3> GetHandAreaList()
    {
        // 씨앗이거나 비료일 때만 동작하도록 이중 체크
        bool isHandItem = _cachedSelectedItem is FertilizerItem || _cachedSelectedItem.SubType == ItemSubType.Seed;
        if (!isHandItem) return null;

        // 경과 시간을 기준으로 0, 1, 2 레벨(최대 2)로 계산합니다.
        int chargeLv = (int)Mathf.Clamp(elapsed / HAND_CHARGETIME, 0, 2);

        // switch 식으로 파트너가 요청한 영역을 반환해요!
        return chargeLv switch
        {
            0 => AreaOrigin, // 1x1 (기본)
            1 => AreaA4,     // 3x3
            2 => AreaA5,     // 5x5
            _ => AreaOrigin
        };
    }

    private ChargeArea GetTargetArea(in GearItem item, float chargeLv)
    {
        var areas = item.ChargeInfo.ChargeAreas;


        if (areas == null || areas.Length == 0)
        {
            return ChargeArea.Unknown;
        }
        return areas[(int)chargeLv];
    }

}
