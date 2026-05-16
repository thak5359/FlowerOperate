using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;
using static Constant;

public class InventoryUIController : MonoBehaviour
{
    const ContainerType type = ContainerType.INVENTORY;

    [SerializeField] private UIDocument _uiDocument;

    private List<Button> buttons = new();
    private List<VisualElement> images = new();
    VisualElement root;
    VisualElement _ghostIcon;

    private Button closeButton;

    private IMapChangable _mapChanger;
    private PlayerOwnItemDataManager _inventoryManager;



    private int dragStartIdx;
    private int dragEndIdx;

    private bool _isDragging = false;


    [Inject]
    private void Construct(IMapChangable input_mapChanger, PlayerOwnItemDataManager input_inventoryManager)
    {
        _mapChanger = input_mapChanger;
        _inventoryManager = input_inventoryManager;
    }

    #region Unity Event
    private void Awake()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
            Debug.Log("<color=red>GetComponent on Awake is failed</color>");
        else
            Debug.Log("<color=green>GetComponent on Awake is success</color>");
    }

    private void Start()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
            Debug.Log("<color=red>GetComponent on Start is failed</color>");
        Debug.Log("<color=green>GetComponent on Start is success</color>");
    }

    private void OnEnable()
    {
        root = _uiDocument.rootVisualElement;

        root.visible = false;

        buttons.Clear();
        images.Clear();


        buttons = root.Query<Button>("SlotButton").ToList();
        closeButton = root.Query<Button>("CloseButton");
        _ghostIcon = root.Q<VisualElement>("GhostIcon");



        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].userData = i; // 버튼에 인덱스 저장


            buttons[i].RegisterCallback<PointerDownEvent>(OnSlotDown, TrickleDown.TrickleDown);
            buttons[i].RegisterCallback<PointerUpEvent>(OnSlotUp, TrickleDown.TrickleDown);
        }


        closeButton.clicked += closeInventory;


        images = root.Query<VisualElement>("SlotImage").ToList();





        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);

    }

    private void OnDisable()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int ClosureFixer = i;
            buttons[i].UnregisterCallback<PointerDownEvent>(OnSlotDown, TrickleDown.TrickleDown);
            buttons[i].UnregisterCallback<PointerUpEvent>(OnSlotUp, TrickleDown.TrickleDown);
        }
        buttons.Clear();

        _ghostIcon.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
    }

    //private void RefreshUI()
    //{
    //    _inventoryManager.GetData.GetList(type).ForEach((itemData, idx) =>
    //    {
    //        if (itemData.GetItemID == 0)
    //        {
    //            buttons[idx].style.backgroundImage = null;
    //            buttons[idx].text = "";
    //        }
    //        else
    //        {
    //            string address = GlobalItemDB.GetAddressString((short)itemData.GetItemID);
    //            AddressableManager.LoadAssetAsync<Texture2D>(address).ContinueWith(texture =>
    //            {
    //                buttons[idx].style.backgroundImage = texture;
    //                buttons[idx].text = itemData.GetAmount.ToString();
    //            });
    //        }
    //    });
    //}


    #endregion

    #region Open Inventory
    public void OnOpenInventory(InputAction.CallbackContext callbackContext)
    {
        openInventory();
    }

    #endregion

    #region Close Inventory
    public void OnEscape(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("OnEscape has been called");
        closeInventory();
    }
    public void closeInventory()
    {
        Debug.Log("closeInventory Has been Called");

        if (_mapChanger.getCurrentIAmap() == INVENTORY_MAP_NAME)
        {
            root.visible = false;
            _mapChanger.changeIAmapPrev();
        }
    }
    #endregion

    #region 마우스 드래그 앤 드롭
    // 드래그 시작 (눌린 버튼의 번호를 그대로 가져옴)
    private void OnSlotDown(PointerDownEvent evt)
    {
        if (evt.currentTarget is Button btn && btn.userData is int index)
        {
            dragStartIdx = index;
            _isDragging = true;

            _ghostIcon.style.backgroundImage = images[index].style.backgroundImage;

            _ghostIcon.style.display = DisplayStyle.Flex;
            UpdateGhostPosition(evt.position);
        }
    }

    // 드래그 종료 (놓은 위치의 버튼을 '조사'해서 가져옴)
    private void OnSlotUp(PointerUpEvent evt)
    {

        // 현재 마우스 위치 아래에 있는 요소를 픽업
        VisualElement picked = root.panel.Pick(evt.position);
        Button targetBtn = picked as Button ?? picked?.GetFirstAncestorOfType<Button>();

        if (targetBtn != null && int.Parse(targetBtn.text) is int endIdx)
        {
            dragEndIdx = endIdx;
            Debug.Log($"종료: {dragEndIdx}");
            images[dragStartIdx].style.backgroundImage = images[dragEndIdx].style.backgroundImage;
            images[dragEndIdx].style.backgroundImage = _ghostIcon.style.backgroundImage;
            _ghostIcon.style.backgroundImage = default;
            _inventoryManager.Swap(type, type, dragStartIdx, dragEndIdx);
        }
        else
        {
            Debug.Log("유효한 버튼이 아닙니다. 드래그가 취소됩니다.");
            (images[dragStartIdx].style.backgroundImage, _ghostIcon.style.backgroundImage) = (_ghostIcon.style.backgroundImage, images[dragStartIdx].style.backgroundImage);
        }

        _ghostIcon.style.display = DisplayStyle.None;
        _isDragging = false;

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
        }
    }
    #endregion



    #region Mouse Icon

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        // 드래그 중이라면 마우스 좌표에 맞춰 아이콘 이동
        UpdateGhostPosition(evt.position);
    }

    private void UpdateGhostPosition(Vector2 mousePosition)
    {

        _ghostIcon.transform.position = new Vector3(mousePosition.x, mousePosition.y , 0);
    }

    #endregion
}
