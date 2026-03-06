using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    [SerializeField]
    [Header("핫키 슬롯을 등록해주세요")]
    List<HotBarSlot> slots;
    PlayerController player;

    [SerializeField]
    private int pointingSlot = 0;

    void Awake()
    {
        if(slots == null)
        {
            Debug.Log("Hotbar slots is NULL");
        }
    }
    private void OnEnable()
    {
        slots[pointingSlot].enabled = true;
    }


    public void OnScrollMouse(InputAction.CallbackContext value)
    {
        Vector2 scrollDelta = value.ReadValue<Vector2>();

        if (scrollDelta.y > 0)
        {

            if (pointingSlot <= slots.Count)
            {
                pointSlot(++pointingSlot);
            }
            Debug.Log("ScrollUP is dected");
        }
        else
        {
            if (pointingSlot > 1)
            {
                pointSlot(--pointingSlot);
            }
            Debug.Log("ScrollDown is dected");
        }

    }

    public void pointSlot(int i)
    {
        //이전에 쓰던 슬롯 '사용중'프레임 비활성화
        slots[pointingSlot].slotFrame.enabled = false;

        slots[i].toggle.isOn = true;
        UnityEngine.Debug.Log($"{i} is pressed");
        slots[i].slotFrame.enabled = true;
        if (slots[i].item != null)
        player.item = slots[i].item;

        pointingSlot = i;
    }



    //IA 테스팅 용도
    //void OnClickMouse(InputAction.CallbackContext value)
    //{

    //    if (value.started == true)
    //    {
    //        Debug.Log("Mouse click is detected");
    //    }
    //}

    
    
}
