using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.UI;

public class HotBarSlot : MonoBehaviour
{
    public Image ItemIcon;
    public TextMeshProUGUI tmp;
    public Toggle toggle;


    public Item item;

    public Item changeItem(Item draggedItem)
    {
        if (draggedItem != null)
        {
            Item returnvalue = item;
            item = draggedItem;


            // 표기 변환
            ItemIcon = item.image;
            tmp.text = item.amount.ToString();




            return returnvalue;
        }
        else
            item = null;
        return null;
    }

}
