using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData")]
public class ItemData : ScriptableObject
{
    public int itemID;
    [SerializeField] protected List<string> itemName; // 아이템 이름 목록
    [SerializeField] protected List<string> description; // 아이템 설명 목록
    [SerializeField] protected List<Sprite> imageList; // 아이템 이미지 목록

    protected bool isStackable; // 아이템을 겹쳐서 보관할 수 있음
    protected bool isUsable; // 핫 키에 있을 떄 사용할 수 있음
    protected bool canDump; // 아이템을 버릴 수 있음
    protected bool canSold; // 아이템을 상점에 팔 수 있음
    protected int maxLevel; // 아이템 최대 레벨

    public string GetName(int lv)
    {
        int index = Mathf.Clamp(lv, 0, itemName.Count - 1);
        return itemName[index];
    }
    public string GetDescription(int lv)
    {
        int index = Mathf.Clamp(lv, 0, description.Count - 1);
        return description[index];
    }
    public Sprite GetSprite(int lv)
    {
        int index = Mathf.Clamp(lv, 0, imageList.Count - 1);
        return imageList[index];
    }

}


