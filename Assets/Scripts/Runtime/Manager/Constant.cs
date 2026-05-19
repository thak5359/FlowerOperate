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

    #region 플레이어 Anim 상태 정의명

    public const string ANIM_X = "MoveX";
    public const string ANIM_Y = "MoveY"; 
    public const string ANIM_MOVING = "IsMoving";

    #endregion

    public const float CHUNK_SIZE = 15.0f;

    public const int INVENTORY_ROW_SIZE = 5;
    public const int INVENTORY_COLUMN_SIZE = 10;
    public const int INVENTORY_SLOT_COUNT= INVENTORY_ROW_SIZE * INVENTORY_COLUMN_SIZE;

}

