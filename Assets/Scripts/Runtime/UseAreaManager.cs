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
    public void StartCharging(Transform playerTransform, Vector2 heading);
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

    List<Vector3> AreaC2 = new List<Vector3>()
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

    List<Vector3> AreaC3 = new List<Vector3>()
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

    List<Vector3> AreaC4 = new List<Vector3>()
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

    List<Vector3> AreaC5 = new List<Vector3>()
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
        if (_loadedPrefab == null)
        {
            Debug.LogError("Addressable Prefab이 아직 로드되지 않았습니다!");
            return null;
        }

        // 초기화 단계에서는 _originTransform이 null일 수 있으므로 Instantiate 시 부모를 지정하지 않거나 체크해야 함
        GameObject go = UnityEngine.Object.Instantiate(_loadedPrefab);
        go.SetActive(false);

        var component = go.GetComponent<UseAreaFunction>();
        if (component == null)
        {
            Debug.LogError("Prefab에 UseAreaFunction 컴포넌트가 없습니다!");
        }
        return component;
    }

    #endregion



    public void StartCharging(Transform playerTransform, Vector2 heading)
    {
        if (_isCharging) return;
        _isCharging = true;

        _originTransform = playerTransform; // 참조 저장
        _currentHeading = heading;         // 방향 저장
        _chargeStartTime = Time.time;
    }

    void ITickable.Tick()  // 모았다가...
    {
        if (_loadedPrefab == null || !_isCharging) return;

        elapsed = Time.time - _chargeStartTime;

        int level = Mathf.Min((int)(elapsed / charTimePerPhase) + 1, 5);

        // 2. 현재 아이템 종류와 레벨에 맞는 데이터 가져오기
        List<Vector3> rawOffsets = GetAreaList(_hotbar.PointingItemId, level);

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

            // 4. 화면에 영역 표시 (기존 targetLockON 활용)
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

    // [수정 위치] Fire() 메서드: 현재 활성화된 영역의 좌표를 출력하는 로직 추가
    public void Fire() // Context.canceled일 때 발사!
    {
        if (!_isCharging) return; // 차징이 시작되지 않았으면 무시

        // --- 좌표 디버깅 로그 시작 ---
        Debug.Log($"<color=yellow>[UseArea Debug]</color> 현재 생성된 영역 개수: {_activeObjects.Count}");

        int index = 0;
        foreach (var obj in _activeObjects)
        {
            if (obj != null)
            {
                Debug.Log($"[{index}] 월드 좌표: {obj.transform.position} | 로컬 좌표: {obj.transform.localPosition}");
            }
            index++;
        }
        // --- 좌표 디버깅 로그 끝 ---

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
        if (returned == null) return;

        returned.gameObject.SetActive(false);

        // _originTransform이 있을 때만 부모로 설정 (없으면 최상위로)
        if (_originTransform != null)
        {
            returned.transform.SetParent(_originTransform);
        }
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
