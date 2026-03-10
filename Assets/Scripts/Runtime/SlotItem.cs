using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;


public struct UseParam
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
public class SlotItem
{
    public int? itemId = null;
    public int amount = 1;

    public Sprite cachedSprite;
    protected ItemManager itemManager;


    public SlotItem(int? id, int count)
    {
        itemManager = ItemManager.Instance;
        this.itemId = id;
        this.amount = count;
    }

    // 2.로드 ( 수동 호출)
    public virtual async void LoadData(int input_itemId, int input_amount)
    {
        itemId = input_itemId;
        amount = input_amount;

        if (itemId != -1 && itemId != null)
        {
            await RefreshSprite();
        }
    }

    // 3. 리소스 해제 ( 수동 호출)
    public virtual void Cleanup()
    {
        if (cachedSprite != null)
        {
            AddressableManager.ReleaseAsset(cachedSprite);
            cachedSprite = null;
            Debug.Log($"Item {itemId} 리소스 해제 완료");
        }
    }

    public virtual async Task<Sprite> RefreshSprite()
    {
        if (itemId != null && itemId != -1)
        {
            if (cachedSprite != null) AddressableManager.ReleaseAsset(cachedSprite);

            string addr = itemManager.GetItemAddress((int)itemId);
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(addr);
            return cachedSprite;
        }
        return null;
    }



    virtual public string GetName()
    {
        if (itemId != -1)
            return itemManager.GetItemName((int)itemId);

        else return null;
    }
    virtual public string GetDescription()
    {
        if (itemId != -1)
            return itemManager.GetItemDescription((int)itemId);
        else return null;
    }
    public virtual async Task<Sprite> GetSprite()
    {
        if (itemId != -1)
        {
            // 중복 로드를 방지하기 위해 기존 스프라이트가 있다면 해제하고 시작하는 것이 안전합니다.
            if (cachedSprite != null) AddressableManager.ReleaseAsset(cachedSprite);

            string addr = itemManager.GetItemAddress((int)itemId);
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(addr);

            if (cachedSprite == null)
            {
                Debug.LogAssertion($"Sprite on {itemId} is missing at address: {addr}");
                return null;
            }
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

    

}