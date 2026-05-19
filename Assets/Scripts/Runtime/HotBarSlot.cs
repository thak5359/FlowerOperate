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
    public TextMeshProUGUI Count; // 아이템의 스택 수량을 표시. MaxStack이 1이 아닌 아이템만 표시.
    public Toggle toggle;
    public Image slotFrame;
    public Image bg_img;


}
