using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using static Constant;



public class ActionKeyMapper : IAsyncStartable
{

    //Wasd 조작법 쓸거면 true 아니면 False
    [SerializeField] public bool isWASDKeySetting = true;


    private PlayerInput _playerInput;
    private PauseMenu _pauseMenu;
    private HotbarManager _hotbarManager;
    private PlayerController _playerController;


    [Inject]
    void Construct(PlayerInput input_playerInput, PauseMenu input_pauseMenu,
     HotbarManager input_hotbarManager,PlayerController input_playerController)
    {
        Debug.Log("Construct B");
        _playerInput = input_playerInput;
        _pauseMenu = input_pauseMenu;
        _hotbarManager = input_hotbarManager;
        _playerController = input_playerController;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {

        _playerInput.actions = await AddressableManager.LoadAssetAsync<InputActionAsset>("InputActionAsset");

        while (_playerInput.actions == null)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
        }


        string targetScheme = isWASDKeySetting ? WASD_SCHEME_NAME : ARROW_SCHEME_NAME;

        _playerInput.actions.bindingMask = InputBinding.MaskByGroup(targetScheme);

        FarmMapActionAllocator();
        PauseMapActionAllocator();
        SettingMapActionAllocator();
        _playerInput.SwitchCurrentActionMap(FARM_MAP_NAME);
        Debug.Log("farmMap으로 현재 맵 전환 완료!");

    }

    #region wasd, 화살표 조작법 선택/변경

    public void changeKeySetting(bool isWASD)
    {
        isWASDKeySetting = isWASD;
        Debug.Log("여기까지는 정상!");
        string targetScheme = isWASD ? WASD_SCHEME_NAME : ARROW_SCHEME_NAME;
        Debug.Log("여기까지는 정상!!");
        _playerInput.actions.bindingMask = InputBinding.MaskByGroup(targetScheme);
        
        Debug.Log("여기까지는 정상!!");
        //Debug.Log($"[IA Manager] {targetScheme}이 준비됨!");
    }
    #endregion

    #region  맵 별 액션에 함수 할당

    #region 타이틀 키 할당
    void TitleMapActionAllocator()
    {
        var map = _playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion

    #region 정지 메뉴 키 할당
    void PauseMapActionAllocator()
    {
        var map = _playerInput.actions.FindActionMap(PAUSEMENU_MAP_NAME); // 현재 맵에서 사용중인 IA에서 특정 맵을 가져옴
        var actionEscape = map.FindAction("Escape"); // 거기서 어떤키를 눌렀을때에 동작을 할당하는 곳을 찾음
        actionEscape.performed += _pauseMenu.OnBackAction; // 함수 할당
    }
    #endregion

    #region 세팅 메뉴 키 할당
    void SettingMapActionAllocator()
    {
        var map = _playerInput.actions.FindActionMap(SETTING_MAP_NAME);
        var actionEscape = map.FindAction("Escape");
        actionEscape.performed += _pauseMenu.OnBackAction;
        Debug.Log("세팅 키 할당됨!");

    }
    #endregion


    // 기존의 쓰던 A 맵  + 할당 >>> B 맵 + 할당...  A 맵 >> A맵에서의 키 해제 >> B에서의 키 할당 >>  맵 액션맵 A에서 B로 변경

    #region 농장 조작 키 할당
    void FarmMapActionAllocator()
    {
        if (_playerInput == null || _playerInput.actions == null) return;

        InputActionMap map = _playerInput.actions.FindActionMap(FARM_MAP_NAME);
        if (map == null) return;

        map.Disable();


        InputAction actionMove = map.FindAction("Move");

        if (actionMove != null)
        {
            actionMove.performed += _playerController.OnMove;
            actionMove.canceled += _playerController.OnMove;
        }

        InputAction actionUse = map.FindAction("Use");
        actionUse.started += _playerController.OnUse;
        actionUse.performed += _playerController.OnUse;
        actionUse.canceled += _playerController.OnUse;

        InputAction actionInteract = map.FindAction("Interact");
        actionInteract.started += _playerController.OnInteract;
        actionInteract.performed += _playerController.OnInteract;
        actionInteract.canceled += _playerController.OnInteract;

        #region  핫 슬롯 좌우 이동
        InputAction actionPrevHotSlot = map.FindAction("PrevHotSlot");
        actionPrevHotSlot.performed += _hotbarManager.OnPrevHotSlot;

        InputAction actionNextHotSlot = map.FindAction("NextHotSlot");
        actionNextHotSlot.performed += _hotbarManager.OnNextHotSlot;
        #endregion

        InputAction actionEscape = map.FindAction("Escape");
        actionEscape.performed += _pauseMenu.OnBackAction;

        #region 핫슬롯 1~0번 바로 이동

        for (int i = 0; i < 10; i++)
        {
            int slotIndex = i;
            // 1~9 , 0번 슬롯 할당
            int displayNum = (i == 9) ? 0 : i + 1;
            string actionName = $"HotSlot{displayNum}";

            InputAction action = map.FindAction(actionName);
            if (action != null)
            {
                //ctx로 InputAction.callbackContext를 처리
                action.performed += ctx => _hotbarManager.pointSlot(slotIndex);
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
        var map = _playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion

    #region 가게 경영 조작 키 할당
    void ShopMapActionAllocator()
    {
        var map = _playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion


    #region 대화창 조작 키 할당... 일단 써놔
    void ChatboxMapActionAllocator()
    {
        var map = _playerInput.actions.FindActionMap(SETTING_MAP_NAME);
    }
    #endregion

    #endregion

}
