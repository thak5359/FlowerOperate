using Unity.Collections;


public class Constant
{

    

    // Class for Constant Value in this Project

    public static readonly FixedString64Bytes ADDRESSABLE_USEAREA = "Prefab_UseArea";
    public static readonly FixedString64Bytes ADDRESSABLE_PLOT= "Prefab_Plot";

    public static readonly FixedString64Bytes ADDRESSABLE_SPR_PLOT_DEFAULT = "Spr_Plot_Default";
    public static readonly FixedString64Bytes ADDRESSABLE_SPR_PLOT_WATERED = "Spr_Plot_Watered";

    public static readonly FixedString64Bytes ADDRESSABLE_SPR_FLOWER_SEED;
    


    public const ushort MAX_COUNT_INVENTORY = 999;
    public const ushort MAX_COUNT_STORAGE = 9999;
    public const ushort MAX_SLOT_INVENTORY = 50;

    //#region 아이템 ID 범위

    //public const short USABLE_START_ID = 100;
    //public const short COMMON_START_ID = 1;
    //public const short FLOWER_START_ID = 700;

    //public const short USABLE_END_ID = 40;
    //public const short COMMON_END_ID = 300;
    //public const short FLOWER_END_ID = 1000;

    //public const short QUALITY_FERTILIZER_START_ID = 600;
    //public const short BOUNTIFUL_FERTILIZER_START_ID = 605;
    //public const short ALLINONE_FERTILIZER_START_ID = 610;

    //public const short ALLINONE_FERTILIZER_END_ID = 614;


    //#endregion

    //#region  장비 아이템 ID 범위

    //public const short MIN_HOE_ID = 0;
    //public const short MAX_HOE_ID = 7;

    //public const short MIN_WATERINGCAN_ID = 8;
    //public const short MAX_WATERINGCAN_ID = 15;

    //public const short MIN_HAMMER_ID = 16;
    //public const short MAX_HAMMER_ID = 23;

    //public const short MIN_SICKLE_ID = 24;
    //public const short MAX_SICKLE_ID = 31;

    //public const short MIN_AXE_ID = 32;
    //public const short MAX_AXE_ID = 39;



    //#endregion

    #region 액션 맵, 스키마 이름

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

   
    #region BLOB 데이터 파일 경로

    public const string BLOB_FOLDER = "Blobs";
    public const string ITEM_BLOB = "ItemData.blob";
    public const string FLOWER_BLOB = "FlowerData.blob";
    public const string USABLE_BLOB = "UsableData.blob";
    public const string FLOWER_DETAIL_BLOB = "FlowerDetail.blob";
    public const string USABLE_DETAIL_BLOB = "UsableDetail.blob";

    #endregion

    #region 오디오 믹서 그룹 이름

    public const string MASTER_MIXER_GROUP = "MasterVolume";
    public const string BGM_MIXER_GROUP = "BGMVolume";
    public const string SFX_MIXER_GROUP = "SFXVolume";
    public const string VOICE_MIXER_GROUP = "VoiceVolume";

    #endregion


    #region LayerName ( For LayerMask on UseAreaFunction)
    public const string LAYER_TREE = "Tree";
    public const string LAYER_PLOT = "Plot";
    public const string LAYER_OBSTACLE = "Obstacle";
    public const string LAYER_INTERACTABLE = "Interactable";
    public const string LAYER_ORE = "Ore";
    public const string LAYER_GRASS = "Grass";
    #endregion

    #region TagName
    public const string TAG_YUUNA = "Yuuna";
    public const string TAG_STORAGE = "Storage";
    public const string TAG_SEEDMAKER = "SeedMaker";
    public const string TAG_BED = "Bed";



    #endregion

    public const int MAX_GROWTH = 4;

}

