using MemoryPack;
using R3;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using static Constant;
using VContainer.Unity;

public enum ContainerType
{
    INVENTORY,
    STORAGE,
    SELLING
}


#region PlayerOwnItemDataManager (플레이어 소유 아이템 관리 매니저)
/// <summary>
/// 플레이어의 인벤토리, 창고, 판매 상자 등 모든 소유 아이템의 상태를 관리하는 매니저 클래스입니다.
/// 데이터의 변경(추가, 삭제, 이동 등)이 발생하면 스트림을 통해 UI 등에 변경 사항을 알립니다.
/// </summary>
[Serializable]
public class PlayerOwnItemDataManager : IInitializable, IDisposable
{
    #region Fields & Properties
    [SerializeField]
    protected ItemInstantData _Data = new ItemInstantData();


    /// <summary>
    /// 원본 데이터 접근자 (Ref 반환으로 구조체 복사 방지)
    /// </summary>
    public ref ItemInstantData GetData => ref _Data;


    /// <summary>
    /// 현재 활성화된 박스 번호 (기본값 0)
    /// </summary>
    [SerializeField] private int _currentBoxIndex = 0;
    public int CurrentBoxIndex { get => _currentBoxIndex; set => _currentBoxIndex = value; }

    protected virtual ContainerType currentType => ContainerType.INVENTORY;
    #endregion

    #region Reactive Streams (반응형 상태 관리)

    private readonly Subject<int> inventoryRevisionChanged = new Subject<int>();
    private int inventoryRevision;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    /// <summary>
    /// Inventory, Storage, SellingBox 등 소유 아이템 데이터가 변경될 때 증가하는 revision 스트림.
    /// UI(HotbarManager 등)는 이 스트림을 구독하여 변경이 일어날 때만 화면을 갱신합니다.
    /// </summary>
    public Observable<int> InventoryRevisionChanged => inventoryRevisionChanged;

    private void PublishDataChanged()
    {
        inventoryRevision++;
        inventoryRevisionChanged.OnNext(inventoryRevision);
    }
    #endregion

    #region Initialization & Lifecycle

    void IInitializable.Initialize()
    {
        GlobalEventManager.OnItemPickedUpObservable.Subscribe(AddItem).AddTo(_disposables);
        GlobalEventManager.OnNextDayObservable.Subscribe(_ => CalculateMoneyInSellingBox()).AddTo(_disposables);
    }

    void IDisposable.Dispose()
    {
        inventoryRevisionChanged.Dispose();

        _disposables.Dispose();
    }
    protected virtual void Initialize(ItemInstantData data)
    {
        _Data = data;
        PublishDataChanged();
    }


    public virtual void Load(SaveDatas saveDatas)
    {
        Initialize(saveDatas.GetItemData);
    }
    #endregion

    #region Data Operations (데이터 조작: 추가/삭제/이동/정렬)

    /// <summary>
    /// 인벤토리 원본 리스트에서 특정 구간만 잘라서 볼 수 있는 무할당(Zero-Allocation) View를 반환합니다.
    /// (예: segmentIndex 0 => [0..9], 1 => [10..19])
    /// </summary>
    public ItemInstantData.InventoryRangeView GetInventorySegment(int segmentIndex)
    {
        return _Data.GetInventorySegment(segmentIndex);
    }

    // 특정 박스나 인벤토리를 초기화(비우기)
    protected void ResetData(ContainerType type, int boxNum = 0)
    {
        if (type == ContainerType.INVENTORY)
        {
            List<GameItem> emptyList = new List<GameItem>(new GameItem[50]);
            _Data.SetItemList(type, emptyList);
        }
        else
        {
            var boxes = _Data.GetStorageBoxes;
            // TODO: StorageBox 초기화 로직 필요 시 추가
        }
        PublishDataChanged();
    }

    public virtual void Swap(ContainerType startPoint, ContainerType endPoint, int startIdx, int endIdx,
        int startBoxNum = 0, int endBoxNum = 0)
    {
        _Data.SwapItem(startPoint, endPoint, startIdx, endIdx, startBoxNum, endBoxNum);
        PublishDataChanged();
        Debug.Log($"{startPoint}[{startIdx}] <-> {endPoint}[{endIdx}] 아이템 스왑 (Box: {startBoxNum}/{endBoxNum})");
    }

    /// <summary>
    /// 두 아이템 스택을 합칩니다. 최대 스택 개수를 초과하지 않는 선에서만 이동됩니다.
    /// </summary>
    public virtual void EngraftItem(GameItem a, GameItem b)
    {
        if (!a.CanStackWith(b))
            return;

        int amountToMove = Mathf.Min(a.GetRemainStackSpace(), b.Count);

        a.Count += amountToMove;
        b.Count -= amountToMove;

        PublishDataChanged();
    }

    public void Sort() => Sort(currentType, _currentBoxIndex);

    public void Sort(ContainerType type, int boxNum = 0)
    {
        _Data.SortList(type, boxNum);

        var list = _Data.GetItemList(type, boxNum);
        if (list == null) return;

        // 같은 아이템끼리 인접하게 정렬되었으므로 스택을 합침
        for (int i = 0; i < list.Count - 1; i++)
        {
            EngraftItem(list[i], list[i + 1]);

            if (ItemInstantData.IsEmpty(list[i + 1]))
                list[i + 1] = null;
        }

        // 합친 후 발생한 빈 공간을 메꾸기 위해 다시 정렬
        _Data.SortList(type, boxNum);
        PublishDataChanged();
    }

    protected virtual void AddItem(GameItem item) => AddItem(currentType, item, _currentBoxIndex);

    public virtual void AddItem(ContainerType type, GameItem item, int boxNum = 0)
    {
        _Data.AddItem(type, item, boxNum);
        PublishDataChanged();
    }

    public bool RemoveItem(ContainerType type, int id, FlowerGrade grade, int count, int boxNum = 0)
    {
        if (count <= 0)
            return false;

        if (!HasItem(type, id, grade, count, boxNum))
            return false;

        var list = _Data.GetItemList(type, boxNum);
        if (list == null)
            return false;

        int remainingToRemove = count;

        for (int i = 0; i < list.Count; i++)
        {
            GameItem item = list[i];

            if (!IsSameItemForRemove(item, id, grade))
                continue;

            int toTake = Mathf.Min(remainingToRemove, item.Count);

            item.Count -= toTake;
            remainingToRemove -= toTake;

            if (item.Count <= 0)
                list[i] = null;

            if (remainingToRemove <= 0)
                break;
        }

        PublishDataChanged();
        return true;
    }

    public bool HasItem(ContainerType type, int id, FlowerGrade grade, int count, int boxNum = 0)
    {
        if (count <= 0)
            return true;

        var list = _Data.GetItemList(type, boxNum);
        if (list == null)
            return false;

        int totalCount = 0;

        for (int i = 0; i < list.Count; i++)
        {
            GameItem item = list[i];

            if (!IsSameItemForRemove(item, id, grade))
                continue;

            totalCount += item.Count;

            if (totalCount >= count)
                return true;
        }
        return false;
    }
    #endregion

    #region Helper & Specific Logics (유틸리티 및 특수 로직)
    // 해당 아이템에 등급 유무 판단 (Seed/Flower만 등급 관리)
    private static bool IsGradeManagedItem(GameItem item)
    {
        if (item == null)
            return false;

        return item.SubType == ItemSubType.Seed
            || item.SubType == ItemSubType.Flower;
    }

    // 아이템 제거 시, 단순 Id 비교로 제거 가능한지 판단하는 메서드
    private static bool IsSameItemForRemove(GameItem item, int id, FlowerGrade grade)
    {
        if (item == null)
            return false;

        // 장비는 삭제 불가
        if (item.MainType == ItemMainType.Equipment) return false;

        if (item.Id != id) return false;

        // Seed / Flower가 아니면 Id만 같으면 제거 가능
        if (!IsGradeManagedItem(item)) return true;

        // Seed / Flower는 FlowerItem이어야 하며, Grade까지 같아야 함
        if (item is FlowerItem flowerItem)  return flowerItem.Grade == grade;

        return false;
    }

    public void CalculateMoneyInSellingBox()
    {
        var sellingBox = _Data.GetItemList(ContainerType.SELLING).ToList<GameItem>();
        if (sellingBox.Count == 0 || sellingBox.Count(item => item.Id == 0) == 50) return;

        int totalMoney = sellingBox.Sum(item => GlobalItemDB.GetPrice(item.Id) * item.Count);
        _Data.SetItemList(ContainerType.SELLING, new List<GameItem>(new GameItem[50]));
        _Data.AddMoney(totalMoney);

        PublishDataChanged();
        Debug.Log($"하루가 지나 판매 완료. 총 수익: {totalMoney}골드");
    }
    #endregion
}
#endregion


#region ItemInstantData (아이템 상태 데이터 구조체)
/// <summary>
/// 실제 아이템 데이터들을 들고 있는 구조체. 
/// MemoryPack을 통한 빠른 직렬화/역직렬화를 지원합니다.
/// </summary>
[MemoryPackable]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct ItemInstantData
{
    #region Fields
    [MemoryPackInclude, SerializeField] private int money;
    [MemoryPackInclude, SerializeField] private int reputation;
    [MemoryPackInclude, SerializeField] private List<GameItem> invenList;
    [MemoryPackInclude, SerializeField] private List<StorageBox> storageBoxList;

    [MemoryPackIgnore, SerializeField] private List<GameItem> sellingBox;
    #endregion

    #region Getter  & Setter
    public int GetMoney => this.money;
    public int GetReputation => this.reputation;
    public void AddMoney(int money) => this.money += money;
    public List<StorageBox> GetStorageBoxes => storageBoxList;
    public IList<GameItem> GetItemList(ContainerType type, int boxNum = 0)
    {
        if (type == ContainerType.INVENTORY) return invenList;
        if (type == ContainerType.STORAGE) return storageBoxList[boxNum].BoxSlots;
        if (type == ContainerType.SELLING) return sellingBox;
        return null;
    }
    public void SetItemList(ContainerType type, List<GameItem> itemList, int boxNum = 0)
    {

        if (type == ContainerType.INVENTORY)
        {
            invenList = itemList;
            return;
        }

        if (type == ContainerType.SELLING)
        {
            sellingBox = itemList;
            return;
        }

        if (type == ContainerType.STORAGE)
        {
            if (boxNum < 0 || boxNum >= storageBoxList.Count)
            {
                Debug.LogError($"<b>[에러(SetItemList)]</b> StorageBox index out of range: {boxNum}");
                return;
            }

            StorageBox box = storageBoxList[boxNum];
            box.SetBoxSlots(itemList.ToArray());
            storageBoxList[boxNum] = box;
        }
    }
    #endregion

    #region Initialization & List Management


   
    // size 만큼 null로 채워진 GameItem 리스트를 생성하는 유틸리티 메서드입니다. (예: 인벤토리 초기화 시 사용)
    public static List<GameItem> CreateEmptyItemList(int size)
    {
        return Enumerable.Repeat<GameItem>(null, size).ToList();
    }

    // 인벤토리 칸수 변경, 최초 게임 실행 시 리스트가 아예 없을 경우를 대비하여, 기존 리스트를 유지하되 부족한 칸은 null로 채우고, 초과하는 칸은 제거하는 메서드입니다.
    private static List<GameItem> EnsureFixedSlotList(List<GameItem> source, int size)
    {
        source ??= CreateEmptyItemList(size);

        while (source.Count < size)
            source.Add(null);

        if (source.Count > size)
            source.RemoveRange(size, source.Count - size);

        return source;
    }
    // 인벤토리, 판매 상자, 창고 리스트가 null이거나 칸 수가 맞지 않을 때, EnsureFixedSlotList를 통해 초기화하거나 조정하는 최종 점검 메서드입니다.
    public void EnsureSlotLists()
    {
        invenList = EnsureFixedSlotList(invenList, INVENTORY_SLOT_COUNT);
        sellingBox ??= CreateEmptyItemList(INVENTORY_SLOT_COUNT);
        storageBoxList ??= new List<StorageBox>();
    }
    #endregion

    #region Item Internal Operations (추가, 이동, 정렬)
    public void AddItem(ContainerType type, GameItem item, int boxNum = 0)
    {
        if (IsEmpty(item)) return;

        IList<GameItem> targetList = GetItemList(type, boxNum);
        if (targetList == null) return;

        // 1. 같은 아이템의 기존 스택에 먼저 합침.
        for (int i = 0; i < targetList.Count; i++)
        {
            GameItem curItem = targetList[i];
            if (curItem == null || !curItem.CanStackWith(item)) continue;

            int moveAmount = Mathf.Min(curItem.GetRemainStackSpace(), item.Count);
            curItem.Count += moveAmount;
            item.Count -= moveAmount;

            if (item.Count <= 0) return;
        }

        // 2. 빈 슬롯에 남은 아이템을 넣음.
        for (int i = 0; i < targetList.Count; i++)
        {
            if (!IsEmpty(targetList[i])) continue;

            targetList[i] = item;
            return;
        }

        Debug.Log($"[{type} Box:{boxNum}] 슬롯 가득 참");
    }

    public void SwapItem(ContainerType startPoint, ContainerType endPoint, int startIdx, int endIdx, int startBoxNum = 0, int endBoxNum = 0)
    {
        IList<GameItem> target1 = GetItemList(startPoint, startBoxNum);
        IList<GameItem> target2 = GetItemList(endPoint, endBoxNum);

        if (target1 == null || target2 == null ||
            startIdx < 0 || endIdx < 0 ||
            startIdx >= target1.Count || endIdx >= target2.Count)
        {
            Debug.LogError("<b>[에러(Swap)]</b> 인덱스가 범위를 초과했거나 대상 리스트가 없음");
            return;
        }

        (target1[startIdx], target2[endIdx]) = (target2[endIdx], target1[startIdx]);
    }

    public void SortList(ContainerType type, int boxNum = 0)
    {
        IList<GameItem> targetList = GetItemList(type, boxNum);
        if (targetList == null) return;

        // 힙 할당 없이 스택 메모리에 구조체 비교기 생성 (GC = 0)
        var comparer = new GameItemComparer();

        // IList의 실제 타입에 따라 내부적으로 최적화된 C# 고속 정렬을 수행해요.
        if (targetList is List<GameItem> list)
        {
            // Inventory, SellingBox는 List<GameItem> 이므로 이곳을 탑니다.
            list.Sort(comparer);
        }
        else if (targetList is GameItem[] array)
        {
            // StorageBox의 BoxSlots는 GameItem[] 배열이므로 이곳을 탑니다.
            Array.Sort(array, comparer);
        }
    }

    public static bool IsEmpty(GameItem item)
    {
        return item == null || item.Id <= 0 || item.Count <= 0;
    }

    #endregion

    #region Zero-Allocation Inventory View (가비지 생성 없는 인벤토리 뷰어)
    /* * [알림] 이 영역은 UI 최적화를 위해 매우 중요합니다! (Zero-Allocation Pattern)
     * InventoryRangeView와 내장 Enumerator가 'struct'로 선언된 이유는, 
     * GetInventorySegment() 호출이나 foreach 순회 시 힙 메모리 할당(GC 발생)을 원천 차단하기 위함입니다.
     * 클래스로 변경하거나 yield return을 사용하지 않도록 주의하세요.
     */


    public InventoryRangeView GetInventorySegment(int segmentIndex)
    {
        EnsureSlotLists();

        int startIndex = segmentIndex * INVENTORY_COLUMN_SIZE;
        return GetInventoryRangeView(startIndex, INVENTORY_COLUMN_SIZE);
    }

    public InventoryRangeView GetInventoryRangeView(int startIndex, int count)
    {
        EnsureSlotLists();

        if (!IsValidInventoryRange(startIndex, count))
        {
            Debug.LogError($"<b>[에러(GetInventoryRangeView)]</b> Inventory range out of range. Start: {startIndex}, Count: {count}");
            // 빈 리스트 대신 count가 0인 뷰를 반환하여 에러 방지
            return new InventoryRangeView(invenList, startIndex, 0);
        }

        return new InventoryRangeView(invenList, startIndex, count);
    }
    public static bool IsValidInventoryRange(int startIndex, int count)
    {
        if (startIndex < 0 || count < 0)
            return false;

        if (startIndex + count > INVENTORY_SLOT_COUNT)
            return false;

        return true;
    }

    public readonly struct InventoryRangeView : IReadOnlyList<GameItem>
    {
        private readonly List<GameItem> source;
        private readonly int startIndex;
        private readonly int count;

        public InventoryRangeView(List<GameItem> source, int startIndex, int count)
        {
            this.source = source;
            this.startIndex = startIndex;
            this.count = count;
        }

        public int Count => count;

        public GameItem this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException($"Inventory range view index out of range: {index}");
                return source[startIndex + index];
            }
        }

        // foreach 순회 시 박싱(Boxing)을 피하기 위해 구조체 Enumerator를 직접 반환합니다.
        public Enumerator GetEnumerator() => new Enumerator(source, startIndex, count);

        IEnumerator<GameItem> IEnumerable<GameItem>.GetEnumerator() => GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        // yield return을 대체하는 가비지 없는 커스텀 구조체 Enumerator
        public struct Enumerator : IEnumerator<GameItem>
        {
            private readonly List<GameItem> _source;
            private readonly int _endIndex;
            private int _currentIndex;

            public Enumerator(List<GameItem> source, int startIndex, int count)
            {
                _source = source;
                _currentIndex = startIndex - 1;
                _endIndex = startIndex + count;
            }

            public GameItem Current => _source[_currentIndex];
            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _endIndex;
            }

            public void Reset() { }
            public void Dispose() { }
        }
    }
    #endregion
}
#endregion


#region StorageBox (창고 개별 박스 데이터 구조체)
[MemoryPackable]
[StructLayout(LayoutKind.Sequential)]
public partial struct StorageBox
{
    [MemoryPackInclude] private string boxName;
    [MemoryPackInclude] private GameItem[] boxSlots;

    public GameItem[] BoxSlots => boxSlots;
    public string BoxName => boxName;

    public void SetBoxName(string boxName) => this.boxName = boxName;
    public void SetBoxSlots(GameItem[] gameItems) => this.boxSlots = gameItems;
}
#endregion

#region GameItemComparer (아이템 정렬을 위한 비교자). Zero -Allocation을 위해 struct로 선언되어 힙 할당 없이 스택 메모리에 생성됩니다.
public readonly struct GameItemComparer : IComparer<GameItem>
{
    public int Compare(GameItem x, GameItem y)
    {
        bool xEmpty = ItemInstantData.IsEmpty(x);
        bool yEmpty = ItemInstantData.IsEmpty(y);

        // 1순위: 빈칸 밀어내기 (!IsEmpty)
        if (xEmpty && yEmpty) return 0;       // 둘 다 비었으면 순서 유지
        if (xEmpty && !yEmpty) return 1;      // x가 비었고 y가 있으면 y가 앞으로 (1)
        if (!xEmpty && yEmpty) return -1;     // x가 있고 y가 비었으면 x가 앞으로 (-1)

        // 2순위: Id 오름차순 (ThenBy)
        int idCompare = x.Id.CompareTo(y.Id);
        if (idCompare != 0) return idCompare;

        // 3순위: Count 내림차순 (ThenByDescending)
        // 내림차순이므로 y.Count와 x.Count의 비교 순서를 뒤집었어요.
        return y.Count.CompareTo(x.Count);
    }
}
#endregion