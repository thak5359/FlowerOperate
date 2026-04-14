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
    [SerializeField] public GameObject UseArea; // 아이템 사용 범위 (추후 삭제할 예정)
    [SerializeField] public GameObject Plot;


    // 차징 관리용
    [Header("차지 타임을 조절 하는 기능. 아이템 데이터가 만들어지기 전까지 실험용임.")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;

    private UseAreamanager _useAreaManager;

    private Vector2 moveInput;
    private Rigidbody rb;

    // 상호작용 연속 방지용 
    private float interactCooldown = 0.2f;
    private float lastInteractTime = 0f;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    public Vector2 heading;  // 캐릭터가 보고 있는 방향 ( 아이템 사용)
    Vector3 cached3Vec;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Inject]
    void Construct(UseAreamanager input_UseAreaManager)
    {
        _useAreaManager = input_UseAreaManager;
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
                return;
            }

            // 나 자신(this)을 IInteractable로 형변환해서 호출
            ((IInteractable)this).Interact(this.messageTarget);
        }
    }

    // 나중에 삭제할 시연용 코드. 인벤토리까지 완성되면 변경.
    public void OnUse(InputAction.CallbackContext context)
    {
        // 1. 버튼을 누르기 시작했을 때 (Started)
        if (context.started)
        {
            if (UseArea.activeSelf == true)
            {
                Debug.LogAssertion("오류! 키입력이 잘못됨!");
                return;
            }



            _useAreaManager.StartCharging(this.transform, heading);// 차징 시작!

        }


        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {


            _useAreaManager.Fire(); // 발사!
        }
    }

    private void SnapToWorldGrid(Transform targetPos, Vector3 offset)
    {
        Vector3 targetWorldPos = transform.position + offset;

        targetPos.position = new Vector3(Mathf.Round(targetWorldPos.x), 0.15f, Mathf.Round(targetWorldPos.z));
    }

    void IInteractable.Interact(string Tag)
    {
        Debug.Log($"메세지 송신 to :{Tag}");
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }

}
