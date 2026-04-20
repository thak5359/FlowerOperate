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
    public float decelereationRate = 0.5f;

    bool isCharging = false;

    public bool canInteractive = false;

    [Header("캐릭터가 상호작용 가능한 위치")]
    [SerializeField] public Transform interactableArea;


    // 차징 관리용
    [Header("차지 타임을 조절 하는 기능. 아이템 데이터가 만들어지기 전까지 실험용임.")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;


    [Header("캐릭터 이미지 칸 [앞] [옆] [뒤]")]
    [SerializeField] public List<Sprite> CharacterSprite = new(3);


    private UseAreaManager _useAreaManager;

    private Vector2 moveInput;
    private Rigidbody rb;
    private SpriteRenderer sprRenderer;

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
        sprRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    [Inject]
    void Construct(UseAreaManager input_UseAreaManager)
    {
        _useAreaManager = input_UseAreaManager;
    }

    void Start()
    {
        sprRenderer.sprite = CharacterSprite[0];
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
        Vector3 targetVelocity;
        if (isCharging == true)
        { targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed * decelereationRate; }
        else
            targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;


        //Debug.Log($"MoveInput: {moveInput.x}, {moveInput.y}");
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        if (moveInput != Vector2.zero && isCharging == false)
        {
            if (moveInput.x != 0)
            {

                switchSpr(1);
                
                sprRenderer.flipX = (moveInput.x > 0) ? true : false;
                heading = (moveInput.x > 0) ? Vector2.right : Vector2.left;

                cached3Vec.Set(heading.x, 0.0f, 0.0f);
            }
            else
            {
                _ = (moveInput.y > 0) ? switchSpr(2) : switchSpr(0);
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
            isCharging = true;
            _useAreaManager.StartCharging(this.transform, heading);// 차징 시작!
        }

        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {
            isCharging = false;
            _useAreaManager.Fire(); // 발사!
        }
    }

    void IInteractable.Interact(string Tag)
    {
        Debug.Log($"메세지 송신 to :{Tag}");
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }
    /// <summary>
    /// [Front: 0] [Side : 1] [Rear : 2]
    /// </summary>
    /// <param name="idx"></param>
    int switchSpr(int idx)
    {

        if (CharacterSprite.Count < 3)
        {
            Debug.Log($"CharacerSprite.count is {CharacterSprite.Count}!");
            return -1;
        }

        if (sprRenderer.sprite != CharacterSprite[idx])
            sprRenderer.sprite = CharacterSprite[idx];
        return 0;
    }
}
