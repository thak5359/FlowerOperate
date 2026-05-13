using Cysharp.Threading.Tasks;
using MemoryPack;
using Unity.Collections;
using UnityEngine;


[MemoryPackable]
//[MemoryPackUnion(0, typeof(GameItem))] 내부적으로 어떤아이템인지 알수 있게 분류할 방법. 저장 로직 확정후에 같이 결정!
//[MemoryPackUnion(1, typeof(FlowerItem))]
//[MemoryPackUnion(2, typeof(GearItem))]
public partial class GameItem : IGameResource
{
    [field: SerializeField]
    public int Id { get; protected set; }
    
    public int Count { get; set; }
    [MemoryPackIgnore]
    public int RefundPrice { get; protected set;  } // 되팔기 기준 금액 ( 상점가의 50%)

    [MemoryPackIgnore]
    [field: SerializeField]
    public Sprite DisplaySprite { get; protected set; }

    [MemoryPackIgnore] public ItemMainType MainType { get; protected set; }
    [MemoryPackIgnore] public ItemSubType SubType { get; protected set; }
    [MemoryPackIgnore] public int StackLimit { get; protected set; }

    [MemoryPackIgnore] public FixedString64Bytes ItemName { get; protected set; }
    [MemoryPackIgnore] public FixedString128Bytes Description { get; protected set; }
    [MemoryPackIgnore] public FixedString128Bytes SpriteAddress { get; protected set; }

    [MemoryPackConstructor]
    protected GameItem()
    {
    }

    public GameItem(int id, int count)
    {
        Id = id;
        Count = count;
    }
    public virtual async UniTask OnLoadAsync()
    {
        if (!GlobalItemDB.IsInitialized)
        {
            Debug.LogError("[GameItem] GlobalItemDB가 초기화되지 않았습니다.");
            return;
        }

        if (!GlobalItemDB.TryGetBase(Id, out ItemBaseBlobData baseData))
        {
            Debug.LogError($"[GameItem] ItemBaseDB 조회 실패. Id: {Id}");
            return;
        }

        MainType = baseData.MainType;
        SubType = baseData.SubType;
        StackLimit = baseData.StackLimit;
        ItemName = baseData.ItemName;
        Description = baseData.Description;
        SpriteAddress = baseData.SpriteAddress;

        if (!SpriteAddress.IsEmpty)
        {
            DisplaySprite = await AddressableManager.LoadAssetAsync<Sprite>(
                SpriteAddress
            );
        }
    }

    public bool CanStackWith(GameItem other)
    {
        return other != null
            && Id == other.Id
            && Count < StackLimit;
    }

    public int GetRemainStackSpace()
    {
        return Mathf.Max(0, StackLimit - Count);
    }
}