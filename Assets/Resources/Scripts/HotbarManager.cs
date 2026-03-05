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

    [SerializeField]
    private int pointingSlot = 0;

    void Awake()
    {
        if(slots == null)
        {
            Debug.Log("Hotbar slots is NULL");
        }
    }

    
    void OnScrollMouse(InputValue value)
    {
        Debug.Log("Scroll is dected");
        Vector2 scrollDelta = value.Get<Vector2>();

        if (scrollDelta.y > 0)
        { pointSlot(++pointingSlot); }
        else
        {pointSlot(--pointingSlot); }
    }

    void pointSlot(int i)
    {
        slots[i].toggle.isOn = true;
    }

    void OnClickMouse(InputValue value)
    {

        if (value.isPressed == true)
        {
            Debug.Log("Mouse click is detected");
        }
    }

    
}
