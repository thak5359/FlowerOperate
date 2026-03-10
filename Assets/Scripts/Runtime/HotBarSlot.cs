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
        // 1. 기존 아이템 리소스 정리
        item?.Cleanup();

        // 2. 새 아이템 할당
        item = newItem;

        // 3. UI 업데이트
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

    private void Start()
    {
        if(slotFrame.IsActive() == true)
        {
            slotFrame.enabled = false;
        }
    }
}
