using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;



public class Flower
{
    public float grade; // 꽃의 등급, 
    public int growth; // 꽃이 성장 단계 0: 씨앗 1, 2, 3(수확가능)
    public int itemNumber; // 
    public int worth; //
    public int amount;//
}

public class Soil
{
    public Flower flower;
    public bool isWatered;

    public void grow()
    {

    }
}


public class Sickle : Item 
{
    public  override void onUse()
    {



    }
    protected override void useItem()
    {
        UnityEngine.Debug.Log("아이템 사용됨");
    }
}


public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

    public Item item;
    public PlayerInput IA;


    private Vector2 moveInput;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;


    IItem quickSlotitem;




    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;


    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
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
    }

    protected void OnInteraction(InputValue value)
    {

        // 상호작용 버튼이 눌렸을 때만 동작.
        if (value.isPressed == true)
        {
            if (canInteractive == true)

            {
                UnityEngine.Debug.Log("상호작용 버튼 ON");
                this.GetComponent<SpriteRenderer>().color = Color.blue;
            }

            else if (canInteractive == false)
            {
                UnityEngine.Debug.Log("주변에 상호작용할만한거 없음");

                if (quickSlotitem != null && quickSlotitem is IUsable)
                {
                }
            }
        }
    }
}
