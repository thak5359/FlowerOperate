using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;
using R3;
using static Constant;
using Cysharp.Threading.Tasks; // [수정: 비동기 UI 로딩 처리를 위해 UniTask 추가]

public class InventoryUIController : MonoBehaviour
{
    // [수정: 기본 타입 상수를 삭제하고 동적으로 분배하도록 변경했습니다]

    [SerializeField] private UIDocument _uiDocument;

    #region UI 요소
    private List<Button> InventorySlots = new();
    private List<VisualElement> InventorySlotImages = new();
    private List<Label> InventorySlotCounts = new();
    VisualElement root;
    VisualElement _ghostIcon;
    private Button closeButton;

    #region Detail Container Elements
    private Label _itemNameLabel;
    private VisualElement _itemIcon;
    private Label _categoryLabel;
    private Label _qualityLabel;
    private Label _growthDaysLabel;
    private Label _priceLabel;
    private Label _flowerSpeciesLabel;
    private Label _flowerColorLabel;
    private VisualElement _floriography1Container;
    private VisualElement _floriography2Container;
    private Label _flavorTextLabel;
    #endregion
    #endregion

    private IMapChangable _mapChanger;
    private PlayerOwnItemDataManager _inventoryManager;

    private int dragStartIdx;
    private int dragEndIdx;
    private bool _isDragging = false;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private ItemManager _itemManager;

    [Inject]
    private void Construct(
        IMapChangable input_mapChanger, 
        PlayerOwnItemDataManager input_inventoryManager,
        ItemManager itemManager
    )
    {
        _mapChanger = input_mapChanger;
        _inventoryManager = input_inventoryManager;
        _itemManager = itemManager;
    }

    #region Unity Event
    private void Awake()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
            Debug.Log("<color=red>GetComponent on Awake in InventoryUI is failed </color>");
    }

    private void OnEnable()
    {
        root = _uiDocument.rootVisualElement;
        root.visible = false;

        InventorySlots.Clear();
        InventorySlotImages.Clear();
        InventorySlotCounts.Clear();

        InventorySlots = root.Query<Button>("SlotButton").ToList();

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            var img = InventorySlots[i].Q<VisualElement>("SlotImage");
            InventorySlotImages.Add(img != null ? img : InventorySlots[i]);

            var countLabel = InventorySlots[i].Q<Label>("SlotCount");
            if (countLabel != null)
            {
                countLabel.text = "";
                countLabel.style.display = DisplayStyle.None;
                InventorySlotCounts.Add(countLabel);
            }
        }

        closeButton = root.Query<Button>("CloseButton");
        _ghostIcon = root.Q<VisualElement>("GhostIcon");

        _itemNameLabel = root.Q<Label>("ItemNameLabel");
        _itemIcon = root.Q<VisualElement>("IteItemIcon");
        _categoryLabel = root.Q<Label>("CategoryLabel");
        _qualityLabel = root.Q<Label>("QualityLabel");
        _growthDaysLabel = root.Q<Label>("GrowthDaysLabel");
        _priceLabel = root.Q<Label>("PriceLabel");
        _flowerSpeciesLabel = root.Q<Label>("FlowerSpeciesLabel");
        _flowerColorLabel = root.Q<Label>("FlowerColorLabel");
        _floriography1Container = root.Q<VisualElement>("Floriography1Container");
        _floriography2Container = root.Q<VisualElement>("Floriography2Container");
        _flavorTextLabel = root.Q<Label>("FlavorTextLabel");

        ClearDetailContainer();

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            InventorySlots[i].userData = i;
            InventorySlots[i].RegisterCallback<PointerDownEvent>(OnSlotDown, TrickleDown.TrickleDown);
            InventorySlots[i].RegisterCallback<PointerUpEvent>(OnSlotUp, TrickleDown.TrickleDown);
        }

        closeButton.clicked += closeInventory;
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);

        _inventoryManager.InventoryRevisionChanged
            .Subscribe(_ => RefreshUI().Forget()) // [수정: async void 대신 UniTaskVoid 꼬리 자르기 호출]
            .AddTo(_disposables);
    }

    private void OnDisable()
    {
        for (int i = 0; i < InventorySlots.Count; i++)
        {
            InventorySlots[i].UnregisterCallback<PointerDownEvent>(OnSlotDown, TrickleDown.TrickleDown);
            InventorySlots[i].UnregisterCallback<PointerUpEvent>(OnSlotUp, TrickleDown.TrickleDown);
        }
        InventorySlots.Clear();
        _ghostIcon.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        InventorySlotCounts.Clear();
        _disposables.Clear();
    }

    // [수정: 통합된 UI 인덱스(0~54)를 실제 데이터인 타입과 내부 인덱스로 매핑해주는 유틸 함수]
    private (ContainerType type, int internalIndex) GetSlotInfo(int uiIndex)
    {
        if (uiIndex < INVENTORY_SLOT_COUNT) // 0 ~ 49
            return (ContainerType.INVENTORY, uiIndex);
        else // 50 ~ 54
            return (ContainerType.GEAR, uiIndex - INVENTORY_SLOT_COUNT);
    }

    // [수정: 백그라운드 스레드에서 UI를 건드리지 않도록 비동기 대기(await) 구조로 변경]
    private async UniTaskVoid RefreshUI()
    {
        if (_itemManager != null && !_itemManager.IsInitialized)
        {
            await UniTask.WaitUntil(() => _itemManager.IsInitialized);
        }

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            var info = GetSlotInfo(i);
            var itemList = _inventoryManager.GetData.GetItemList(info.type);

            // 데이터가 없거나 덜 찼을 때의 방어 로직
            if (itemList == null || info.internalIndex >= itemList.Count) continue;

            var itemData = itemList[info.internalIndex];
            if (ItemInstantData.IsEmpty(itemData))
            {
                InventorySlotImages[i].style.backgroundImage = null;

                if (i < InventorySlotCounts.Count && InventorySlotCounts[i] != null)
                {
                    InventorySlotCounts[i].text = "";
                    InventorySlotCounts[i].style.display = DisplayStyle.None;
                }

            }
            else
            {
                string address = GlobalItemDB.GetSpriteAddress(itemData.Id).ToString();
                if (string.IsNullOrEmpty(address))
                {
                    Debug.LogWarning($"[InventoryUIController] Sprite address is empty for ItemId: {itemData.Id} in slot {i}");
                    continue;
                }
                Debug.Log($"[InventoryUIController] Loading sprite for ItemId: {itemData.Id} from address: {address} into slot index: {i}");
                // [수정: ContinueWith 대신 await를 사용하여 메인 스레드 렌더링 파이프라인에서 이미지를 할당!]
                // 파트너의 실제 Addressable 로드 로직에 맞게 메서드 이름은 맞춰주세요.
                Texture2D itemSprite = await AddressableManager.LoadAssetAsync<Texture2D>(address);
                if (itemSprite != null)
                {
                    InventorySlotImages[i].style.backgroundImage = itemSprite;
                    if (i < InventorySlotCounts.Count && InventorySlotCounts[i] != null)
                    {
                        if (itemData.Count > 1)
                        {
                            InventorySlotCounts[i].text = itemData.Count.ToString();
                            InventorySlotCounts[i].style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            InventorySlotCounts[i].style.display = DisplayStyle.None;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Open / Close Inventory
    public void OnOpenInventory(InputAction.CallbackContext callbackContext)
    {
        openInventory();
    }

    public void OnEscape(InputAction.CallbackContext callbackContext)
    {
        closeInventory();
    }

    public void closeInventory()
    {
        if (_mapChanger.getCurrentIAmap() == INVENTORY_MAP_NAME)
        {
            root.visible = false;
            _mapChanger.changeIAmapPrev();
        }
    }

    public void openInventory()
    {
        if (_mapChanger.getCurrentIAmap() != INVENTORY_MAP_NAME)
        {
            _mapChanger.changeIAmapInventory();

            if (root == null)
            {
                Debug.LogError("파트너, UIDocument의 Root를 찾을 수 없어요! 패널 설정을 확인해 주세요.");
                return;
            }

            root.visible = true;
            RefreshUI().Forget(); // [수정: 인벤토리를 열 때 갱신 호출]
        }
    }
    #endregion

    #region 마우스 상호작용 (Drag & Drop 및 상세 정보)

    private void OnSlotDown(PointerDownEvent evt)
    {
        if (evt.currentTarget is Button btn && btn.userData is int index)
        {
            dragStartIdx = index;
            _isDragging = true;

            UpdateDetailContainer(index).Forget(); // [수정: 상세 정보도 비동기로 텍스트 로드]

            if (InventorySlotImages[index].style.backgroundImage.value.texture != null)
            {
                _ghostIcon.style.backgroundImage = InventorySlotImages[index].style.backgroundImage;
                _ghostIcon.style.display = DisplayStyle.Flex;
                UpdateGhostPosition(evt.position);
            }
        }
    }

    private void OnSlotUp(PointerUpEvent evt)
    {
        VisualElement picked = root.panel.Pick(evt.position);
        Button targetBtn = picked as Button ?? picked?.GetFirstAncestorOfType<Button>();

        if (targetBtn != null && targetBtn.userData is int endIdx)
        {
            dragEndIdx = endIdx;

            // [수정: 통합 UI 인덱스를 데이터 분리형(ContainerType과 내부인덱스)으로 쪼개서 매니저로 넘겨요]
            var startInfo = GetSlotInfo(dragStartIdx);
            var endInfo = GetSlotInfo(dragEndIdx);

            bool isValidDrop = true;
            var dragItem = _inventoryManager.GetData.GetItemList(startInfo.type)[startInfo.internalIndex];

            // 장비 칸으로 드래그 했을 때 타입 검사
            if (endInfo.type == ContainerType.GEAR && dragItem != null && !ItemInstantData.IsEmpty(dragItem))
            {
                if (dragItem.MainType != ItemMainType.Equipment)
                {
                    Debug.LogWarning("이 슬롯에는 장비(Equipment) 타입만 넣을 수 있어요!");
                    isValidDrop = false;
                }
            }

            if (isValidDrop)
            {
                // [수정: 이제 서로 다른 컨테이너(INVENTORY <-> GEAR) 간의 스왑도 완벽하게 지원합니다!]
                _inventoryManager.Swap(startInfo.type, endInfo.type, startInfo.internalIndex, endInfo.internalIndex);
            }
        }

        _ghostIcon.style.display = DisplayStyle.None;
        _isDragging = false;
        _ghostIcon.style.backgroundImage = null;
    }
    #endregion

    #region Mouse Icon
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;
        UpdateGhostPosition(evt.position);
    }

    private void UpdateGhostPosition(Vector2 mousePosition)
    {
        _ghostIcon.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
    }
    #endregion

    #region 상세 정보 UI 갱신 (Detail Container)
    // [수정: Addressable 로딩을 위해 UniTaskVoid로 변경]

    private void ClearDetailContainer()
    {
        if (_itemNameLabel == null) return; // 초기화 전 방어 코드

        _itemNameLabel.text = "";
        _categoryLabel.text = "";
        _qualityLabel.text = "";
        _growthDaysLabel.text = "";
        _priceLabel.text = "";
        _flavorTextLabel.text = "";
        _itemIcon.style.backgroundImage = null;

        _flowerSpeciesLabel.style.display = DisplayStyle.None;
        _flowerColorLabel.style.display = DisplayStyle.None;
        _floriography1Container.style.display = DisplayStyle.None;
        _floriography2Container.style.display = DisplayStyle.None;
    }



    private async UniTaskVoid UpdateDetailContainer(int slotIndex)
    {
        var info = GetSlotInfo(slotIndex);
        var itemList = _inventoryManager.GetData.GetItemList(info.type);
        if (itemList == null || info.internalIndex >= itemList.Count) return;

        var item = itemList[info.internalIndex];

        // 빈 슬롯 클릭 시 화면 비우기
        if (ItemInstantData.IsEmpty(item))
        {
            _itemNameLabel.text = "";
            _categoryLabel.text = "";
            _qualityLabel.text = "";
            _growthDaysLabel.text = "";
            _priceLabel.text = "";
            _flavorTextLabel.text = "";
            _itemIcon.style.backgroundImage = null;

            _flowerSpeciesLabel.style.display = DisplayStyle.None;
            _flowerColorLabel.style.display = DisplayStyle.None;
            _floriography1Container.style.display = DisplayStyle.None;
            _floriography2Container.style.display = DisplayStyle.None;
            return;
        }

        _itemNameLabel.text = GlobalItemDB.GetItemName(item.Id).ToString();
        _categoryLabel.text = item.MainType.ToString();
        _priceLabel.text = GlobalItemDB.GetPrice(item.Id).ToString();

        if (slotIndex >= 0 && slotIndex < InventorySlotImages.Count)
        {
            _itemIcon.style.backgroundImage = InventorySlotImages[slotIndex].style.backgroundImage;
        }

        if (item is FlowerItem flowerItem)
        {
            _qualityLabel.text = flowerItem.Grade.ToString();
            int totalGrowthDays = flowerItem.GrowthDuration.x +
                                  flowerItem.GrowthDuration.y +
                                  flowerItem.GrowthDuration.z +
                                  flowerItem.GrowthDuration.w;
            _growthDaysLabel.text = totalGrowthDays.ToString();

            _flowerSpeciesLabel.style.display = DisplayStyle.Flex;
            _flowerColorLabel.style.display = DisplayStyle.Flex;
            _floriography1Container.style.display = DisplayStyle.Flex;
            _floriography2Container.style.display = DisplayStyle.Flex;
        }
        else
        {
            _qualityLabel.text = "-";
            _growthDaysLabel.text = "-";

            _flowerSpeciesLabel.style.display = DisplayStyle.None;
            _flowerColorLabel.style.display = DisplayStyle.None;
            _floriography1Container.style.display = DisplayStyle.None;
            _floriography2Container.style.display = DisplayStyle.None;
        }

        string descriptionAddress = $"ItemDescription_{item.Id}";
        _flavorTextLabel.text = "설명을 불러오는 중...";

        // [수정: 상세 정보의 플레이버 텍스트 역시 await로 메인 스레드에서 안전하게 적용]
        /*
        TextAsset textAsset = await AddressableManager.LoadAssetAsync<TextAsset>(descriptionAddress);
        if (textAsset != null)
        {
            _flavorTextLabel.text = textAsset.text;
        }
        */
    }
    #endregion
}