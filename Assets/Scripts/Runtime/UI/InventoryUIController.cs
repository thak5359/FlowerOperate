using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;
using static Constant;

public class InventoryUIController : MonoBehaviour
{
    // class for control Inventory UI. Including method for show/hide UI.
    [SerializeField] private UIDocument _uiDocument;

    private List<Button> buttons = new List<Button>();
    VisualElement root;

    private Button closeButton;


    private InventoryManager _inventoryManager;
    private IMapChangable _mapChanger;


    [Inject]
    private void Construct(InventoryManager input_inventorymanager, IMapChangable input_mapChanger)
    {
        _inventoryManager = input_inventorymanager;
        _mapChanger = input_mapChanger;
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

        buttons = root.Query<Button>("slot-button").ToList(); // 리스트에 집어넣기

        //int i = 0;
        //foreach(Button button in buttons)
        //{
        //   button.clicked += -_inventoryManager.isClicked(i++);
        //}




        closeButton = root.Query<Button>("CloseButton");
        closeButton.clicked += closeInventory;

    }

    private async UniTask loadItemDatas()
    {
        ushort i = 0;
        int targetItemID;
        foreach ( ItemObjectData data in _inventoryManager.getSlotList)
        {

            string address;
            targetItemID = _inventoryManager.getSlotList[i].GetItemID;
            if (targetItemID == 0) continue;
            address = GlobalItemDB.GetAddressString((short)targetItemID);
            Texture2D img = await AddressableManager.LoadAssetAsync<Texture2D>(address);
            buttons[i].style.backgroundImage = img;

            i++;
        }
    }

    private void OnDisable()
    {
        buttons.Clear();
    }
    #endregion

    #region Open Inventory
    public void OnOpenInventory(InputAction.CallbackContext callbackContext)
    {
        openInventory();
    }

    public void openInventory()
    {
        if ( _mapChanger.getCurrentIAmap() != INVENTORY_MAP_NAME)
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

}
