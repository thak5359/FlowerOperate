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
        UpdateColor(toggle.isOn);
        toggle.onValueChanged.AddListener(UpdateColor);
    }

    private void UpdateColor(bool isOn)
    {
        bg_img.color = isOn ? Color.yellow : Color.black;
    }
    


}
