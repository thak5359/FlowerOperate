using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarManager : MonoBehaviour
{
    [Header("핫키 슬롯을 등록해주세요")]
    [SerializeField] List<HotBarSlot> slots;
    [SerializeField] PlayerController player;

    private int cachedInt;
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
        // 수정 위치: i 대신 cachedInt를 끝까지 활용해야 합니다.
        cachedInt = (i + slots.Count) % slots.Count;

        // 1. 범위 체크 (이미 순환 로직을 썼으므로 사실상 통과하지만 안전장치로 유지)
        if (cachedInt < 0 || cachedInt >= slots.Count) return;

        // 2. 중복 체크 (i 대신 cachedInt 사용)
        if (cachedInt == pointingSlot && slots[cachedInt].slotFrame.enabled) return;

        // 3. 쿨타임 체크
        if (Time.time < lastScrollTime + scrollCooldown) return;

        // 이전 슬롯 끄기
        if (pointingSlot >= 0 && pointingSlot < slots.Count)
        {
            slots[pointingSlot].slotFrame.enabled = false;
        }

        // 새 슬롯 켜기
        pointingSlot = cachedInt;
        lastScrollTime = Time.time;

        // ★ 수정: slots[i]가 아니라 slots[cachedInt]를 써야 i가 -1일 때 에러가 안 납니다!
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