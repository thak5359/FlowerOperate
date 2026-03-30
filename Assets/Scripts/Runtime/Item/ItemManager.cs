using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;



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
                        bool flowerControl = InsertDataToMasterDB(data);
                        if (!flowerControl)
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
        for (int i = 0; i < data.itemName.Count; i++)
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
        for (int i = 0; i < data.itemName.Count; i++)
        {
            UsableDataBase usable = new UsableDataBase(
                data.ItemName(i), usableDetail.Duration(data.DuratIndex(i))
                , usableDetail.Power(data.PowerIndex(i)), usableDetail.ChargeInfo(data.ChargeIndex(i)));
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
        for (int i = 0; i < data.itemName.Count; i++)
        {
            FlowerDataBase flower = new FlowerDataBase(
                data.ItemName(i), flowerDetail.Species(data.SpeciesIndex(i)),
                flowerDetail.Color(data.ColorIndex(i)), flowerDetail.Floro(data.FloroIndex(i)), flowerDetail.Floro(data.FloroIndex2(i)));
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
    public T GetIdData<T>(int id) where T : MasterData
    {
        if (id < 0 || id > LAST_ID) return null;
        return masterDb[id] as T;
    }

    #region 1. 공통 정보
    public string GetItemName(int id, int level = 0) => GetIdData<MasterData>(id)?.GetItemName ?? "알 수 없는 아이템";
    public string GetItemDescription(int id, int level = 0) => GetIdData<MasterData>(id)?.GetItemDescription ?? "";
    public string GetItemAddress(int id, int level = 0) => GetIdData<MasterData>(id)?.GetItemSpriteAddress ?? "";
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
    public string GetFlowerColor(int id)
    {
        var data = GetIdData<FlowerDataBase>(id);
        if (data == null) return "알 수 없는 꽃";

        return data.GetColor;
    }

    // 꽃말 리스트 가져오기
    public List<string> GetFlowerFloros(int id)
    {
        List<string> result = new List<string>();
        var data = GetIdData<FlowerDataBase>(id);

        if (data != null)
        {
            result.Add(data.Getfloro);

            if (data != null)
                result.Add(data.Getfloro);
        }
        return result;
    }

    #endregion
    #region 3. 도구 데이터 (Usable)

    // 차징 정보 (시간, 최대 단계)
    public ChargeInfo GetChargeInfo(int id)
    {
        var data = GetIdData<UsableDataBase>(id);
        // ID 데이터에서 찾은 chargeIndex로 Detail 테이블에서 ChargeInfo 구조체를 가져옵니다.
        return (data != null) ? data.GetChargeInfo : new ChargeInfo(0, 0);
    }

    // 도구 파워 (곡갱이 파워 등)
    public int GetUsablePower(int id)
    {
        var data = GetIdData<UsableDataBase>(id);
        return (data != null) ? data.GetPower : 0;
    }

    // 도구 내구도 (기본 최대치)
    public int GetMaxDuration(int id)
    {
        var data = GetIdData<UsableDataBase>(id);
        return (data != null) ? data.GetDuration : 0;
    }

    #endregion
}

[System.Serializable]
public class MasterData
{
    [SerializeField] protected string itemName;
    [SerializeField] protected string description;
    [SerializeField] protected string spriteAddress;

    public MasterData(string name)
    {
        this.itemName = name;
        //this.description = description;
        //this.spriteAddress = spriteAddress;
    }

    public string GetItemName => itemName;
    public string GetItemDescription => description;
    public string GetItemSpriteAddress => spriteAddress;
    public void SetItemName(string itemName) => this.itemName = itemName;
    public void SetItemDescription(string description) => this.description = description;
}

[System.Serializable]
public class FlowerDataBase : MasterData
{
    [SerializeField] private string species;
    [SerializeField] private string color;
    [SerializeField] private string floro;
    [SerializeField] private string floro2;
    [SerializeField] private bool isSeed = false;

    public FlowerDataBase(string Name = "Empty", string Species = "Empty", string Color = "Empty", string Floro = "Empty", string Floro2 = null)
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

    public string GetSpecies => species;
    public string GetColor => color;
    public string Getfloro => floro;
    public string GetFloro2 => floro2;
    public bool GetIsSeed => isSeed;

    public void SetIsSeed(bool isSeed = false) => this.isSeed = isSeed;
}

[System.Serializable]
public class UsableDataBase : MasterData
{
    [SerializeField] private int duration;
    [SerializeField] private int power;
    [SerializeField] private ChargeInfo chargeInfo;

    public UsableDataBase(string Name, int dura, int pow, ChargeInfo info)
        : base(Name)
    {
        //this.description = Description;
        //this.spriteAddress = Addr;
        this.duration = dura;
        this.power = pow;
        this.chargeInfo = info;
    }

    public int GetDuration => duration;
    public int GetPower => power;
    public ChargeInfo GetChargeInfo => chargeInfo;
}