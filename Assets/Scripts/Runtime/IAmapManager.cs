using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public interface IMapChangable // 컨트롤 방법을을 변경하는 기능은 이 인터페이스를 포함.
{
    string getCurrentIAmap();
    void changeIAmap(string targetMap);

    void changeIAmapTitle();

    void changeIAmapUI();

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
    [SerializeField] private string prevIAMap;

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
            prevIAMap = null;
        }
        else { Destroy(this); }
    }
    public static IAmapManager Instance => instance;

    string IMapChangable.getCurrentIAmap()
    {


        return playerInput.currentActionMap.name;
    }

    void IMapChangable.changeIAmap(string targetMap) // 직접 키고 싶은 맵 요청
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(targetMap);

        }
    }

    void IMapChangable.changeIAmapTitle()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(TITLE_MAP_NAME);
            prevIAMap = TITLE_MAP_NAME;
        }
    }
    void IMapChangable.changeIAmapUI()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(SETTING_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }

    void IMapChangable.changeIAmapInventory()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(INVENTORY_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }

    void IMapChangable.changeIAmapStorage()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(STORAGE_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }

    void IMapChangable.changeIAmapPauseMenu()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(PAUSEMENU_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }

    void IMapChangable.changeIAmapShop()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(SHOP_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }

    void IMapChangable.changeIAmapFarm()
    {
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap(FARM_MAP_NAME);
            prevIAMap = SETTING_MAP_NAME;
        }
    }
    void IMapChangable.changeIAmapPrev()
    {
        if (playerInput != null && prevIAMap != null)
        {
            playerInput.SwitchCurrentActionMap(prevIAMap);
            prevIAMap = null;
        }
    }

}
