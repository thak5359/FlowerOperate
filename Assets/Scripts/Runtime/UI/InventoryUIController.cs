using AYellowpaper.SerializedCollections.Editor;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

    private IMapChangable _mapChanger;
    private ItemStorageParent _inventoryManager;

    private int dragStartIdx;
    private int dragEndIdx;

    const ContainerType type = ContainerType.INVENTORY;

    [Inject]
    private void Construct(IMapChangable input_mapChanger, ItemStorageParent input_inventoryManager)
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

        buttons = root.Query<Button>("SlotButton").ToList(); // 리스트에 집어넣기

        for (int i =0; i < buttons.Count; i++)
        {
            int ClosureFixer = i;
            buttons[ClosureFixer].text = ClosureFixer.ToString();

            buttons[ClosureFixer].RegisterCallback<PointerDownEvent>(evt =>
            {
                OnSlotDown(evt);
            },TrickleDown.TrickleDown);

            buttons[ClosureFixer].RegisterCallback<PointerUpEvent>(evt =>
            {
                OnSlotUp(evt);
                _inventoryManager.Swap(type, type, dragStartIdx, dragEndIdx);
            }, TrickleDown.TrickleDown);
        }

        Debug.Log($"On buttons[39], Text is {buttons[39].text}");

        closeButton = root.Query<Button>("CloseButton");
        closeButton.clicked += closeInventory;
    }

    private void OnDisable()
    {
        for (int i =0; i < buttons.Count; i++)
        {
            int ClosureFixer = i;
            buttons[ClosureFixer].UnregisterCallback<PointerDownEvent>(evt =>
            {
                OnSlotDown(evt);
            });
            buttons[ClosureFixer].UnregisterCallback<PointerUpEvent>(evt =>
            {
                OnSlotUp(evt);
                _inventoryManager.Swap(type, type, dragStartIdx, dragEndIdx);
            });
        }
        buttons.Clear();
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


    //private async UniTask loadItemDatas()
    //{
    //    ushort i = 0;
    //    int targetItemID;
    //    foreach ( ItemObjectData data in _inventoryManager.getSlotList)
    //    {

    //        string address;
    //        targetItemID = _inventoryManager.getSlotList[i].GetItemID;
    //        if (targetItemID == 0) continue;
    //        address = GlobalItemDB.GetAddressString((short)targetItemID);
    //        Texture2D img = await AddressableManager.LoadAssetAsync<Texture2D>(address);
    //        buttons[i].style.backgroundImage = img;

    //        i++;
    //    }
    //}

    #endregion

    #region Open Inventory
    public void OnOpenInventory(InputAction.CallbackContext callbackContext)
    {
        openInventory();
    }

    // 드래그 시작 (눌린 버튼의 번호를 그대로 가져옴)
    private void OnSlotDown(PointerDownEvent evt)
    {
        if (evt.currentTarget is Button btn)
        {
            dragStartIdx = int.Parse(btn.text);
            Debug.Log($"시작: {dragStartIdx}");
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
            _inventoryManager.Swap(type, type, dragStartIdx, dragEndIdx);
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
