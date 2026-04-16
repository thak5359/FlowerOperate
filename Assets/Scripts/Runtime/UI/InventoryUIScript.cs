using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;

public class InventoryUIController : MonoBehaviour
{
    // class for control Inventory UI. Including method for show/hide UI.
    [SerializeField] private UIDocument _uiDocument;

    private List<Button> buttons = new List<Button>();
    VisualElement root;

    private Button closeButton;


    private InventoryManager _inventoryManager;
    private ItemManagerHeavilyModified _itemManager;


    [Inject]
    private void Construct(InventoryManager input_inventorymanager)
    {
        _inventoryManager = input_inventorymanager;
    }

    #region Unity Event
    private void Awake()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        root = _uiDocument.rootVisualElement;


        buttons.Clear();

        buttons = root.Query<Button>("slot-button").ToList(); // 리스트에 집어넣기

        closeButton = root.Query<Button>("CloseButton");
        closeButton.clicked += closeInventory;

    }

    private async UniTask loadItemDatas()
    {
        ushort i = 0;
        foreach( ItemObjectData data in _inventoryManager.getSlotList)
        {

            FixedString128Bytes address;
            _itemManager.GetAddressBurst((short)_inventoryManager.getSlotList[i].GetItemID, out address);
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
        root.visible = true;
    }

    #endregion

    #region Close Inventory
    public void OnEscape(InputAction.CallbackContext callbackContext)
    {
        closeInventory();
    }
    public void closeInventory()
    {
        root.visible = false;
    }
    #endregion

}
