using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


public class Constant
{
    public static readonly FixedString64Bytes ADDRESSABLE_USEAREA = "Prefab_UseArea";



    #region 아이템 장비 코드

    public const short MIN_HOE_ID = 0;
    public const short MAX_HOE_ID = 7;

    public const short MIN_WATERINGCAN_ID = 8;
    public const short MAX_WATERINGCAN_ID = 15;

    public const short MIN_HAMMER_ID = 16;
    public const short MAX_HAMMER_ID = 23;

    public const short MIN_SICKLE_ID = 24;
    public const short MAX_SICKLE_ID = 31;

    public const short MIN_AXE_ID = 32;
    public const short MAX_AXE_ID = 39;

    public const short MIN_CONSUMABLE_ID = 500;
    public const short MAX_CONSUMABLE_ID = 7;

    #endregion


    #region 액션 맵 이름

    public static readonly FixedString64Bytes TITLE_MAP_NAME = "MAP_TITLE";
    public static readonly FixedString64Bytes SETTING_MAP_NAME = "MAP_SETTING";
    public static readonly FixedString64Bytes PAUSEMENU_MAP_NAME = "MAP_PAUSE";
    public static readonly FixedString64Bytes SHOP_MAP_NAME = "MAP_SHOP";
    public static readonly FixedString64Bytes FARM_MAP_NAME = "MAP_FARM";
    public static readonly FixedString64Bytes INVENTORY_MAP_NAME = "MAP_INVENTORY";
    public static readonly FixedString64Bytes STORAGE_MAP_NAME = "MAP_STORAGE";
    public static readonly FixedString64Bytes CHATBOX_MAP_NAME = "MAP_CHATBOX";

    public static readonly FixedString64Bytes WASD_SCHEME_NAME = "WASD_Scheme";
    public static readonly FixedString64Bytes ARROW_SCHEME_NAME = "Arrow_Scheme";
    #endregion


    #region 씬 이름
    public static readonly FixedString64Bytes TITLE_SCENE_NAME = "MainTitle";
    public static readonly FixedString64Bytes FARM_SCENE_NAME = "SampleScene";
    #endregion

    #region 아이템 데이터

    public const short  LAST_USABLE_ID = 40;
    public const short LAST_COMMON_ID = 300;
    public const short LAST_FLOWER_ID = 1000;

    #endregion 
}