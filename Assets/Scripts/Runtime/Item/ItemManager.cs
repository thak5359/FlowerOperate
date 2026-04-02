using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using static Constant;



public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private const int LAST_ID = 1000;

    [Header("Data Sources")]
    // 이 리스트에 FlowerIdData, UsableIdData SO들을 다 넣으시면 됩니다.
    [SerializeField] private List<ItemIdData> itemIdDatas;

    [Header("Master Tables (Detail)")]
    [SerializeField] private FlowerDetailData flowerDetail;
    [SerializeField] private UsableDetailData usableDetail;

    // 모든 유형의 아이템을 담는 통합 마스터 DB (부모 타입으로 선언)
    [SerializeField] MasterData[] masterDb = new MasterData[LAST_ID + 1];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabases();
        }
        else { Destroy(gameObject); }
    }

    private void InitializeDatabases()
    {
        System.Array.Clear(masterDb, 0, masterDb.Length);
        foreach (var data in itemIdDatas)
        {
            switch (data.startId)
            {
                case 0:
                    {
                        var IDdata = data as UsableIdData;
                        bool flowControl = InsertDataToMasterDB(IDdata);
                        if (!flowControl)
                            continue;
                    }
                    break;
                case 40:
                    {
                        bool flowControl = InsertDataToMasterDB(data);
                        if (!flowControl)
                            continue;
                    }
                    break;
                case 300:
                    {
                        var IDdata = data as FlowerIdData;
                        bool flowControl = InsertDataToMasterDB(IDdata);
                        if (!flowControl)
                            continue;
                    }
                    break;
                default:
                    continue;
            }
        }
        Debug.Log("ItemManager: 데이터베이스 초기화 완료!");
    }

    #region 데이터 타입별 InsertDataToMasterDB 함수
    private bool InsertDataToMasterDB(ItemIdData data) //사용 불가능 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (byte i = 0; i < data.itemName.Count; i++)
        {
            MasterData masterData = new MasterData(data.ItemName(i));
            if (masterData != null)
                masterDb[data.startId + i] = masterData;
        }
        return true;
    }
    private bool InsertDataToMasterDB(UsableIdData data) //사용 가능 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (byte i = 0; i < data.itemName.Count; i++)
        {
            UsableDataBase usable = new UsableDataBase(
                data.ItemName(i), usableDetail.Duration(data.DuratIndex(i))
                , (sbyte)usableDetail.Power(data.PowerIndex(i)), usableDetail.ChargeInfo(data.ChargeIndex(i)));
            if (usable != null)
                masterDb[data.startId + i] = usable;
        }
        return true;
    }
    private bool InsertDataToMasterDB(FlowerIdData data) //꽃 아이템
    {
        if (data == null)
        {
            Debug.Log("ID데이터에 잘못된 데이터가 들어있습니다.");
            return false;
        }
        for (byte i = 0; i < data.itemName.Count; i++)
        {
            FlowerDataBase flower = new FlowerDataBase(
                data.ItemName(i), flowerDetail.Species(data.SpeciesIndex(i)),
                flowerDetail.Color(data.ColorIndex(i)), flowerDetail.Floro((sbyte)data.FloroIndex(i)), flowerDetail.Floro(data.FloroIndex2((sbyte)i)));
            if (flower != null)
            {
                var seed = new FlowerDataBase(flower);
                seed.SetItemName(data.itemName[i] + " 씨앗");
                seed.SetIsSeed(true);

                masterDb[data.startId + (i * 2 + 1)] = flower;
                masterDb[data.startId + (i * 2)] = seed;

                if (seed != null)
                    Debug.Log(seed.GetItemName);
            }
        }
        return true;
    }
    #endregion

    // --- 데이터 접근 엔진 ---

    //  기본 데이터 가져오기 
    public T GetIdData<T>(short id) where T : MasterData
    {
        if (id < 0 || id > LAST_ID) return null;
        return masterDb[id] as T;
    }

    #region 1. 공통 정보
    public FixedString64Bytes GetItemName(short id) => GetIdData<MasterData>(id)?.GetItemName ?? "알 수 없는 아이템";
    public FixedString128Bytes GetItemDescription(short id) => GetIdData<MasterData>(id)?.GetItemDescription ?? "";
    public FixedString128Bytes GetItemAddress(short id) => GetIdData<MasterData>(id)?.GetItemSpriteAddress ?? "";
    #endregion

    #region 2. 꽃 데이터 

    // 꽃 전체 이름 (색상 + 품종 조합) 시범적 기능
    //public string GetFlowerFullName(int id)
    //{
    //    var data = GetIdData<FlowerIdData>(id);
    //    if (data == null) return "알 수 없는 꽃";

    //    // 기획 가이드: Color에 '붉은', '푸른' 등으로 저장되어 있어야 자연스럽습니다.
    //    return $"{flowerDetail.Color(data.ColorIndex)} {flowerDetail.Species(data.SpeciesIndex)}";
    //}

    // 꽃 이름
    public FixedString32Bytes GetFlowerColor(short id)
    {
        var data = GetIdData<FlowerDataBase>(id);
        if (data == null) return "알 수 없는 꽃";

        return data.GetColor;
    }

    // 꽃말 리스트 가져오기
    public List<FixedString32Bytes> GetFlowerFloros(short id)
    {
        List<FixedString32Bytes> result = new List<FixedString32Bytes>();
        var data = GetIdData<FlowerDataBase>(id);

        if (data != null)
        {
            result.Add(data.GetFloro);

            if (data != null)
                result.Add(data.GetFloro);
        }
        return result;
    }

    #endregion
    #region 3. 도구 데이터 (Usable)

    // 차징 정보 (시간, 최대 단계)
    public ChargeInfo GetChargeInfo(short id)
    {
        var data = GetIdData<UsableDataBase>(id);
        // ID 데이터에서 찾은 chargeIndex로 Detail 테이블에서 ChargeInfo 구조체를 가져옵니다.
        return (data != null) ? data.GetChargeInfo : new ChargeInfo(0, 0);
    }

    // 도구 파워 (곡갱이 파워 등)
    public sbyte GetUsablePower(short id)
    {
        var data = GetIdData<UsableDataBase>(id);
        return (data != null) ? data.GetPower : default(sbyte);
    }

    // 도구 내구도 (기본 최대치)
    public short GetMaxDuration(short id)
    {
        var data = GetIdData<UsableDataBase>(id);
        return (data != null) ? data.GetDuration : default(short);
    }

    #endregion
}

[System.Serializable]
public class MasterData
{
    [SerializeField] protected FixedString64Bytes itemName;
    [SerializeField] protected FixedString128Bytes description;
    [SerializeField] protected FixedString64Bytes spriteAddress;

    public MasterData(FixedString64Bytes name)
    {
        this.itemName = name;
        //this.description = description;
        //this.spriteAddress = spriteAddress;
    }

    public FixedString64Bytes GetItemName => itemName;
    public FixedString128Bytes GetItemDescription => description;
    public FixedString128Bytes GetItemSpriteAddress => spriteAddress;
    public void SetItemName(string itemName) => this.itemName = itemName;
    public void SetItemDescription(string description) => this.description = description;
}

[System.Serializable]
public class FlowerDataBase : MasterData
{
    [SerializeField] private FixedString64Bytes species;
    [SerializeField] private FixedString32Bytes color;
    [SerializeField] private FixedString32Bytes floro;
    [SerializeField] private FixedString32Bytes floro2;
    [SerializeField] private bool isSeed = false;

    public FlowerDataBase(FixedString64Bytes Name = default(FixedString64Bytes), FixedString64Bytes Species = default(FixedString64Bytes),
        FixedString32Bytes Color = default(FixedString32Bytes), FixedString32Bytes Floro = default(FixedString32Bytes), FixedString32Bytes Floro2 = default(FixedString32Bytes))
        : base(Name)
    {
        //this.description = Description;
        //this.spriteAddress = Addr;
        this.species = Species;
        this.color = Color;
        this.floro = Floro;
        this.floro2 = Floro2;
    }

    public FlowerDataBase(FlowerDataBase other)
        : base(other.itemName)
    {
        this.species = other.species;
        this.color = other.color;
        this.floro = other.floro;
        this.floro2 = other.floro2;
    }

    public FixedString64Bytes GetSpecies => species;
    public FixedString32Bytes GetColor => color;
    public FixedString32Bytes GetFloro => floro;
    public FixedString32Bytes GetFloro2 => floro2;
    public bool GetIsSeed => isSeed;

    public void SetIsSeed(bool isSeed = false) => this.isSeed = isSeed;
}

[System.Serializable]
public class UsableDataBase : MasterData
{
    [SerializeField] private short duration;
    [SerializeField] private sbyte power;
    [SerializeField] private ChargeInfo chargeInfo;

    public UsableDataBase(FixedString64Bytes Name, short dura, sbyte pow, ChargeInfo info)
        : base(Name)
    {
        //this.description = Description;
        //this.spriteAddress = Addr;
        this.duration = dura;
        this.power = pow;
        this.chargeInfo = info;
    }

    public short GetDuration => duration;
    public sbyte GetPower => power;
    public ChargeInfo GetChargeInfo => chargeInfo;
}