using Cysharp.Threading.Tasks;
using Fungus;
using MemoryPack;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor.UIElements;
using UnityEngine;


[MemoryPackable]
[Serializable]
//[MemoryPackUnion(0, typeof(GameItem))] 내부적으로 어떤아이템인지 알수 있게 분류할 방법. 저장 로직 확정후에 같이 결정!
//[MemoryPackUnion(1, typeof(FlowerItem))]
//[MemoryPackUnion(2, typeof(GearItem))]
public partial class GameItem : IGameResource
{
    [field: SerializeField]
    public int Id { get; protected set; }

    [field: SerializeField]
    public int Count { get; set; }
    [MemoryPackIgnore]
    public int RefundPrice { get; protected set; } // 되팔기 기준 금액 ( 상점가의 50%)

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

    public GameItem(int id, int count = 1)
    {
        Id = id;
        Count = count;
        OnLoadAsync();
    }



    public virtual void OnLoadAsync(IPropData propData = default)
    {
        if (!GlobalItemDB.IsInitialized)
        {
            Debug.LogError("[GameItem] GlobalItemDB가 초기화되지 않았습니다.");
            return;
        }

        if (!GlobalItemDB.HasBase(Id))
        {
            Debug.LogError($"[GameItem] ItemBaseDB 조회 실패. Id: {Id}");
            return;
        }

        ref ItemBaseBlobData baseData = ref GlobalItemDB.GetBaseRef(Id);


        MainType = baseData.MainType;
        SubType = baseData.SubType;
        StackLimit = baseData.StackLimit;
        ItemName = baseData.ItemName;
        Description = baseData.Description;
        SpriteAddress = baseData.SpriteAddress;

        if (!SpriteAddress.IsEmpty)
        {
            DisplaySprite = AddressableManager.LoadAssetAsync<Sprite>(SpriteAddress).GetAwaiter().GetResult();
        }
    }

    public bool CanStackWith(GameItem other)
    {
        if (other == null)
            return false;

        if (Id != other.Id)
            return false;

        if (Count >= StackLimit)
            return false;

        bool thisGradeManaged =
            SubType == ItemSubType.Seed ||
            SubType == ItemSubType.Flower;

        bool otherGradeManaged =
            other.SubType == ItemSubType.Seed ||
            other.SubType == ItemSubType.Flower;

        if (thisGradeManaged != otherGradeManaged)
            return false;

        if (!thisGradeManaged)
            return true;

        if (this is FlowerItem thisFlower && other is FlowerItem otherFlower)
            return thisFlower.Grade == otherFlower.Grade;

        return false;
    }

    public int GetRemainStackSpace()
    {
        return Mathf.Max(0, StackLimit - Count);
    }

    public void AddAmount(int input_amount)
    {
        if (this.Count + input_amount > Constant.MAX_COUNT_INVENTORY)
        {
            this.Count = Constant.MAX_COUNT_INVENTORY;
            return;
        }
        this.Count += input_amount;
    }
    
    /// 요청사항만큼 amount를 빼고 남은 amount를 반환합니다.
    public void SubCount(ref int amount)
    {
        if (amount <= 0) return;

        if (this.Count - amount <= 0)
        {
            this.Id = 0;
            amount -= this.Count;
            this.Count = 0;
        }
        else
        {
            this.Count -= amount;
            amount = 0;
        }

    }

    public bool CheckEmpty()
    {
        if (this.Count <= 0)
            return true;
        return false;
    }

    public bool CheckFull()
    {
        // 스택이 Full인지 Zero인지 판단하는 함수
        if (Count == Constant.MAX_COUNT_INVENTORY)
            return true;
        return false;
    }
}