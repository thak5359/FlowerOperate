using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] protected ItemData itemData;
    public int amount = 1;
    [SerializeField] protected int level = 0;

    private Sprite cachedSprite;
    virtual public void loadData(ItemData input_itemData, int input_amount, int input_level)
    {
        itemData = input_itemData;
        amount = input_amount;
        level = input_level;
    }

    virtual public string GetName()
    {
        if (itemData != null)
            return itemData.GetName(level);

        else return null;
    }

    virtual public string GetDescription()
    {
        if (itemData != null)
            return itemData.GetDescription(level);
        else return null;
    }
    public virtual async Task<Sprite> GetSprite()
    {
        if (itemData != null)
        {
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(itemData.GetSpriteAddress(level));
            if (cachedSprite != null)
            {
                return cachedSprite;
            }
        }

        return null;
    }

    virtual public void OnUse(Vector2 heading, Vector3 pos)
    {

    }

    virtual protected void Use()
    {

    }
    virtual public void LevelUp()
    {
        level++;
    }

    virtual public int GetLevel()
    {
        return level;
    }
}




