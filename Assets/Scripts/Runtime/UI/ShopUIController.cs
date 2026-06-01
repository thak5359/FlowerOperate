using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using VContainer;
using Fungus;
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

    // UI 요소
    private VisualElement _root;
    private ScrollView _scrollView;
    private Button _closeButton;

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
        if (_buyButton != null) _buyButton.clicked -= ExecutePurchase;
        if (_cancelButton != null) _cancelButton.clicked -= CloseMessageBox;

        if (_amountField != null) _amountField.UnregisterValueChangedCallback(evt => OnAmountFieldChanged(evt.newValue));

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
        AddressableManager.ReleaseAllByLabel(_shopImageLabel);

        int playerMoney = _playerItemManager.GetData.GetMoney;

        for (int i = 0; i < _shopItemListSO.GetLength(); i++)
        {
            ProductData product = _shopItemListSO.getProductData(ref i);
            int itemId = product.ProductNo;
            int price = product.Cost;

            VisualElement newSlot = _shopSlotTemplate.Instantiate();

            UnityEngine.UIElements.Label nameLabel = newSlot.Q<UnityEngine.UIElements.Label>("ItemText");
            UnityEngine.UIElements.Label priceLabel = newSlot.Q<UnityEngine.UIElements.Label>("PriceText");
            VisualElement iconElement = newSlot.Q<VisualElement>("Image");

            if (nameLabel != null) nameLabel.text = product.ProductName;
            if (priceLabel != null) priceLabel.text = $"{price:N0} $";

            FixedString128Bytes spriteAddress = GlobalItemDB.GetSpriteAddress(itemId);
            LoadAndSetSpriteAsync(spriteAddress, iconElement).Forget();

            // === [수정됨] 구매 조건 판별 및 확실한 시각적 피드백 ===
            bool canBuy = playerMoney >= price;

            if (!canBuy)
            {
                newSlot.SetEnabled(false);

                // C# 코드에서 강제로 투명도와 회색조(Tint)를 씌워서 시각적으로 확실하게 못 산다는 걸 보여줍니다!
                newSlot.style.opacity = 0.5f;
                if (iconElement != null) iconElement.style.unityBackgroundImageTintColor = Color.gray;
            }
            else
            {
                newSlot.style.opacity = 1f;
                if (iconElement != null) iconElement.style.unityBackgroundImageTintColor = Color.white;

                newSlot.RegisterCallback<ClickEvent>(evt => OpenMessageBox(product));
            }

            _scrollView.Add(newSlot);
        }
    }

    private async UniTaskVoid LoadAndSetSpriteAsync(FixedString128Bytes address, VisualElement targetElement)
    {
        if (address.IsEmpty) return;

        Sprite loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(address);
        if (loadedSprite != null && targetElement != null)
        {
            targetElement.style.backgroundImage = new StyleBackground(loadedSprite);
        }
    }

    #endregion

    #region 구매 및 메세지 박스 로직

    private void ModifyAmount(int offset)
    {
        if (_amountField == null) return;
        int targetAmount = _amountField.value + offset;
        _amountField.value = ClampAmount(targetAmount);
    }

    private void OnAmountFieldChanged(int newValue)
    {
        int validatedAmount = ClampAmount(newValue);

        if (_amountField.value != validatedAmount)
        {
            // SetValueWithoutNotify를 써야 이벤트가 무한 반복(StackOverflow)되는 것을 막을 수 있습니다.
            _amountField.SetValueWithoutNotify(validatedAmount);
        }

        _currentBuyAmount = validatedAmount;
        UpdateMessageBoxUI();
    }

    // [보너스 최적화] 수량은 "아이템 한도"와 "소지금" 두 가지를 모두 고려하여 Clamp 합니다!
    private int ClampAmount(int amount)
    {
        // 1. 해당 아이템의 최대 중첩 개수 제한
        int maxStack = GlobalItemDB.GetStackLimit(_currentSelectedProduct.ProductNo);
        if (maxStack <= 0) maxStack = MAX_COUNT_INVENTORY;

        // 2. 현재 소지금으로 살 수 있는 최대 개수 계산
        int maxAffordable = 9999;
        if (_currentSelectedProduct.Cost > 0)
        {
            maxAffordable = _playerItemManager.GetData.GetMoney / _currentSelectedProduct.Cost;
        }

        // 스택 제한과 소지금 제한 중 더 빡빡한 쪽을 진짜 최대치로 잡습니다.
        int actualMax = Mathf.Min(maxStack, maxAffordable);

        // 아무리 돈이 없어도 입력칸 자체는 최소 1개를 유지시킵니다.
        if (actualMax < 1) actualMax = 1;

        return Mathf.Clamp(amount, 1, actualMax);
    }

    private void OpenMessageBox(ProductData product)
    {
        _currentSelectedProduct = product;
        _currentBuyAmount = 1;

        // 🚨 [핵심 수정 사항] 메세지 박스가 띄워질 때 UI 입력칸의 숫자도 1로 확실하게 초기화해줍니다!
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
        int totalCost = _currentSelectedProduct.Cost * _currentBuyAmount;
        int currentMoney = _playerItemManager.GetData.GetMoney;

        if (currentMoney >= totalCost)
        {
            _playerItemManager.GetData.AddMoney(-totalCost);

            GameItem purchasedItem = _itemManager.CreateItem(_currentSelectedProduct.ProductNo, _currentBuyAmount);
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