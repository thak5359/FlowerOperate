using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public SlotItem item;

    public async Task<SlotItem> changeItem(SlotItem draggedItem)
    {
        if (draggedItem != null)
        {
            SlotItem returnvalue = item;
            item = draggedItem;


            tmp.text = item.amount.ToString();
            ItemIcon.sprite = await item.GetSprite(); 
              

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
