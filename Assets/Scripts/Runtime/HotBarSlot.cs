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
    private void OnDisable()
    {
        item?.Cleanup();
    }
    public async Task ChangeItem(SlotItem newItem)
    {
        item?.Cleanup();

        item = newItem;

        if (item != null)
        {
            ItemIcon.sprite = await item.RefreshSprite();
            tmp.text = item.Amount.ToString();
        }
        else
        {
            ItemIcon.sprite = null;
            tmp.text = "0";
        }
    }

}
