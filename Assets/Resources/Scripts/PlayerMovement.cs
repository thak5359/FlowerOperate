using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

    private Vector2 moveInput;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;

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

    void OnInteraction(InputValue value)
    {
        if(canInteractive == true)
        {if (value.isPressed == true)
            {
                Debug.Log("상호작용 버튼 ON");
                this.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        else if(canInteractive == false)
        {
            Debug.Log("주변에 상호작용할만한거 없음");
        }
    }
}