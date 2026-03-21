using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public interface IInteractable
{

    void Interact(string tag);
}


public class PlayerController : MonoBehaviour, IInteractable
{
    private static PlayerController instance;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;


    [Header("캐릭터가 상호작용 가능한 위치")]
    [SerializeField]public  Transform interactableArea;
    Vector3 cachedVec3;



    private float chargeStartTime;
    private bool isCharging = false;

    public Item item;

    private Vector2 moveInput;
    private Transform trans;
    private Rigidbody rb;
    private Animation anim;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    Vector2 heading; // 캐릭터가 보고 있는 방향 ( 아이템 사용)

    public static PlayerController Instance()
    {
        if (instance != null)
        { 
            return instance;
        }
        else return null;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        trans = GetComponent<Transform>();
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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
            //spriteRenderer.flipX = (moveInput.x < 0); // TODO :: MeshRenderer 변경하는 기능으로 만들기!
        }
        // 4방향 애니메이션이 예정되어있다는 가정하의 조건문. 
        if (moveInput.x > 0)
        {
            heading.Set(1.0f, 0.0f);
            cachedVec3.Set(1.0f, 0.0f, 0.0f);
            interactableArea.localPosition = cachedVec3;
        }
        else if (moveInput.x < 0)
        {
            heading.Set(-1.0f, 0.0f);
            cachedVec3.Set(-1.0f, 0.0f, 0.0f);
            interactableArea.localPosition = cachedVec3;
        }
        else if (moveInput.y > 0)
        {
            heading.Set(0.0f, 1.0f);
            cachedVec3.Set(0.0f, 0.0f, 1.0f);
            interactableArea.localPosition = cachedVec3;
        }
        else if (moveInput.y < 0)
        {
            heading.Set(0.0f, -1.0f);
            cachedVec3.Set(0.0f, 0.0f, -1.0f);
            interactableArea.localPosition = cachedVec3;
        }
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canInteractive == true)
        {
            // 나 자신(this)을 IInteractable로 형변환해서 호출해야 합니다.
            ((IInteractable)this).Interact(this.messageTarget);
        }
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        //   if (currentItem == null) return;


        // 1. 버튼을 누르기 시작했을 때 (Started)
        if (context.started)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            Debug.Log("<color=yellow>[Item]</color> 차징 시작...!");

            // TODO: 차징 시작 애니메이션이나 이펙트 트리거
        }

        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {
            if (!isCharging) return;

            float totalChargeTime = Time.time - chargeStartTime;
            isCharging = false;

            // 차징 시간을 포함하여 UseParam 생성
            UseParam param = new UseParam(
             heading,            // 캐릭터가 바라보는 2D 방향 (Vector2)
             transform.position, // 현재 플레이어의 3D 위치
             10,                 // 기본 효율 (나중에 아이템 등급에 따라 변경 가능)
             totalChargeTime     // 실제 버튼을 누르고 있었던 시간
         );

            // 결과 출력
            PrintUseResult(param);

            //currentItem.OnUse(param);
            //selectionArea.SetActive(false);

            //// SelectionArea 스케일 초기화
            //selectionArea.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f);
        }
    }

    private void PrintUseResult(UseParam param)
    {
        string directionName = GetDirectionName(param.heading);
        Debug.Log($"<color=cyan><b>[Item Use Result]</b></color>\n" +
                  $"방향: {directionName} ({param.heading})\n" +
                  $"위치: {param.pos}\n" +
                  $"차징 시간: {param.elapsedTime:F2}초\n" +
                  $"효율: {param.efficiency}");
    }

    // --- [추가] 방향 벡터를 읽기 쉬운 텍스트로 변환 ---
    private string GetDirectionName(Vector2 h)
    {
        if (h == Vector2.up) return "위(North)";
        if (h == Vector2.down) return "아래(South)";
        if (h == Vector2.left) return "왼쪽(West)";
        if (h == Vector2.right) return "오른쪽(East)";
        return "방향 알 수 없음";
    }


    void IInteractable.Interact(string Tag)
    {
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }

    void Update()
    {
        if (isCharging)
        {
            float currentElapsed = Time.time - chargeStartTime;
            //UpdateSelectionVisual(currentElapsed);
        }
    }

    //private void UpdateSelectionVisual(float elapsed)
    //{
    //    // ItemManager에서 현재 아이템의 ChargeInfo를 가져와서 
    //    // 시간에 따라 selectionArea의 스케일을 키워줍니다.
    //    ChargeInfo info = ItemManager.Instance.GetChargeInfo((int)currentItem.itemId);

    //    if (elapsed >= info.ChargeTime)
    //    {
    //        // 예: 차징 완료 시 범위를 1x3 또는 3x3 느낌으로 확장 ($3 \times 3$ 유닛 등)
    //        selectionArea.transform.localScale = new Vector3(0.8f, 0.01f, 2.4f);
    //    }
    //}
    //public Item currentItem; // 현재 쥔 아이템

    //public void SetItem(Item newItem)
    //{
    //    currentItem = newItem;
    //    UnityEngine.Debug.Log(currentItem != null ? $"{currentItem.GetName()} 장착됨" : "맨손 상태");
    //}


}
