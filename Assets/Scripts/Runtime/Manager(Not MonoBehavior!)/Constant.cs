using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constant;


public class Constant
{
    public const string ADDRESSABLE_USEAREA = "Prefab_UseArea";



    #region 아이템 장비 코드


    public const int MIN_HOE_ID = 0;
    public const int MAX_HOE_ID = 7;

    public const int MIN_WATERINGCAN_ID = 8;
    public const int MAX_WATERINGCAN_ID = 15;

    public const int MIN_HAMMER_ID = 16;
    public const int MAX_HAMMER_ID = 23;

    public const int MIN_SICKLE_ID = 24;
    public const int MAX_SICKLE_ID = 31;

    public const int MIN_AXE_ID = 32;
    public const int MAX_AXE_ID = 39;

    public const int MIN_CONSUMABLE_ID = 500;
    public const int MAX_CONSUMABLE_ID = 7;

    #endregion


    #region 액션 맵 이름

    public const string TITLE_MAP_NAME = "MAP_TITLE";
    public const string SETTING_MAP_NAME = "MAP_SETTING";
    public const string PAUSEMENU_MAP_NAME = "MAP_PAUSE";
    public const string SHOP_MAP_NAME = "MAP_SHOP";
    public const string FARM_MAP_NAME = "MAP_FARM";
    public const string INVENTORY_MAP_NAME = "MAP_INVENTORY";
    public const string STORAGE_MAP_NAME = "MAP_STORAGE";
    public const string CHATBOX_MAP_NAME = "MAP_CHATBOX";

    public const string WASD_SCHEME_NAME = "WASD_Scheme";
    public const string ARROW_SCHEME_NAME = "Arrow_Scheme";
    #endregion


    #region 씬 이름
    public const string TITLE_SCENE_NAME = "MainTitle";
    public const string FARM_SCENE_NAME = "SampleScene";
    #endregion

    #region 아이템 데이터


    public const int LAST_USABLE_ID = 40;
    public const int LAST_COMMON_ID = 300;
    public const int LAST_FLOWER_ID = 1000;


    #endregion 
}