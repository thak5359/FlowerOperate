using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class HotbarManager : MonoBehaviour
{

    [Header("핫키 슬롯을 등록해주세요")]
    [SerializeField] List<ItemObjectData> items;
    [SerializeField] List<HotBarSlot> slots;
    [SerializeField] PlayerController player;

    private InventoryManager inventoryManager;

    private int cachedInt;
    private int pointingSlot = -1;

    private float scrollCooldown = 0.15f;
    private float lastScrollTime = 0.0f;

    private int pointingItemId;

    public int PointingItemId => pointingItemId;

    [Inject]
    public void Construct(InventoryManager inven)
    {
        inventoryManager = inven;
        Debug.Log("HotbarManager InventoryManager 의존성 주입 완료!");
    }

    void Awake()
    {

        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("Hotbar slots is NULL or Empty!");
        }

        if(items == null || items.Count == 0)
        {
            Debug.LogWarning("Hotbar items list is NULL or Empty! Defaulting to empty items.");
            items = new List<ItemObjectData>();
        }
    }

    private void Start()
    {
        lastScrollTime = -scrollCooldown;
        pointSlot(0);
    }

    private void UpdateSlotData()
    {
        for(int i = 0; i<10; i++)
        {
            if (i < inventoryManager.GetData.GetList.Count)
            {
                items[i] = inventoryManager.GetData.GetList[i];
            }
            else
            {
                items[i] = default;
            }
        }
    }

    public void OnPrevHotSlot(InputAction.CallbackContext context)
    {
        // 버튼을 눌렀을 때(performed)만 실행
        if (context.performed)
        {
            // 현재 위치에서 -1 한 곳으로 이동 (순환 로직은 pointSlot이 처리)
            pointSlot(pointingSlot - 1);
        }
    }
    public void OnNextHotSlot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 현재 위치에서 +1 한 곳으로 이동
            pointSlot(pointingSlot + 1);
        }
    }
    public void pointSlot(int i)
    {
        cachedInt = (i + slots.Count) % slots.Count;

        if (cachedInt < 0 || cachedInt >= slots.Count) return;

        if (cachedInt == pointingSlot && slots[cachedInt].slotFrame.enabled) return;

        if (Time.time < lastScrollTime + scrollCooldown) return;

        if (pointingSlot >= 0 && pointingSlot < slots.Count)
        {
            slots[pointingSlot].slotFrame.enabled = false;
        }

        pointingSlot = cachedInt;
        lastScrollTime = Time.time;

        slots[cachedInt].toggle.isOn = true;
        slots[cachedInt].slotFrame.enabled = true;

        Debug.Log($"{cachedInt + 1}번 슬롯 선택됨");
    }


    public void SyncPlayerItem()
    {
        if (pointingSlot < 0 || pointingSlot >= slots.Count) return;
        if (player != null)
        {
            //player.SetItem(slots[pointingSlot].item);
        }
    }
}