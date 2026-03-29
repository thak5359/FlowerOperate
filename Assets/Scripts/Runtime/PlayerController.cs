using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public interface IInteractable
{
    void Interact(string tag);
}



// ÇÃ·¹ÀÌ¾îÀÇ ÀÔ·Â ( WASD, »óÈ£ÀÛ¿ë, ¾ÆÀÌÅÛ »ç¿ë)À» Ã³¸®.
public class PlayerController : MonoBehaviour, IInteractable
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

    [Header("Ä³¸¯ÅÍ°¡ »óÈ£ÀÛ¿ë °¡´ÉÇÑ À§Ä¡")]
    [SerializeField] public Transform interactableArea;
    [SerializeField] public GameObject UseArea; // ¾ÆÀÌÅÛ »ç¿ë ¹üÀ§ (ÃßÈÄ »èÁ¦ÇÒ ¿¹Á¤)
    [SerializeField] public GameObject Plot;

    

    // Â÷Â¡ °ü¸®¿ë
    [Header("Â÷Áö Å¸ÀÓÀ» Á¶Àý ÇÏ´Â ±â´É")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;
    private float chargeStartTime;
    private bool isCharging = false;
    float cachedSign;

    private Vector2 moveInput;
    private Rigidbody rb;

    // »óÈ£ÀÛ¿ë ¿¬¼Ó ¹æÁö¿ë 
    private float interactCooldown = 0.2f;
    private float lastInteractTime = 0f;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    Vector2 heading;  // Ä³¸¯ÅÍ°¡ º¸°í ÀÖ´Â ¹æÇâ ( ¾ÆÀÌÅÛ »ç¿ë)
    Vector3 cached3Vec;

    [Inject]
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void Start()
    {
        if (UseArea.activeSelf == true)
        {
            UseArea.SetActive(false);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Move();
        interactableArea.localPosition = cached3Vec;
        SnapToWorldGrid(UseArea.transform, cached3Vec);
    }

    void Move()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        if (moveInput.x != 0)
        {
            //spriteRenderer.flipX = (moveInput.x < 0); // TODO :: MeshRenderer º¯°æÇÏ´Â ±â´ÉÀ¸·Î ¸¸µé±â!
        }

        if (moveInput != Vector2.zero)
        {
            if (moveInput.x != 0)
            {
                heading = (moveInput.x > 0) ? Vector2.right : Vector2.left;
                cached3Vec.Set(heading.x, 0.0f, 0.0f);
            }
            else
            {
                heading = (moveInput.y > 0) ? Vector2.up : Vector2.down;
                cached3Vec.Set(0.0f, 0.0f, heading.y);

            }

        }

    }

    public void OnInteract(InputAction.CallbackContext context)
    {


        if (canInteractive == true && context.canceled)
        {

            if (Time.time < lastInteractTime + interactCooldown)
            {
#if UNITY_EDITOR
                Debug.Log("ì¢€ ?´ì‚´ ì¢€ ?ŒëŸ¬ì£¼ì„¸??..");
#endif
                return;
            }

            // ³ª ÀÚ½Å(this)À» IInteractable·Î Çüº¯È¯ÇØ¼­ È£Ãâ
            ((IInteractable)this).Interact(this.messageTarget);
        }
    }

    // ³ªÁß¿¡ »èÁ¦ÇÒ ½Ã¿¬¿ë ÄÚµå. ÀÎº¥Åä¸®±îÁö ¿Ï¼ºµÇ¸é º¯°æ.
    public void OnUse(InputAction.CallbackContext context)
    {
        // 1. ¹öÆ°À» ´©¸£±â ½ÃÀÛÇßÀ» ¶§ (Started)
        if (context.started)
        {
            if (UseArea.activeSelf == true)
            {
                Debug.LogAssertion("¿À·ù! Å°ÀÔ·ÂÀÌ Àß¸øµÊ!");
                return;
            }



            // ¾Æ·¡ ºÎºÐÀº Å×½ºÆ® ³¡³ª¸é »èÁ¦ÇÒ ÇÔ¼öÀÓ.
            UseArea.SetActive(true);
            isCharging = true;
            chargeStartTime = Time.time;
        }


        // 2. ¹öÆ°À» ¶¼¾úÀ» ¶§ (Canceled)
        if (context.canceled)
        {
            // ¾Æ·¡ ºÎºÐÀº Å×½ºÆ® ³¡³ª¸é »èÁ¦ÇÒ ÇÔ¼öÀÓ.
            UseArea.SetActive(false);
            Instantiate(Plot, UseArea.transform.position, Quaternion.identity);
        }
    }

    private void SnapToWorldGrid(Transform targetPos, Vector3 offset)
    {
        Vector3 targetWorldPos = transform.position + offset;

        targetPos.position = new Vector3(Mathf.Round(targetWorldPos.x), 0.15f, Mathf.Round(targetWorldPos.z));
    }

    void IInteractable.Interact(string Tag)
    {
        Debug.Log($"¸Þ¼¼Áö ¼Û½Å to :{Tag}");
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }

    void Update()
    {
        if (isCharging)
        {
            //UpdateSelectionVisual(currentElapsed);
        }
    }

}
