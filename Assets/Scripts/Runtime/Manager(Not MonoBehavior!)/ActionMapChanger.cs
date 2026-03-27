using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using static Constant;



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




public class ActionMapChanger : IMapChangable, IStartable
{
    

    [Header("InputAction Asset 파일을 할당해주세요. 위치는 Asset/Settings입니다")]
    [SerializeField] public InputActionAsset IA;

    private PlayerInput _playerInput;
    private PlayerController _playerController;
    private HotbarManager _hotbarManager;

    private Stack<string> prevMapStack = new();

    [Inject]
    void Construct(PlayerInput input_playerInput, HotbarManager input_hotBarManager, PlayerController input_playerController)
    {
        _playerInput = input_playerInput;
        _hotbarManager = input_hotBarManager;
        _playerController = input_playerController;
    }



    //public async UniTask StartAsync(CancellationToken cancellation)
    //{
       
    //}

    void IStartable.Start()
    {
        Debug.Log($"Construct A");
        if (_playerController == null)
        {
            Debug.LogError("[IA Manager] PlayerController 인스턴스를 찾을 수 없음!");
            return;
        }
    }  

    #region IA 맵 변경

    private void PushAndChange(string targetMap)
    {
        Debug.Log($"{targetMap}으로 전환하기");

        if (_playerInput == null) return;

        string currentMap = _playerInput.currentActionMap.name;

        // 똑같은 맵을 또 스택에 넣는 것을 방지 
        if (currentMap != targetMap)
        {
            prevMapStack.Push(currentMap);
            Debug.Log($"[IA Manager] Push: {currentMap} / 현재 스택 크기: {prevMapStack?.Count??null}");
        }

        _playerInput.SwitchCurrentActionMap(targetMap);
        
    }

    string IMapChangable.getCurrentIAmap()
    {
        return (_playerInput?.currentActionMap?.name ?? "nothing");
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
        if (_playerInput != null && prevMapStack.Count > 0)
        {
            string target = prevMapStack.Pop();
            _playerInput.SwitchCurrentActionMap(target);
            Debug.Log($"[IA Manager] Pop: {target} / 남은 스택 크기: {prevMapStack?.Count??0}");

        }
    }
    void IMapChangable.changeIAmap(string targetMap) // 직접 키고 싶은 맵 요청
    {
        if (_playerInput != null)
        {
            _playerInput.SwitchCurrentActionMap(targetMap);
        }
    }
    #endregion

   
    #region 액션 바인딩 커스텀아이즈


     public void Keymapping()
    { InputActionMap map = _playerInput.currentActionMap; }

    #endregion
}
