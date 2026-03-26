using Fungus;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public interface IInteractable
{

    void Interact(string tag);
}

public struct itemData
{

    float chargeTIme1;
    float chargeTIme2;
    float chargeTIme3;

    int maxChargeCount;
    string itemType;

}



public class PlayerController : MonoBehaviour, IInteractable
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool canInteractive = false;


    [Header("캐릭터가 상호작용 가능한 위치")]
    [SerializeField] public Transform interactableArea;
    Vector3 cached3Vec1;
    Vector3 cached3Vec2;


    [SerializeField] public GameObject UseArea;

    //test용 코드, 땅 생성하기 (현재 아이템 데이터 없어서 스킵)
    [SerializeField] public GameObject obj;


    // 상호작용 연속 방지용 
    private float interactCooldown = 0.2f;
    private float lastInteractTime = 0f;
    
    private List<GameObject> useAreaList = new List<GameObject>();


    // 차징 관리용
    [Header("차지 타임을 조절 하는 기능")]
    [Range(1,2)]
    public float charTimePerPhase = 1.75f;
    private float chargeStartTime;
    private bool isCharging = false;
    float cachedSign;

    private Vector2 moveInput;
    private Transform trans;
    private Rigidbody rb;
    private Animation anim;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    Vector2 heading; // 캐릭터가 보고 있는 방향 ( 아이템 사용)


    void Awake()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
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
        interactableArea.localPosition = cached3Vec1;
        SnapToWorldGrid(UseArea.transform, cached3Vec1);
    }

    void Move()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        if (moveInput.x != 0)
        {
            //spriteRenderer.flipX = (moveInput.x < 0); // TODO :: MeshRenderer 변경하는 기능으로 만들기!
        }

        if(moveInput != Vector2.zero)
        {
            if (moveInput.x != 0)
            {
                cachedSign = (moveInput.x > 0 )? 1 : -1f;
                heading.Set(cachedSign, 0.0f);
                cached3Vec1.Set(cachedSign, 0.0f, 0.0f);
            }
            else 
            {
                cachedSign = (moveInput.y > 0) ? 1 : -1f;
                heading.Set(0.0f, cachedSign);
                cached3Vec1.Set(0.0f, 0.0f, cachedSign);
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
                Debug.Log("좀 살살 좀 눌러주세요...");
                #endif
                return;
            }

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
            if (UseArea.activeSelf == true)
            {
                Debug.LogAssertion("오류! 키입력이 잘못됨!");
                return;
            }

            UseArea.SetActive(true);

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

            cached3Vec2 = UseArea.gameObject.transform.position;
            cached3Vec2.y = 0.2f;


            float chargePhase = MathF.Floor(param.elapsedTime / charTimePerPhase);

            // 

            if( chargePhase >= 0.0f )
            {
                Instantiate(obj, cached3Vec2, Quaternion.identity);
            }
            if( chargePhase >= 1.0f)
            {

            }
            if (chargePhase >= 2.0f)
            {

            }
            if (chargePhase >= 3.0f)
            {

            }
            if (chargePhase >= 4.0f)
            {

            }



            Instantiate(obj, cached3Vec2, Quaternion.identity);

            #if UNITY_EDITOR
            // 결과 출력
            PrintUseResult(param);
            #endif
           
            UseArea.SetActive(false);
        }
    }

    //선택 영역을 반올림/ 반내림 처리해주는 함수
    private void SnapToWorldGrid(Transform targetPos, Vector3 offset)
    {
        Vector3 targetWorldPos = transform.position + offset;

        float snappedX = Mathf.Round(targetWorldPos.x);
        float snappedZ = Mathf.Round(targetWorldPos.z);

        targetPos.position = new Vector3(snappedX, 0.15f, snappedZ);



    }


#if UNITY_EDITOR
    private void PrintUseResult(UseParam param)
    {
        string directionName = GetDirectionName(param.heading);
        Debug.Log($"<color=cyan><b>[Item Use Result]</b></color>\n" +
                  $"방향: {directionName} ({param.heading})\n" +
                  $"위치: {param.pos}\n" +
                  $"차징 시간: {param.elapsedTime:F2}초\n" +
                  $"효율: {param.efficiency}");
    }

    private string GetDirectionName(Vector2 h)
    {
        if (h == Vector2.up) return "위(North)";
        if (h == Vector2.down) return "아래(South)";
        if (h == Vector2.left) return "왼쪽(West)";
        if (h == Vector2.right) return "오른쪽(East)";
        return "방향 알 수 없음";
    }

#endif

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

    //private async void SampleItemUseCode()
    //{
    //    // TODO :: 나중에 item 자식 클래스로 각 장비별 클래스를 만든 뒤에 as로 검사 방식을 좀 더 똑똑하게 하기!

    //    switch (item.itemId)
    //    {
    //        case (3):


    //        case (4):
    //            break;

    //        case (5):

    //        case (6):
    //            break;

    //        case (7):

    //        case (8):
    //        case (9):
    //            break;




    //    }


    //    for (int i = 0; i < 10; i++)
    //    {
    //        useAreaList[i] = await AddressableManager.LoadAssetAsync<GameObject>("UseArea");
    //    }




    //}
}
