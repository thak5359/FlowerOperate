using Fungus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static IAmapManager;


public interface IMapChangable // 컨트롤 방법을 변경하는 기능은 이 인터페이스를 포함.
{
    string getCurrentIAmap();
    void changeIAmap(string targetMap);
    void changeIAmapTitle();
    void changeIAmapSetting();
    void changeIAmapInventory();
    void changeIAmapStorage();
    void changeIAmapPauseMenu();
    void changeIAmapShop();
    void changeIAmapFarm();
    void changeIAmapChatBox();
    void changeIAmapPrev();
}


public class IAmapManager : MonoBehaviour, IMapChangable
{
    private static IAmapManager instance;

    [Header("InputAction Asset 파일을 할당해주세요. 위치는 Asset/Settings입니다")]
    [SerializeField] public InputActionAsset IA;

    [Header("Wasd키로 조작하신다면 체크해주세요!")]
    [SerializeField] public bool isWASDKeySetting = true;

    private PauseMenu pauseMenu;
    private PlayerInput playerInput;
    private PlayerController playerController;
    private HotbarManager hotbarManager;

    private Stack<string> prevMapStack = new Stack<string>();

    public event Action OnMapChange;

    const string TITLE_MAP_NAME = "MAP_TITLE";
    const string SETTING_MAP_NAME = "MAP_SETTING";
    const string PAUSEMENU_MAP_NAME = "MAP_PAUSE";
    const string SHOP_MAP_NAME = "MAP_SHOP";
    const string FARM_MAP_NAME = "MAP_FARM";
    const string INVENTORY_MAP_NAME = "MAP_INVENTORY";
    const string STORAGE_MAP_NAME = "MAP_STORAGE";
    const string CHATBOX_MAP_NAME = "MAP_CHATBOX";
    const string WASD_SCHEME_NAME = "WASD_Scheme";
    const string ARROW_SCHEME_NAME = "Arrow_Scheme";

    void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
            playerInput.actions = IA;
    }

    #region 싱글톤 패턴
    private void Start()
    {
        playerController = PlayerController.Instance();
        pauseMenu = PauseMenu.Instance();
        hotbarManager = HotbarManager.Instance();


        if (playerController == null)
        {
            Debug.LogError("[IA Manager] 파트너! PlayerController 인스턴스를 찾을 수 없어!");
            return;
        }

        changeKeySetting(isWASDKeySetting);
        FarmMapActionAllocator();
    }
    public static IAmapManager Instance()
    {
        if (instance != null)
            return instance;
        else return null;
    }

    #endregion

    #region wasd, 화살표 조작법 선택/변경

    public void changeKeySetting(bool isWASD)
    {
        isWASDKeySetting = isWASD;
        string targetScheme = isWASD ? WASD_SCHEME_NAME : ARROW_SCHEME_NAME;

        playerInput.actions.bindingMask = InputBinding.MaskByGroup(targetScheme);

        Debug.Log($"[IA Manager] {targetScheme}이 활성화되었습니다, 파트너!");
    }
    #endregion

    #region IA 맵 변경

    private void PushAndChange(string targetMap)
    {
        if (playerInput == null) return;

        string currentMap = playerInput.currentActionMap.name;

        // 똑같은 맵을 또 스택에 넣는 것을 방지 
        if (currentMap != targetMap)
        {
            prevMapStack.Push(currentMap);
            Debug.Log($"[IA Manager] Push: {currentMap} / 현재 스택 크기: {prevMapStack?.Count??null}");
        }

        playerInput.SwitchCurrentActionMap(targetMap);
        
        OnMapChange?.Invoke();
    }

    string IMapChangable.getCurrentIAmap()
    {
        return (playerInput?.currentActionMap?.name ?? "nothing");
    }

    // 인터페이스 구현들
    void IMapChangable.changeIAmapPauseMenu() => PushAndChange(PAUSEMENU_MAP_NAME);
    void IMapChangable.changeIAmapSetting() => PushAndChange(SETTING_MAP_NAME);
    void IMapChangable.changeIAmapTitle() => PushAndChange(TITLE_MAP_NAME);
    void IMapChangable.changeIAmapInventory() => PushAndChange(INVENTORY_MAP_NAME);
    void IMapChangable.changeIAmapStorage() => PushAndChange(STORAGE_MAP_NAME);
    void IMapChangable.changeIAmapShop() => PushAndChange(SHOP_MAP_NAME);
    void IMapChangable.changeIAmapFarm() => PushAndChange(FARM_MAP_NAME);
    void IMapChangable.changeIAmapChatBox() => PushAndChange(CHATBOX_MAP_NAME);
    void IMapChangable.changeIAmapPrev()
    {
        if (playerInput != null && prevMapStack.Count > 0)
        {
            string target = prevMapStack.Pop();
            playerInput.SwitchCurrentActionMap(target);
            Debug.Log($"[IA Manager] Pop: {target} / 남은 스택 크기: {prevMapStack?.Count??0}");

            OnMapChange?.Invoke();
        }
    }
    void IMapChangable.changeIAmap(string targetMap) // 직접 키고 싶은 맵 요청
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(targetMap);
            OnMapChange?.Invoke();
        }
    }
    #endregion

    #region  맵 별 액션에 함수 할당

    // 타이틀 키 할당
    void TitleMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }

    #region 정지 메뉴 키 할당
    void PauseMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(PAUSEMENU_MAP_NAME); // 현재 맵에서 사용중인 IA에서 특정 맵을 가져옴
        var actionEscape = map.FindAction("Escape"); // 거기서 어떤키를 눌렀을때에 동작을 할당하는 곳을 찾음
        actionEscape.performed += pauseMenu.OnBackAction; // 함수 할당
    }
    #endregion

    #region 세팅 메뉴 키 할당
    void SettingMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(SETTING_MAP_NAME);
        var actionEscape = map.FindAction("Escape");
    }
    #endregion

    #region 농장 조작 키 할당
    void FarmMapActionAllocator()
    {
        if (playerInput == null || playerInput.actions == null) return;

        InputActionMap map = playerInput.actions.FindActionMap(FARM_MAP_NAME);
        if (map == null) return;

        map.Disable();

        // 2. 액션을 찾습니다. (직접 경로를 쓰는 방식이 더 정확할 때가 있습니다)
        InputAction actionMove = map.FindAction("Move");

        if (actionMove != null)
        {
            // 3. 중복 구독 방지를 위해 -= 후 += (매우 권장)
            actionMove.performed -= playerController.OnMove;
            actionMove.canceled -= playerController.OnMove;

            actionMove.performed += playerController.OnMove;
            actionMove.canceled += playerController.OnMove;
        }

        InputAction actionUse = map.FindAction("Use");
        actionUse.started += playerController.OnUse;
        actionUse.performed += playerController.OnUse;
        actionUse.canceled += playerController.OnUse;

        InputAction actionInteract = map.FindAction("Interact");
        actionInteract.started += playerController.OnInteract;
        actionInteract.performed += playerController.OnInteract;
        actionInteract.canceled += playerController.OnInteract;

        #region  핫 슬롯 좌우 이동
        InputAction actionPrevHotSlot = map.FindAction("PrevHotSlot");
        actionPrevHotSlot.performed += hotbarManager.OnPrevHotSlot;

        InputAction actionNextHotSlot = map.FindAction("NextHotSlot");
        actionNextHotSlot.performed += hotbarManager.OnNextHotSlot;

        InputAction actionEscape = map.FindAction("Escape");
        actionEscape.performed += pauseMenu.OnBackAction;
        #endregion

        # region 핫슬롯 1~0번 바로 이동
        
        for (int i = 0; i < 10; i++)
        {
            int slotIndex = i;
            // 1~9 , 0번 슬롯 할당
            int displayNum = (i == 9) ? 0 : i + 1;
            string actionName = $"HotSlot{displayNum}";

            InputAction action = map.FindAction(actionName);
            if(action != null)
            {
                //ctx로 InputAction.callbackContext를 처리
                action.performed += ctx => hotbarManager.pointSlot(slotIndex);
            }
        }
        #endregion

        var actionOpenInventory = map.FindAction("OpenInventory");
        // actionOpenInventory.performed +=   //TODO : 인벤토리 UI를 여는 함수 할당하기

        map.Enable();

    }
    #endregion

    #region 인벤토리 조작 키 할당
    void InventoryMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion

    #region 가게 경영 조작 키 할당
    void ShopMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion


    #region 대화창 조작 키 할당... 일단 써놔
    void ChatboxMapActionAllocator()
    {
        var map = playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion
    
    #endregion

    #region 액션 바인딩 커스텀아이즈

    #endregion
}
