using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public interface IInteractable
{
    void Interact(string tag);
}


// 플레이어의 입력 ( WASD, 상호작용, 아이템 사용)을 처리.
public class PlayerController : MonoBehaviour, IInteractable
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;

    [Header("캐릭터가 상호작용 가능한 위치")]
    [SerializeField] public Transform interactableArea;


    // 차징 관리용
    [Header("차지 타임을 조절 하는 기능. 아이템 데이터가 만들어지기 전까지 실험용임.")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;

    private UseAreaManager _useAreaManager;

    private Vector2 moveInput;
    private Rigidbody rb;

    // 상호작용 연속 방지용 
    private float interactCooldown = 1f;
    private float lastInteractTime = 0f;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    public Vector2 heading = Vector2.down;  // 캐릭터가 보고 있는 방향 ( 아이템 사용)
    Vector3 cached3Vec;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Inject]
    void Construct(UseAreaManager input_UseAreaManager)
    {
        _useAreaManager = input_UseAreaManager;
    }

    void Start()
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Move();
        interactableArea.localPosition = cached3Vec;
    }

    void Move()
    {



        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

        //Debug.Log($"MoveInput: {moveInput.x}, {moveInput.y}");
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        if (moveInput.x != 0)
        {
            //spriteRenderer.flipX = (moveInput.x < 0); // TODO :: MeshRenderer 변경하는 기능으로 만들기!

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
                Debug.Log("잠시 뒤에 말을 걸어보자...");
                return;
            }

            // 나 자신(this)을 IInteractable로 형변환해서 호출
            ((IInteractable)this).Interact(this.messageTarget);
        }
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        // 1. 버튼을 누르기 시작했을 때 (Started)
        if (context.started)
        {

            _useAreaManager.StartCharging(this.transform, heading);// 차징 시작!
        }


        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {
            _useAreaManager.Fire(); // 발사!
        }
    }

    void IInteractable.Interact(string Tag)
    {
        Debug.Log($"메세지 송신 to :{Tag}");
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }

}
