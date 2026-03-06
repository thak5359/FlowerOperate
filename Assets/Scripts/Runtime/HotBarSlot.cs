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
    public Image slotFrame;
    public Image bg_img;
    public Item item;

    public Item changeItem(Item draggedItem)
    {
        if (draggedItem != null)
        {
            Item returnvalue = item;
            item = draggedItem;


            tmp.text = item.amount.ToString();
            ItemIcon.sprite = item.GetSprite();


            return returnvalue;
        }
        else
            item = null;
        return null;
    }

    private void Start()
    {
        if(slotFrame.IsActive() == true)
        {
            slotFrame.enabled = false;
        }
    }
}
