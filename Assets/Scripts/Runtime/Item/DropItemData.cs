using Cysharp.Threading.Tasks;
using MemoryPack;
using System.Runtime.InteropServices;
using UnityEngine;

public class DropItemData : MonoBehaviour
{
    [SerializeReference]
    private GameItem data;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    //[SerializeField]
    //private int waitMilliSeconds = 1000;

    private bool isPickingUp;

    public GameItem GetData => data;
    public int GetItemId => data != null ? data.Id : 0;
    public int GetCount => data != null ? data.Count : 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public FlowerGrade GetGrade
    {
        get
        {
            if (data is FlowerItem flowerItem)
                return flowerItem.Grade;

            return FlowerGrade.Unknown;
        }
    }

    public void SetData(GameItem data)
    {
        this.data = data;
        spriteRenderer.sprite = data.DisplaySprite;
    }

    public void AddAmount(int amount)
    {
        if (data == null)
            return;

        data.Count += amount;

        if (data.Count < 0)
            data.Count = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"트리거됨 : {other.tag}");
        if (!other.CompareTag("Player"))
            return;

        if (isPickingUp)
            return;

        DoOnTrigger();
    }

    private void DoOnTrigger()
    {
        isPickingUp = true;

        if (data == null || data.Count <= 0)
        {
            Destroy(gameObject);
            return;
        }

        GlobalEventManager.InvokeItemPickedUp(data);
        Destroy(gameObject);
    }
}


[MemoryPackable]
[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct ItemObjectData /// ItemInstantData
{
    [MemoryPackInclude, SerializeField] ushort itemID;
    [MemoryPackInclude, SerializeField] short Duration;
    [MemoryPackInclude, SerializeField] short amount;
    [MemoryPackInclude, SerializeField] byte grade;

    //게터
    public ushort GetItemID => itemID;
    public short GetAmount => amount;
    public short GetDuration => Duration;
    public byte GetGrade => grade;

    //세터
    public void SetItemID(ushort itemID) => this.itemID = itemID;
    public void SetAmount(short amount) => this.amount = amount;
    public void SetDuration(short Dur) => this.Duration = Dur;
    public void SetGrade(byte grade) => this.grade = grade;

    public void AddAmount(short amount)
    {
        if(this.amount + amount > Constant.MAX_COUNT_INVENTORY)
        {
            this.amount = (short)Constant.MAX_COUNT_INVENTORY;
            return;
        }
        this.amount += amount;
    }
    public bool CheckFull()
    {
        // 스택이 Full인지 Zero인지 판단하는 함수
        if(amount == Constant.MAX_COUNT_INVENTORY)
            return true;
        return false;
    }

    public bool CheckEmpty()
    {
        if(amount <= 0) 
            return true;
        return false;
    }


    public ItemObjectData(ushort itemID = 0, short amount = 0, short duration = 0, byte grade = 0)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}

