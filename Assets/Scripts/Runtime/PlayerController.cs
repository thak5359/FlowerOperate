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



    public SlotItem item;

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
    

    public void OnInteract(InputAction.CallbackContext context)
    {
        // 상호작용 버튼이 눌렸을 때만 동작.
        if (context.started == true)
        {
            UnityEngine.Debug.Log("상호작용 누름");

            if (canInteractive == true)
            {
                UnityEngine.Debug.Log("상호작용 성공");
                this.GetComponent<SpriteRenderer>().color = Color.blue;
                //item.OnUse(heading, trans.localPosition);
            }

            else if (canInteractive == false)
            {
                UnityEngine.Debug.Log("주변에 상호작용할만한거 없음");
            }
        }
    }
}
