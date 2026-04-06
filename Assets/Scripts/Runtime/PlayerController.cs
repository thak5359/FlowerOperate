using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] public GameObject UseArea; // ������ ��� ���� (���� ������ ����)
    [SerializeField] public GameObject Plot;



    // ��¡ ������
    [Header("���� Ÿ���� ���� �ϴ� ���")]
    [Range(1, 2)]
    public float charTimePerPhase = 1.75f;
    private float chargeStartTime;
    private bool isCharging = false;
    float cachedSign;

    private Vector2 moveInput;
    private Rigidbody rb;

    // ��ȣ�ۿ� ���� ������ 
    private float interactCooldown = 0.2f;
    private float lastInteractTime = 0f;

    [SerializeField] private string messageTarget;

    public void setTag(string input_tag) => messageTarget = input_tag;
    Vector2 heading;  // ĳ���Ͱ� ���� �ִ� ���� ( ������ ���)
    Vector3 cached3Vec;

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

            // TODO: 차징 시작 애니메이션이나 이펙트 트리거
        }

        // 2. 버튼을 떼었을 때 (Canceled)
        if (context.canceled)
        {

            Instantiate(Plot, UseArea.transform.position, Quaternion.identity);

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

    void IInteractable.Interact(string Tag)
    {
        Debug.Log($" ޼     ۽  to :{Tag}");
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
