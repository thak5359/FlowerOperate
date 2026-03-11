using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarManager : MonoBehaviour
{
    [Header("핫키 슬롯을 등록해주세요")]
    [SerializeField] List<HotBarSlot> slots;
    [SerializeField] PlayerController player;

    private int pointingSlot = -1;

    private float scrollCooldown = 0.15f;
    private float lastScrollTime = 0.0f;

    void Awake()
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("Hotbar slots is NULL or Empty!");
        }
    }

    private void Start()
    {
        lastScrollTime = -scrollCooldown;
        pointSlot(0);
    }

    public void OnScrollMouse(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        Vector2 scrollDelta = value.ReadValue<Vector2>();
        if (Mathf.Abs(scrollDelta.y) < 0.1f) return;

        int newIndex = pointingSlot + (scrollDelta.y > 0 ? -1 : 1);
        newIndex = Mathf.Clamp(newIndex, 0, slots.Count - 1);

        pointSlot(newIndex);
    }

    public void pointSlot(int i)
    {
        //  범위 및 중복 체크
        if (i < 0 || i >= slots.Count) return;
        if (i == pointingSlot && slots[i].slotFrame.enabled) return;

        //  쿨타임 체크
        if (Time.time < lastScrollTime + scrollCooldown) return;

        // 이전 슬롯 끄기
        if (pointingSlot >= 0 && pointingSlot < slots.Count)
        {
            slots[pointingSlot].slotFrame.enabled = false;
        }

        // 새 슬롯 켜기
        pointingSlot = i;
        lastScrollTime = Time.time; // 시간 업데이트 필수!

        slots[i].toggle.isOn = true;
        slots[i].slotFrame.enabled = true;
        

        // 플레이어에게 정보 갱신
        //SyncPlayerItem();

        Debug.Log($"{i+1}번 슬롯 선택됨");
    }

    public void SyncPlayerItem()
    {
        if (pointingSlot < 0 || pointingSlot >= slots.Count) return;
        if (player != null)
        {
            player.SetItem(slots[pointingSlot].item);
        }
    }
}