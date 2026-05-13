using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using static Constant;

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

    [Header("캐릭터가 상호작용 가능한 위치")]
    [SerializeField] public Transform interactableArea;

    // 차징 관리용
    [Header("차지 타임을 조절 하는 기능. 아이템 데이터가 만들어지기 전까지 실험용임.")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;
    bool isCharging = false;

    [Header("캐릭터 이미지 칸 [앞] [옆] [뒤]")]
    [SerializeField] public List<Sprite> CharacterSprite = new(3);



    //이동 로직 처리 중 사용할 속도/캐싱용 Vec3
    private Vector3 targetVelocity;

    [SerializeField] private Animator anim;
    private static readonly int MoveXHash = Animator.StringToHash(ANIM_X);
    private static readonly int MoveYHash = Animator.StringToHash(ANIM_Y);
    private static readonly int isMovingHash = Animator.StringToHash(ANIM_MOVING);

    private UseAreaManager _useAreaManager;

    private Vector2 moveInput;
    private Rigidbody rigidBody;
    private SpriteRenderer sprRenderer;

    // 상호작용 연속 방지용 
    private float interactCooldown = 1f;
    private float lastInteractTime = 0f;


    public Vector2 heading = Vector2.down;  // 캐릭터가 보고 있는 방향 ( 아이템 사용)
    Vector3 cachedPosition = new Vector3(0.0f, 0.0f, -1.0f);
    Quaternion cachedRotation = Quaternion.identity;

    private Vector3 interactableBoxScale;

    private static int _mask;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        sprRenderer = GetComponentInChildren<SpriteRenderer>();
        _mask = LayerMask.GetMask(LAYER_INTERACTABLE);
        interactableBoxScale = interactableArea.gameObject.transform.localScale * 0.5f;
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
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

        interactableArea.localPosition = cachedPosition;
        interactableArea.localRotation = cachedRotation;

    }

    void Move()
    {
        // 입력이 없는 경우, 이동 애니메이션 끄고 속도 0으로 만들어서 멈추게 하기
        if (moveInput == Vector2.zero)
        {
            anim.SetBool(isMovingHash, false);
            rigidBody.velocity = new Vector3(0f, rigidBody.velocity.y, 0f);
            return;
        }

        anim.SetBool(isMovingHash, true);


        // 차징 중에 행동 불가능하게 만
        if (isCharging == true)
        {
            anim.SetBool(isMovingHash, false);

            targetVelocity.Set(0f, rigidBody.velocity.y, 0f);
            rigidBody.velocity = targetVelocity;

            return;
        }



        targetVelocity.Set(moveInput.x * moveSpeed, rigidBody.velocity.y, moveInput.y * moveSpeed);

        rigidBody.velocity = targetVelocity;
        if (moveInput != Vector2.zero)
        {

            if (moveInput.x != 0)
            {

                switchSpr(1);


                sprRenderer.flipX = (moveInput.x > 0) ? true : false;
                heading = (moveInput.x > 0) ? Vector2.right : Vector2.left;

                anim.SetFloat(MoveXHash, heading.x);
                anim.SetFloat(MoveYHash, .0f);

            }
            else
            {
                _ = (moveInput.y > 0) ? switchSpr(2) : switchSpr(0);
                heading = (moveInput.y > 0) ? Vector2.up : Vector2.down;

                bool isHeadingFront = (moveInput.y > 0) ? true : false;

                anim.SetFloat(MoveYHash, heading.y);
                anim.SetFloat(MoveXHash, .0f);


            }
            locateInteractable();
        }

    }

    public void OnInteract(InputAction.CallbackContext context)
    {

        //Debug.Log("OnInteracted has been detected 1 ");
        if (isCharging == false && context.canceled)
        {
            Debug.Log("OnInteracted has been detected 2 ");
            if (Time.time < lastInteractTime + interactCooldown)
            {
                Debug.Log("잠시 뒤에 말을 걸어보자...");
                return;
            }

            Collider[] hits = GetHits();
            if (hits.Length == 1)
            {

                if (hits[0].CompareTag(TAG_STORAGE))
                {
                    // TODO:: 창고 여는 스크립트 여기에 작성하기
                }

                if (hits[0].CompareTag(TAG_BED))
                {
                    GlobalEventManager.InvokeNextDay();
                }


                ((IInteractable)this).Interact(hits[0].gameObject.tag);
            }
            else if (hits.Length > 1)
            {
                Debug.Log($"한번에 여러 상호작용 대상이 들어왔습니다 \n. {hits.ToString()}");
            }
        }
    }


    void IInteractable.Interact(string Tag)
    {
        Debug.Log($"메세지 송신 to :{Tag}");
        Debug.Log("OnInteracted has been detected 3 ");
        Fungus.Flowchart.BroadcastFungusMessage(Tag);
    }

    private Collider[] GetHits()
    {
        return Physics.OverlapBox(interactableArea.position, interactableBoxScale, cachedRotation, _mask);
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
            //Debug.Log("Use 버튼이 떼어졌습니다. 아이템 사용 시도!");
            _useAreaManager.Fire(); // 발사!
        }
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

    private void locateInteractable()
    {
        if (heading == Vector2.right)
        {
            cachedPosition.Set(heading.x, 0.0f, 0.0f);

            cachedRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
        }
        if (heading == Vector2.left)
        {
            cachedPosition.Set(heading.x, 0.0f, 0.0f);

            cachedRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        }
        if (heading == Vector2.up || heading == Vector2.down)
        {
            cachedPosition.Set(0.0f, 0.0f, heading.y);
            cachedRotation = Quaternion.identity;
        }
    }

    private void OnDisable()
    {
        _useAreaManager.CancelCharging();
    }

}
