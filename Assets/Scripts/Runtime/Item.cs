using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


public struct   UseParam
{
    public readonly Vector2 heading;
    public readonly Vector3 pos;
    public readonly int efficiency;
    public readonly float elapsedTime;

    public UseParam(Vector2 input_heading, Vector3 input_pos, int input_efficiency, float input_elapsed)
    {
        heading = input_heading;
        pos = input_pos;
        efficiency = input_efficiency;
        elapsedTime = input_elapsed;
    }
}


[System.Serializable] // 인스펙터나 디버깅 확인을 위해 직렬화 가능하게 설정
public class Item
{
    public int? itemId = null;
    protected int amount = 1;
    protected Sprite cachedSprite;

    public Item(int? id, int count)
    {
        this.itemId = id;
        this.amount = count;
    }

    #region Get
    public int Amount => amount;
    virtual public string GetName()
    {
        if (itemId != -1)
            return ItemManager.Instance.GetItemName((int)itemId);

        else return null;
    }
    virtual public string GetDescription()
    {
        if (itemId != -1)
            return ItemManager.Instance.GetItemDescription((int)itemId);
        else return null;
    }
    public virtual async Task<Sprite> GetSprite()
    {
        if (itemId != null)
        {
            if (cachedSprite != null)
            {
                if (itemId is int)
                    cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ItemManager.Instance.GetItemAddress((int)itemId, 0));
            }
            return cachedSprite;
        }
        return null;
    }
    #endregion

    //데이터 로드
    public virtual async void LoadData(int input_itemId, int input_amount)
    {
        itemId = input_itemId;
        amount = input_amount;

        if (itemId != -1 && itemId != null)
        {
            await RefreshSprite();
        }
    }

    // 리소스 해제

    public virtual async Task<Sprite> RefreshSprite()
    {
        if (itemId != null && itemId is int)
        {
            if (cachedSprite != null) AddressableManager.ReleaseAsset(cachedSprite);
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ItemManager.Instance.GetItemAddress((int)itemId));
            return cachedSprite;
        }
        return null;
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
            Debug.Log($"Item {itemId} 리소스 1회 해제 완료");
        }
    }

}