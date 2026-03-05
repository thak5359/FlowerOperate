using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

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
                Debug.Log("상호작용 버튼 ON");
                this.GetComponent<SpriteRenderer>().color = Color.blue;
            }

            else if (canInteractive == false)
            {
                Debug.Log("주변에 상호작용할만한거 없음");

                if (quickSlotitem != null && quickSlotitem is IUsable)
                {
                }


            }
        }

        

    }















}