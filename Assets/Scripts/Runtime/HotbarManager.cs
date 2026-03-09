using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    [SerializeField]
    [Header("핫키 슬롯을 등록해주세요")]
    List<HotBarSlot> slots;
    PlayerController player;

    [SerializeField] private int pointingSlot = -1;

    private float scrollCooldown = 0.15f;
    private float lastScrollTime;

    void Awake()
    {
        if(slots == null)
        {
            Debug.Log("Hotbar slots is NULL");
        }
    }
    private void Start()
    {
        slots[0].toggle.isOn = true;
        slots[0].slotFrame.enabled = true;

        if (slots[0].item != null && player != null)
        {
            player.item = slots[0].item;
        }
    }


    public void OnScrollMouse(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        //if (Time.time < lastScrollTime + scrollCooldown) return;

        Vector2 scrollDelta = value.ReadValue<Vector2>();

        //  스크롤 값(노이즈) 무시 (보통 휠은 120 단위지만 터치패드는 낮을 수 있음)
        if (Mathf.Abs(scrollDelta.y) < 0.1f) return;
        //lastScrollTime = Time.time;

        int newIndex = pointingSlot + (scrollDelta.y > 0 ? -1 : 1);
        //newIndex = (newIndex + slots.Count) % slots.Count;
        newIndex = Mathf.Clamp(newIndex, 0, slots.Count - 1); // 비 순환 방식
        Debug.Log($"newIndex = {newIndex}");
        //lastScrollTime = Time.time;
        pointSlot(newIndex);

        


    }

    public void pointSlot(int i)
    {

        if (i == pointingSlot && slots[i].slotFrame.enabled|| i < 0 || i >= slots.Count) return;

        //쿨타임 체크( 마지막 스크롤로부터 0.15초간 스크롤 입력 무시)
        if (Time.time < lastScrollTime + scrollCooldown) return;

        if (pointingSlot >= 0 && pointingSlot < slots.Count)
        {
            slots[pointingSlot].slotFrame.enabled = false;
        }

        pointingSlot = i;

        slots[i].toggle.isOn = true;
        slots[i].slotFrame.enabled = true;

        if (slots[i].item != null && player != null)
        {
            player.item = slots[i].item;
        }

        Debug.Log($"{i}번 슬롯 선택됨");
    }
}
