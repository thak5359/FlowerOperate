using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


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

    void changeIAmapPrev();
}


public class IAmapManager : MonoBehaviour, IMapChangable
{
    //컨트롤 방식을 관리하는 매니저
    private static IAmapManager instance;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] private Stack<string> prevMapStack = new Stack<string>();

    const string TITLE_MAP_NAME = "MAP_TITLE";

    const string SETTING_MAP_NAME = "MAP_SETTING";
    const string PAUSEMENU_MAP_NAME = "MAP_PAUSE";

    const string SHOP_MAP_NAME = "MAP_SHOP";
    const string FARM_MAP_NAME = "MAP_FARM";

    const string INVENTORY_MAP_NAME = "MAP_INVENTORY";
    const string STORAGE_MAP_NAME = "MAP_STORAGE";

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else { Destroy(this.gameObject); }

    }
    public static IAmapManager Instance => instance;

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



    void IMapChangable.changeIAmapPrev()
    {
        if (playerInput != null && prevMapStack.Count > 0)
        {
            string target = prevMapStack.Pop();
            playerInput.SwitchCurrentActionMap(target);
            Debug.Log($"[IA Manager] Pop: {target} / 남은 스택 크기: {prevMapStack?.Count??0}");
        }
    }

    void IMapChangable.changeIAmap(string targetMap) // 직접 키고 싶은 맵 요청
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(targetMap);

        }
    }

}
