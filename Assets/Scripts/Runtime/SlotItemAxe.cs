using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;



public class UsableSlotItem : SlotItem
{

    [SerializeField] protected int level = 0;


    override public int? GetLevel()
    {
        return level;
    }
    protected async void LevelUp()
    {
        if (level < 7)
        {
            AddressableManager.ReleaseAsset(itemData.SpriteAddress);
            level++;
            cachedSprite = await AddressableManager.LoadAssetAsync<Sprite>(itemData.SpriteAddress);
        }
    }

}


public class Fertilizer : UsableSlotItem
{
    override public void OnUse(UseParam param)
    {
        switch (level)
        {
            case 0: // 하급 품질 비료
                {
                    break;
                }

            case 1: // 중급 품질 비료
                {

                    break;
                }
            case 2: // 고급 품질 비료
                {
                    break;
                }
            case 3: // 최고급 품질 비료
                {
                    break;
                }
            case 4: // 전설의 품질 비료
                {
                    break;
                }
            case 5: // 하급 풍작 비료
                {
                    break;
                }
            case 6: // 중급 풍작 비료
                {
                    break;
                }
            case 7: // 고급 풍작 비료
                {
                    break;
                }
            case 8: //최고급 풍작 비료
                {
                    break;
                }
            case 9: // 전설의 풍작 비료
                {
                    break;
                }
            case 10: // 하급 종합 비료
                {
                    break;
                }
            case 11: // 중급 종합 비료
                {
                    break;
                }
            case 12:// 고급 종합 비료
                {
                    break;
                }
            case 13: // 최고급 종합 비료
                {
                    break;
                }
            case 14:  // 전설의 종합 비료
                {
                    break;
                }
            default:
                {

                    break;
                }
        }
        Use();
    }
    protected void Use()
    {



    }
}


public class Sickle : UsableSlotItem
{
    override public void OnUse(UseParam param)
    {
        switch (level)
        {
            case 0: // 낡은 낫
                {
                    break;
                }
            case 1: // 동 낫
                {

                    break;
                }
            case 2: // 철 낫
                {
                    break;
                }
            case 3: // 은 낫
                {
                    break;
                }
            case 4: // 금 낫
                {
                    break;
                }
            case 5: // 플래티넘 낫
                {

                    break;
                }
            case 6: // 아다만티움 낫
                {

                    break;
                }
            case 7: // 오리할콘 낫
                {

                    break;
                }
            default:
                {
                    break;
                }
        }
        Use();
    }
    protected void Use()
    {

    }
}

public class SlotItemAxe : UsableSlotItem
{
     public void OnUse(Vector2 heading, Vector3 pos, float elapsed)
    {
        switch (level)
        {
            case 0: // 낡은 도끼
                {
                    break;
                }
            case 1: // 동 도끼
                {
                    break;
                }
            case 2: // 철 도끼
                {
                    break;
                }
            case 3: // 은 도끼
                {
                    break;
                }
            case 4: // 금도끼
                {
                    break;
                }
            case 5: // 플래티넘 도끼
                {
                    break;
                }
            case 6: // 아다만티움 도끼
                {
                    break;
                }
            case 7: // 오리할콘 도끼
                {
                    break;
                }
            default:
                {

                    break;
                }
        }
        Use();
    }
    protected void Use()
    {

    }
}
