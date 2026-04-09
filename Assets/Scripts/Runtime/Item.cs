using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


public struct   UseParam
{
    public readonly Vector2 heading;
    public readonly sbyte power;
    public readonly float elapsedTime;

    public UseParam(Vector2 input_heading,  sbyte input_power, float input_elapsed)
    {
        heading = input_heading;
        power = input_power;
        elapsedTime = input_elapsed;
    }
}   


[System.Serializable] // �ν����ͳ� ����� Ȯ���� ���� ����ȭ �����ϰ� ����
public class Item
{
    public ushort itemId;
    protected short amount = 1;
    protected Sprite cachedSprite;

    public Item(ushort id, short count)
    {
        this.itemId = id;
        this.amount = count;
    }

    #region Get
    public short Amount => amount;
    virtual public FixedString64Bytes GetName()
    {
        if (itemId != 0)
            return ItemManager.Instance.GetItemName(itemId);

        else return null;
    }
    virtual public FixedString128Bytes GetDescription()
    {
        if (itemId != 0)
            return ItemManager.Instance.GetItemDescription(itemId);
        else return null;
    }
    public virtual async Task<Sprite> GetSprite()
    {
            if (cachedSprite == null)
            {
                    cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ItemManager.Instance.GetItemAddress(itemId).ToString());
            }
            return cachedSprite;
    }
    #endregion

    //������ �ε�
    public virtual async void LoadData(ushort input_itemId, short input_amount)
    {
        itemId = input_itemId;
        amount = input_amount;

        if (itemId != 0)
        {
            await RefreshSprite();
        }
    }

    // ���ҽ� ����

    public virtual async Task<Sprite> RefreshSprite()
    {
            if (cachedSprite != null) AddressableManager.ReleaseAsset(cachedSprite);
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ItemManager.Instance.GetItemAddress(itemId).ToString());
            return cachedSprite;
    }

    virtual public void OnUse(UseParam param)
    {

    }

    virtual public int? GetLevel()
    {
        return null;
    }

    Item()
    {
        Cleanup();
    }

    public virtual void Cleanup()
    {
        if (cachedSprite != null)
        {
            AddressableManager.ReleaseAsset(cachedSprite);
            cachedSprite = null;
            Debug.Log($"Item {itemId} ���ҽ� 1ȸ ���� �Ϸ�");
        }
    }

}