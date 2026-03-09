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
    Vector2 heading;
    Vector3 pos;
    int efficiency;
    

}

public class SlotItem : MonoBehaviour
{
    [SerializeField] protected ItemIdData itemData;
    public int amount = 1;

    public Sprite cachedSprite; //현재 선택되어있는 스프라이트
    private int maxCharging; // 최대 차징 가능 횟수


    virtual public void loadData(ItemIdData input_itemData, int input_amount)
    {
        itemData = input_itemData;
        amount = input_amount;
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (itemData == null)
        {
            Debug.LogAssertion("itemData is null");
        }
#endif
    }

    private async void Start()
    {
        await GetSprite();
    }

    private void OnDisable()
    {
        AddressableManager.ReleaseAsset(itemData.SpriteAddress);
    }

    virtual public string GetName()
    {
        if (itemData != null)
            return itemData.ItemName;

        else return null;
    }
    virtual public string GetDescription()
    {
        if (itemData != null)
            return itemData.Description;
        else return null;
    }
    public virtual async Task<Sprite> GetSprite()
    {
        if (itemData != null)
        {
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(itemData.SpriteAddress);
            if (cachedSprite == null)
            {
                Debug.LogAssertion($"Sprite on {itemData} is missing");
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