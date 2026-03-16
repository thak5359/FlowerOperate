using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

    private float chargeStartTime;
    private bool isCharging = false;

    public Item item;

    private Vector2 moveInput;
    private Transform trans;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Animation anim;
    Vector2 heading; // 캐릭터가 보고 있는 방향 ( 아이템 사용)



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        trans = GetComponent<Transform>();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        UnityEngine.Debug.Log($"{moveInput}");
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = (moveInput.x < 0);
        }
        // 4방향 애니메이션이 예정되어있다는 가정하의 조건문. 
        if (moveInput.x > 0)
        {
            heading.Set(1.0f, 0.0f);
        }
        else if (moveInput.x < 0)
        {
            heading.Set(-1.0f, 0.0f);
        }
        else if ( moveInput.y > 0)
        {
            heading.Set( 0.0f, 1.0f);
        }
        else if ( moveInput.y < 0)
        {
            heading.Set(0.0f, -1.0f);
        }
    }

    [SerializeField] private GameObject selectionArea;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (currentItem == null) return;

        // 1. 버튼을 누르기 시작했을 때 (Started)
        if (context.started)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            selectionArea.SetActive(true);
        }

        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {
            float totalChargeTime = Time.time - chargeStartTime;
            isCharging = false;

            // 차징 시간을 포함하여 UseParam 생성
            UseParam param = new UseParam(
                heading,
                transform.position,
                10, // 효율
                totalChargeTime // 소요 시간 추가
            );

            currentItem.OnUse(param);
            selectionArea.SetActive(false);

            // SelectionArea 스케일 초기화
            selectionArea.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f);
        }
    }

    void Update()
    {
        if (isCharging)
        {
            float currentElapsed = Time.time - chargeStartTime;
            //UpdateSelectionVisual(currentElapsed);
        }
    }

    private void UpdateSelectionVisual(float elapsed)
    {
        // ItemManager에서 현재 아이템의 ChargeInfo를 가져와서 
        // 시간에 따라 selectionArea의 스케일을 키워줍니다.
        ChargeInfo info = ItemManager.Instance.GetChargeInfo((int)currentItem.itemId);

        if (elapsed >= info.ChargeTime)
        {
            // 예: 차징 완료 시 범위를 1x3 또는 3x3 느낌으로 확장 ($3 \times 3$ 유닛 등)
            selectionArea.transform.localScale = new Vector3(0.8f, 0.01f, 2.4f);
        }
    }
    public Item currentItem; // 현재 쥔 아이템

    public void SetItem(Item newItem)
    {
        currentItem = newItem;
        UnityEngine.Debug.Log(currentItem != null ? $"{currentItem.GetName()} 장착됨" : "맨손 상태");
    }

   
}
