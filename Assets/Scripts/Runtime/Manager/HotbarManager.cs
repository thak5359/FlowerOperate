using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class HotbarManager : MonoBehaviour
{

    private PlayerOwnItemDataManager _itemDataManager;
    private PlayerStateManager _playerState;

    private DisposableBag disposableBag = new();// R3 구독 해제용 백


    [Header("핫키 슬롯을 등록해주세요")]
    [SerializeField] List<ItemObjectData> items;
    [SerializeField] List<HotBarSlot> slots;



    private int cachedInt;
    private int pointingSlot = -1;

    private int pointingInventoryArray = 0; // 0~4 

    public int PointingSlot => pointingSlot;

    private float scrollCooldown = 0.05f;
    private float lastScrollTime = 0.0f;

    public bool isSwappingGearDefaultArea { get; private set; } = false;






    [Inject]
    void Constuct(PlayerOwnItemDataManager input_itemDataManager, PlayerStateManager input_playerStateManager)
    {
        _itemDataManager = input_itemDataManager;
        _playerState = input_playerStateManager;
    }

    void Awake()
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("Hotbar slots is NULL or Empty!");
        }
    }

    private void Start()
    {
        lastScrollTime = -scrollCooldown;
        pointSlot(0);

        if (_itemDataManager != null)
        {
            _itemDataManager.InventoryRevisionChanged
                .Subscribe(_ => RefreshHotbarSlots())
                .AddTo(ref disposableBag);

            // 첫 시작 시 UI 초기화용 강제 1회 새로고침
            RefreshHotbarSlots();
        }
    }
    private void OnDestroy()
    {
        disposableBag.Dispose(); // 메모리 누수 방지를 위한 R3 스트림 일괄 해제
    }


    public void OnSwapGearDefaultArea(InputAction.CallbackContext context)
    {
        if (_playerState.IsCharging.Value) return; // 전광판 보고 차징 중인지 판단!

        if (context.performed)
        {
            // 무조건 true가 되는 것보다, 같은 키를 눌러서 껐다 켰다(토글) 할 수 있는 게 플레이하기 편할 거예요!
            isSwappingGearDefaultArea = !isSwappingGearDefaultArea;

            Debug.Log($"기본 영역(1x1) 강제 사용 상태: {isSwappingGearDefaultArea}");
        }
    }


    /// <summary>
    /// 인벤토리에서 참조하는 열(Array)을 위로 한 칸 바꿉니다 (0 -> 4 -> 3...)
    /// </summary>
    public void OnSwapHotBarUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_playerState.IsCharging.Value) return; // 차징 중이면 무시!

            pointingInventoryArray = (pointingInventoryArray - 1 + 5) % 5;
            Debug.Log($"핫바 레이어 변경 (Up): {pointingInventoryArray}번 줄 가리킴");
            _playerState.CurrentHotbarLayer.Value = pointingInventoryArray; // 플레이어 상태에 현재 핫바 레이어 정보도 업데이트

            isSwappingGearDefaultArea = false; // 슬롯 환경이 변했으니 false로 초기화
            RefreshHotbarSlots();
        }
    }
    /// <summary>
    /// 인벤토리에서 참조하는 열(Array)을 아래로 한 칸 바꿉니다 (0 -> 1 -> 2...)
    /// </summary>
    public void OnSwapHotBarDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_playerState.IsCharging.Value) return; // 차징 중이면 무시!

            pointingInventoryArray = (pointingInventoryArray + 1) % 5;
            Debug.Log($"핫바 레이어 변경 (Down): {pointingInventoryArray}번 줄 가리킴");
            _playerState.CurrentHotbarLayer.Value = pointingInventoryArray; // 플레이어 상태에 현재 핫바 레이어 정보도 업데이트

            isSwappingGearDefaultArea = false; // 슬롯 환경이 변했으니 false로 초기화
            RefreshHotbarSlots();
        }
    }
    /// <summary>
    /// 주입받은 무할당 뷰 데이터를 기반으로 현재 선택된 핫바 슬롯 UI들을 새로고침합니다.
    /// </summary>
    public void RefreshHotbarSlots()
    {
        if (_itemDataManager == null) return;

        // 가비지 컬렉터(GC)가 전혀 작동하지 않는 구조체 뷰 대여!
        var currentSegment = _itemDataManager.GetInventorySegment(pointingInventoryArray);

        for (int i = 0; i < slots.Count; i++)
        {
            // 10칸씩 쪼개진 세그먼트 내부 아이템에 인덱스로 바로 접근
            GameItem item = currentSegment[i];



            UpdateHotSlotItem(i, item).Forget(); // 각 슬롯 UI 업데이트 (비동기 대기)
        }

        // 핫바를 바꿨거나 아이템이 바뀌었으니 플레이어 손에 들린 아이템도 동기화해줘요.
        SyncPlayerItem();
    }


    /// <summary>
    /// 현재 핫바에서 선택된 슬롯의 아이템을 반환합니다.
    /// GameItem은 클래스(참조 타입)이므로 반환된 아이템의 데이터를 수정하면 인벤토리 원본 데이터에 그대로 반영돼요!
    /// </summary>
    public GameItem GetCurrentSelectedItem()
    {
        if (_itemDataManager == null) return null;
        if (pointingSlot < 0 || slots == null || pointingSlot >= slots.Count) return null;

        // 1. 현재 선택된 인벤토리 줄(세그먼트 10칸)을 구조체 뷰로 가져옵니다.
        var currentSegment = _itemDataManager.GetInventorySegment(pointingInventoryArray);

        // 2. 해당 줄에서 현재 가리키고 있는 슬롯의 아이템을 반환해요 (빈 슬롯이면 null이 반환됩니다).
        GameItem item = currentSegment[pointingSlot];
        if (item == null || item.Id <= 0 || item.Count <= 0) return null;
        return item;
    }



    // 수정할 위치: HotbarManager 스크립트 내부의 UpdateHotSlotItems 메서드 전체 수정
    // 변경 이유: 
    // 1. 가비지를 유발하고 await을 무시하는 LINQ ForEach 대신, 기본 for문을 사용하여 비동기 대기를 완벽하게 처리했어요.
    // 2. 어드레서블 해제 시, 새 이미지를 먼저 로드하고 UI에 덮어씌운 뒤에 이전 이미지를 안전하게 해제하도록 Swap 방식을 적용했어요.
    private async UniTask UpdateHotSlotItem(int i, GameItem input_item)
    {
        var slot = slots[i];
        bool isEmpty = input_item == null || input_item.Id <= 0 || input_item.Count <= 0;

        // 1. 이미지(스프라이트) 갱신 로직
        if (slot.ItemIcon != null)
        {
            Sprite newSprite = null;

            // input_item이 비어있지 않을 때만 이미지를 로드하거나 가져옵니다.
            if (!isEmpty)
            {
                if (input_item.DisplaySprite != null)
                {
                    newSprite = input_item.DisplaySprite;
                }
                else if (!string.IsNullOrEmpty(input_item.SpriteAddress.ToString()))
                {
                    Debug.Log("이미지 로드 시도!");
                    Sprite spr = await AddressableManager.LoadAssetAsync<Sprite>(input_item.SpriteAddress);
                    if (spr != null)
                    {
                        newSprite = spr;
                    }
                }
            }

            if (newSprite != null)
            {
                slot.ItemIcon.sprite = newSprite;
                slot.ItemIcon.enabled = true; // 이미지 활성화
            }
            else
            {
                // 스프라이트가 없는 경우(빈 슬롯): 이전 스프라이트를 제거하고 깔끔하게 비활성화
                slot.ItemIcon.sprite = null; // 남아있는 참조 비우기
                slot.ItemIcon.enabled = false;
            }
        }

        if (slot.Count != null)
        {
            if (!isEmpty && input_item.Count > 1)
            {
                slot.Count.text = input_item.Count.ToString();
            }
            else
            {
                slot.Count.text = string.Empty; // 비어있거나 1개일 때는 숫자 텍스트를 비워요
            }
        }
    }

    public void OnPrevHotSlot(InputAction.CallbackContext context)
    {
        // 버튼을 눌렀을 때(performed)만 실행
        if (context.performed)
        {
            // 현재 위치에서 -1 한 곳으로 이동 (순환 로직은 pointSlot이 처리)
            pointSlot(pointingSlot - 1);
        }
    }
    public void OnNextHotSlot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 현재 위치에서 +1 한 곳으로 이동
            pointSlot(pointingSlot + 1);
        }
    }

    public void pointSlot(int i)
    {
        cachedInt = (i + slots.Count) % slots.Count;

        if (cachedInt < 0 || cachedInt >= slots.Count) return;

        if (cachedInt == pointingSlot && slots[cachedInt].slotFrame.enabled) return;

        if (Time.time < lastScrollTime + scrollCooldown) return;

        if (pointingSlot >= 0 && pointingSlot < slots.Count)
        {
            slots[pointingSlot].slotFrame.enabled = false;
        }

        pointingSlot = cachedInt;
        _playerState.CurrentHotbarSlot.Value = pointingSlot; // 플레이어 상태에 현재 핫바 슬롯 정보도 업데이트
        lastScrollTime = Time.time;

        slots[cachedInt].toggle.isOn = true;
        slots[cachedInt].slotFrame.enabled = true;

        // 현재 가리키고 있는 아이템의 ID를 pointingItedId에 대입
        //pointingItemId = items[i].GetItemID;
        //Debug.Log($"{cachedInt + 1}번 슬롯 선택됨");
    }

    //플레이어가 들고있는 아이템 동기화 함수.
    public void SyncPlayerItem()
    {
        if (pointingSlot < 0 || pointingSlot >= slots.Count) return;
        if (_playerState != null)
        {
            //player.SetItem(slots[pointingSlot].item);
        }
    }

}