using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;
using Fungus;
using R3;
using Unity.Collections;
using static Constant;

public class ShopUIController : MonoBehaviour
{
    [Header("Shop Data")]
    [SerializeField] private ShopItemListSO _shopItemListSO;
    [SerializeField] private VisualTreeAsset _shopSlotTemplate;

    [Header("UI Document Handle")]
    [SerializeField] private UIDocument _uiDocument;

    [Header("Dependencies")]
    private PlayerOwnItemDataManager _playerItemManager;
    private ItemManager _itemManager;
    private IMapChangable _mapChanger;
    // 인벤토리 가득 여부 플래그
    private bool _isInventoryFull = false;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    // UI 요소
    private VisualElement _root;
    private ScrollView _scrollView;
    private Button _closeButton;

    // 상세 보기 UI 요소
    private UnityEngine.UIElements.Label _itemNameLabel;
    private VisualElement _itemIcon;
    private UnityEngine.UIElements.Label _categoryLabel;
    private UnityEngine.UIElements.Label _qualityLabel;
    private UnityEngine.UIElements.Label _growthDaysLabel;
    private UnityEngine.UIElements.Label _priceLabel;
    private UnityEngine.UIElements.Label _flowerSpeciesLabel;
    private UnityEngine.UIElements.Label _flowerColorLabel;
    private VisualElement _floriography1Container;
    private VisualElement _floriography2Container;
    private UnityEngine.UIElements.Label _flavorTextLabel;
    private Button _mainBuyButton;

    // 메세지 박스 관련 UI
    private VisualElement _messageBox;
    private UnityEngine.UIElements.Label _messagePriceLabel;
    private Button _buyButton;
    private Button _cancelButton;

    // 현재 상점 상태 관리
    private ProductData _currentSelectedProduct;
    private int _currentBuyAmount = 1;

    private IntegerField _amountField;
    private Button _amount1UpBtn;
    private Button _amount1DownBtn;
    private Button _amount10UpBtn;
    private Button _amount10DownBtn;

    // 현재 선택된 슬롯 정보
    private VisualElement _selectedSlotElement;
    private int _selectedIndex = 0;

    private readonly FixedString128Bytes _shopImageLabel = new FixedString128Bytes("ShopUI_Images");

    [Inject]
    public void Construct(PlayerOwnItemDataManager playerItemManager, ItemManager itemManager, IMapChangable mapChanger)
    {
        _playerItemManager = playerItemManager;
        _itemManager = itemManager;
        _mapChanger = mapChanger;
    }

    private void Awake()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument == null)
            Debug.LogError("[ShopUIController] UIDocument가 할당되지 않았습니다!");
    }

    private void OnEnable()
    {
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;

        if (_root != null)
        {
            _root.style.display = DisplayStyle.None;
        }

        // === [1. 메인 상점 UI 요소 캐싱 및 연결] ===
        _scrollView = _root.Q<ScrollView>("ScrollView");
        _closeButton = _root.Q<Button>("CloseButton");
        if (_closeButton != null) _closeButton.clicked += CloseShop;

        //// === [인벤토리 가득 이벤트 구독] ===
        //_disposables.Add(
        //        GlobalEventManager.InventoryFullObservable
        //            .Subscribe(isFull =>
        //            {
        //                _isInventoryFull = isFull;
        //                // 현재 선택된 제품에 대해 버튼 상태를 즉시 적용
        //                UpdateBuyButtonState();
        //            }));

        // === [2. 메세지 박스 UI 요소 캐싱 및 연결] ===
        _messageBox = _root.Q<VisualElement>("ItemShopMessageBox");
        _messagePriceLabel = _messageBox?.Q<UnityEngine.UIElements.Label>("PriceLabel");
        _buyButton = _messageBox?.Q<Button>("BuyButton");
        _cancelButton = _messageBox?.Q<Button>("CancelButton");

        // === [상세 보기 UI 요소 캐싱 및 연결] ===
        _itemNameLabel = _root.Q<UnityEngine.UIElements.Label>("ItemNameLabel");
        _itemIcon = _root.Q<VisualElement>("IteItemIcon"); // UXML의 오타 반영
        _categoryLabel = _root.Q<UnityEngine.UIElements.Label>("CategoryLabel");
        _qualityLabel = _root.Q<UnityEngine.UIElements.Label>("QualityLabel");
        _growthDaysLabel = _root.Q<UnityEngine.UIElements.Label>("GrowthDaysLabel");
        _priceLabel = _root.Q<UnityEngine.UIElements.Label>("PriceLabel");
        _flowerSpeciesLabel = _root.Q<UnityEngine.UIElements.Label>("FlowerSpeciesLabel");
        _flowerColorLabel = _root.Q<UnityEngine.UIElements.Label>("FlowerColorLabel");
        _floriography1Container = _root.Q<VisualElement>("Floriography1Container");
        _floriography2Container = _root.Q<VisualElement>("Floriography2Container");
        _flavorTextLabel = _root.Q<UnityEngine.UIElements.Label>("FlavorTextLabel");

        // DescriptionView 하위의 메인 구입 버튼 캐싱 및 연결
        _mainBuyButton = _root.Q<VisualElement>("DescriptionView")?.Q<Button>("BuyButton");
        if (_mainBuyButton != null) _mainBuyButton.clicked += OnMainBuyButtonClicked;

        // === [2. 메세지 박스 UI 요소 캐싱 및 연결] ===
        _messageBox = _root.Q<VisualElement>("ItemShopMessageBox");
        _messagePriceLabel = _messageBox?.Q<UnityEngine.UIElements.Label>("PriceLabel");
        _buyButton = _messageBox?.Q<Button>("BuyButton");
        _cancelButton = _messageBox?.Q<Button>("CancelButton");

        if (_messageBox != null) _messageBox.style.display = DisplayStyle.None;
        if (_buyButton != null) _buyButton.clicked += ExecutePurchase;
        if (_cancelButton != null) _cancelButton.clicked += CloseMessageBox;

        // === [3. 수량 조절 필드 및 버튼 연결] ===
        _amountField = _messageBox?.Q<IntegerField>("AmountField");
        _amount1UpBtn = _messageBox?.Q<Button>("Amount1UpButton");
        _amount1DownBtn = _messageBox?.Q<Button>("Amount1DownButton");
        _amount10UpBtn = _messageBox?.Q<Button>("Amount10UpButton");
        _amount10DownBtn = _messageBox?.Q<Button>("Amount10DownButton");

        if (_amount1UpBtn != null) _amount1UpBtn.clicked += () => ModifyAmount(1);
        if (_amount1DownBtn != null) _amount1DownBtn.clicked += () => ModifyAmount(-1);
        if (_amount10UpBtn != null) _amount10UpBtn.clicked += () => ModifyAmount(10);
        if (_amount10DownBtn != null) _amount10DownBtn.clicked += () => ModifyAmount(-10);

        // 유저가 키보드로 직접 숫자를 입력할 때 발동하는 이벤트
        if (_amountField != null)
        {
            _amountField.RegisterValueChangedCallback(evt => OnAmountFieldChanged(evt.newValue));
        }

        // === [4. Fungus 전역 이벤트 구독] ===
        Fungus.FungusEventBridge.OnFungusMessageBroadcasted += HandleFungusBroadcast;
    }

    private void OnDisable()
    {
        if (_closeButton != null) _closeButton.clicked -= CloseShop;
        if (_mainBuyButton != null) _mainBuyButton.clicked -= OnMainBuyButtonClicked;
        if (_buyButton != null) _buyButton.clicked -= ExecutePurchase;
        if (_cancelButton != null) _cancelButton.clicked -= CloseMessageBox;

        if (_amountField != null) _amountField.UnregisterValueChangedCallback(evt => OnAmountFieldChanged(evt.newValue));

        // UI 피드백 필요 시 여기서 구현 (예: 인벤토리 가득 토스트) // 현재는 ShopUIController에서 처리 구독 해제
        _disposables.Dispose();

        Fungus.FungusEventBridge.OnFungusMessageBroadcasted -= HandleFungusBroadcast;
        AddressableManager.ReleaseAllByLabel(_shopImageLabel);
    }

    #region Fungus 통신 및 UI 열기/닫기

    private void HandleFungusBroadcast(string messageKey)
    {
        if (messageKey == FungusBroadcastType.OpenShop.ToString())
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        if (!GlobalItemDB.IsInitialized) return;

        if (_mapChanger.getCurrentIAmap() != SHOP_MAP_NAME)
        {
            _mapChanger.changeIAmapShop();
            if (_root == null) return;

            _root.style.display = DisplayStyle.Flex;
            if (_messageBox != null) _messageBox.style.display = DisplayStyle.None;

            _selectedIndex = 0; // 상점이 켜질 때 기본 0번째 선택 초기화
            RefreshShopSlots();
        }
    }

    private void CloseShop()
    {
        _root.style.display = DisplayStyle.None;
        AddressableManager.ReleaseAllByLabel(_shopImageLabel);

        if (_mapChanger.getCurrentIAmap() == SHOP_MAP_NAME)
        {
            _mapChanger.changeIAmapPrev();
        }
    }

    #endregion

    #region 상점 슬롯 생성 및 데이터 바인딩

    private void RefreshShopSlots()
    {
        _scrollView.Clear();
        _selectedSlotElement = null; // 슬롯 목록이 파괴되었으므로 하이라이트 참조 초기화

        int playerMoney = _playerItemManager.GetData.GetMoney;
        ProductData selectedProduct = default;
        VisualElement selectedSlot = null;

        for (int i = 0; i < _shopItemListSO.GetLength(); i++)
        {
            ProductData product = _shopItemListSO.getProductData(ref i);
            int itemId = product.ProductNo;
            int price = product.Cost;

            VisualElement newSlot = _shopSlotTemplate.Instantiate();

            UnityEngine.UIElements.Label nameLabel = newSlot.Q<UnityEngine.UIElements.Label>("ItemText");
            UnityEngine.UIElements.Label priceLabel = newSlot.Q<UnityEngine.UIElements.Label>("PriceText");
            VisualElement iconElement = newSlot.Q<VisualElement>("Image");

            string itemName = GlobalItemDB.GetItemName(itemId).ToString();
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = product.ProductName;
            }
            if (nameLabel != null) nameLabel.text = itemName;
            if (priceLabel != null) priceLabel.text = $"{price:N0} $";

            FixedString128Bytes spriteAddress = GlobalItemDB.GetSpriteAddress(itemId);
            _scrollView.Add(newSlot); // Add to scroll view first so targetElement.panel is active during synchronous load completion
            LoadAndSetSpriteAsync(spriteAddress, iconElement).Forget();

            bool canBuy = playerMoney >= price;

            // 소지금 부족 시에도 클릭은 가능해야 상세정보 확인이 되므로 SetEnabled(false)는 생략하고 시각적 투명도 피드백만 제공
            if (!canBuy)
            {
                newSlot.style.opacity = 0.5f;
                if (iconElement != null) iconElement.style.unityBackgroundImageTintColor = Color.gray;
            }
            else
            {
                newSlot.style.opacity = 1f;
                if (iconElement != null) iconElement.style.unityBackgroundImageTintColor = Color.white;
            }

            int index = i; // local copy
            newSlot.RegisterCallback<ClickEvent>(evt => SelectProduct(product, newSlot, index));

            if (i == _selectedIndex)
            {
                selectedProduct = product;
                selectedSlot = newSlot;
            }
        }

        // 초기 또는 이전 인덱스 자동 선택 복원
        if (selectedSlot != null)
        {
            SelectProduct(selectedProduct, selectedSlot, _selectedIndex);
        }
    }

    private async UniTaskVoid LoadAndSetSpriteAsync(FixedString128Bytes address, VisualElement targetElement)
    {
        if (address.IsEmpty) return;

        Sprite loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(address, _shopImageLabel);
        if (loadedSprite != null && targetElement != null)
        {
            targetElement.style.backgroundImage = new StyleBackground(loadedSprite);
        }
    }

    #endregion

    #region 상세 보기 & 슬롯 선택 로직

    private void OnMainBuyButtonClicked()
    {
        if (_currentSelectedProduct.ProductNo > 0)
        {
            OpenMessageBox(_currentSelectedProduct);
        }
    }

    private void SelectProduct(ProductData product, VisualElement slotElement, int index)
    {
        _currentSelectedProduct = product;
        _selectedIndex = index;

        // 이전 선택 슬롯 하이라이트 원상복구
        if (_selectedSlotElement != null && _selectedSlotElement.panel != null)
        {
            _selectedSlotElement.style.unityBackgroundImageTintColor = Color.white;
        }

        // 현재 선택된 슬롯에 하이라이트 틴트(연한 파란색 계열) 적용
        if (slotElement != null && slotElement.childCount > 0)
        {
            VisualElement rootSlotElement = slotElement.ElementAt(0);
            if (rootSlotElement != null)
            {
                rootSlotElement.style.unityBackgroundImageTintColor = new Color(0.85f, 0.95f, 1f, 1f);
                _selectedSlotElement = rootSlotElement;
            }
        }

        UpdateProductDetail(product);
    }

    private void UpdateProductDetail(ProductData product)
    {
        int itemId = product.ProductNo;
        ItemSubType subType = GlobalItemDB.GetSubType(itemId);
        bool isSeed = subType == ItemSubType.Seed;

        // 이름
        string itemName = GlobalItemDB.GetItemName(itemId).ToString();
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = product.ProductName;
        }
        if (_itemNameLabel != null) _itemNameLabel.text = itemName;

        // 카테고리
        if (_categoryLabel != null)
        {
            _categoryLabel.text = GlobalItemDB.GetMainType(itemId).ToString();
        }

        // 품질 (씨앗은 기본 등급인 Lv0으로 고정 표기, 그 외 일반 마스터 아이템은 대시 처리)
        if (_qualityLabel != null)
        {
            _qualityLabel.text = isSeed ? FlowerGrade.Lv0.ToString() : "-";
        }

        // 가격
        if (_priceLabel != null)
        {
            _priceLabel.text = $"{product.Cost:N0} $";
        }

        // 설명(플레이버 텍스트)
        if (_flavorTextLabel != null)
        {
            _flavorTextLabel.text = GlobalItemDB.GetDescription(itemId).ToString();
        }

        // 상세 아이콘 이미지 설정
        if (_itemIcon != null)
        {
            FixedString128Bytes spriteAddress = GlobalItemDB.GetSpriteAddress(itemId);
            LoadAndSetSpriteAsync(spriteAddress, _itemIcon).Forget();
        }

        // 꽃 또는 씨앗 속성 유무에 따른 동적 레이아웃 처리
        bool hasFlower = GlobalItemDB.HasFlower(itemId);
        int flowerLookupId = itemId;

        if (isSeed)
        {
            flowerLookupId = itemId + 1000;
            hasFlower = GlobalItemDB.HasFlower(flowerLookupId);
        }

        if (hasFlower)
        {
            ref FlowerItemBlobData flowerRef = ref GlobalItemDB.GetFlowerRef(flowerLookupId);

            // 성장 기간 계산 (int4 합산)
            int totalGrowthDays = flowerRef.GrowthDuration.x +
                                  flowerRef.GrowthDuration.y +
                                  flowerRef.GrowthDuration.z +
                                  flowerRef.GrowthDuration.w;

            if (_growthDaysLabel != null) _growthDaysLabel.text = totalGrowthDays.ToString();

            if (_flowerSpeciesLabel != null)
            {
                _flowerSpeciesLabel.text = flowerRef.Species.ToString();
                _flowerSpeciesLabel.style.display = DisplayStyle.Flex;
            }

            if (_flowerColorLabel != null)
            {
                _flowerColorLabel.text = flowerRef.Color.ToString();
                _flowerColorLabel.style.display = DisplayStyle.Flex;
            }

            if (_floriography1Container != null) _floriography1Container.style.display = DisplayStyle.Flex;
            if (_floriography2Container != null) _floriography2Container.style.display = DisplayStyle.Flex;
        }
        else
        {
            if (_growthDaysLabel != null) _growthDaysLabel.text = "-";

            if (_flowerSpeciesLabel != null) _flowerSpeciesLabel.style.display = DisplayStyle.None;
            if (_flowerColorLabel != null) _flowerColorLabel.style.display = DisplayStyle.None;
            if (_floriography1Container != null) _floriography1Container.style.display = DisplayStyle.None;
            if (_floriography2Container != null) _floriography2Container.style.display = DisplayStyle.None;
        }

        // 소지금 비교하여 메인 구매 버튼 활성화 여부 지정
        int playerMoney = _playerItemManager.GetData.GetMoney;
        // 인벤토리 가득 여부와 금액을 모두 고려해 구매 버튼 활성화 결정
        bool canBuy = playerMoney >= product.Cost && !_isInventoryFull;
        if (_mainBuyButton != null)
        {
            _mainBuyButton.SetEnabled(canBuy);
        }
    }

    #endregion

    #region 구매 및 메세지 박스 로직

    private void ModifyAmount(int offset)
    {
        // 여기에 만약 
        if (_amountField == null) return;

        if (_amountField.value == 1 && offset == 10)
        {
            _amountField.value = offset;
            return;
        }

        int targetAmount = _amountField.value + offset;
        _amountField.value = ClampAmount(targetAmount);
    }

    private void OnAmountFieldChanged(int newValue)
    {
        int validatedAmount = ClampAmount(newValue);

        if (_amountField.value != validatedAmount)
        {
            _amountField.SetValueWithoutNotify(validatedAmount);
        }

        _currentBuyAmount = validatedAmount;
        UpdateMessageBoxUI();
    }

    private int ClampAmount(int amount)
    {
        int maxStack = GlobalItemDB.GetStackLimit(_currentSelectedProduct.ProductNo);
        if (maxStack <= 0) maxStack = MAX_COUNT_INVENTORY;

        int maxAffordable = 9999;
        if (_currentSelectedProduct.Cost > 0)
        {
            maxAffordable = _playerItemManager.GetData.GetMoney / _currentSelectedProduct.Cost;
        }

        int actualMax = Mathf.Min(maxStack, maxAffordable);
        if (actualMax < 1) actualMax = 1;

        return Mathf.Clamp(amount, 1, actualMax);
    }

    private void OpenMessageBox(ProductData product)
    {
        _currentSelectedProduct = product;
        _currentBuyAmount = 1;

        if (_amountField != null)
        {
            _amountField.SetValueWithoutNotify(1);
        }

        UpdateMessageBoxUI();
        if (_messageBox != null) _messageBox.style.display = DisplayStyle.Flex;
    }

    private void CloseMessageBox()
    {
        if (_messageBox != null) _messageBox.style.display = DisplayStyle.None;
    }

    private void UpdateMessageBoxUI()
    {
        int totalCost = _currentSelectedProduct.Cost * _currentBuyAmount;
        if (_messagePriceLabel != null) _messagePriceLabel.text = $"{totalCost:N0} $";
    }

    private void ExecutePurchase()
    {
        // 수정 위치: UI 콜백 경계에서 구매 비동기 흐름을 시작해요.
        ExecutePurchaseAsync().Forget();
    }

    // 수정 위치: 아이템 로드가 끝난 뒤에만 인벤토리에 추가해요.
    private async UniTask ExecutePurchaseAsync()
    {
        int totalCost = _currentSelectedProduct.Cost * _currentBuyAmount;
        int currentMoney = _playerItemManager.GetData.GetMoney;

        if (currentMoney >= totalCost)
        {
            GameItem purchasedItem = await _itemManager.CreateItemAsync(_currentSelectedProduct.ProductNo, _currentBuyAmount);
            if (purchasedItem == null)
                return;

            // 수정 위치: 아이템 생성이 성공한 뒤에만 구매 금액을 차감해요.
            _playerItemManager.GetData.AddMoney(-totalCost);
            _playerItemManager.AddItem(ContainerType.INVENTORY, purchasedItem);

            Debug.Log($"[Shop] {_currentSelectedProduct.ProductName} {_currentBuyAmount}개 구매 완료. (잔여 금액: {_playerItemManager.GetData.GetMoney} $)");

            CloseMessageBox();
            RefreshShopSlots();
        }
        else
        {
            Debug.LogWarning("[Shop] 돈이 부족합니다!");
        }
    }
    #endregion
}
