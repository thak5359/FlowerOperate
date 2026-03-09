using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private const int LAST_ID = 1000;


    [Header("Data Sources")]
    [SerializeField] private List<ItemIdData> itemDatas; // 모든 itemIdData (using, Flower은 자식으므로 포함됨;


    [Header("Master Tables")]
    [SerializeField] private FlowerDetailData flowerDetail;
    [SerializeField] private UsableDetailData usableDetail;


    // 모든 데이터를 ID 번호 그대로 인덱스에 저장하는 마스터 배열
    private ItemIdData[] masterDb = new ItemIdData[LAST_ID + 1];

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


        foreach (var data in itemDatas)
        {
            if (data != null && data.ItemID >= 0 && data.ItemID <= LAST_ID)
            {
                masterDb[data.ItemID] = data;
            }
        }
    }


    public T GetData<T>(int id) where T : ItemIdData
    {
        if (id < 0 || id > LAST_ID) return null;
        return masterDb[id] as T;
    }

    #region 2단계 조회 통합 시스템

    #region 1. 공통 정보 
    public string GetItemName(int id, int level = 0) => GetData<ItemIdData>(id)?.ItemName(level) ?? "Unknown";
    public string GetItemDescription(int id, int level = 0) => GetData<ItemIdData>(id)?.Description(level) ?? "";
    public string GetItemAddress(int id, int level = 0) => GetData<ItemIdData>(id)?.Address(level) ?? "";
    #endregion

    #region 2. 꽃 데이터 전용 (FlowerIdData)
    public string GetFlowerSpecies(int id)
    {
        var data = GetData<FlowerIdData>(id);
        return (data != null) ? flowerDetail.Species(data.SpeciesIndex) : "알 수 없는 품종";
    }

    public string GetFlowerColor(int id)
    {
        var data = GetData<FlowerIdData>(id);
        return (data != null) ? flowerDetail.Color(data.ColorIndex) : "알 수 없는 색상";
    }

    //  조합형 이름 반환 (붉은 + 장미)
    public string GetFlowerFullName(int id)
    {
        var data = GetData<FlowerIdData>(id);
        if (data == null) return "Unknown Flower";

        // 기획이 빨강, 파랑같은 명사가 아니라 붉은, 푸른 같은 형용사로 넣어줘야함.
        return $"{flowerDetail.Color(data.ColorIndex)} {flowerDetail.Species(data.SpeciesIndex)}";
    }

    // 꽃말은 리스트로 반환
    public List<string> GetFlowerFloros(int id)
    {
        var floros = new List<string>();
        var data = GetData<FlowerIdData>(id);

        if (data != null)
        {
            foreach (int index in data.FloroIndex)
            {
                floros.Add(flowerDetail.Floro(index));
            }
        }
        return floros;
    }

    #endregion

    #region 3. 도구 데이터 전용 (UsableIdData)
    public int GetUsablePower(int id)
    {
        var data = GetData<UsableIdData>(id);
        return (data != null) ? usableDetail.Power(data.PowerIndex) : 0;
    }

    public int GetUsableDuration(int id)
    {
        var data = GetData<UsableIdData>(id);
        return (data != null) ? usableDetail.Duration(data.DuratIndex) : 0;
    }

    public ChargeInfo GetChargeInfo(int id)
    {
        var data = GetData<UsableIdData>(id);
        return (data != null) ? usableDetail.ChargeInfo(data.ChargeIndex) : new ChargeInfo(0, 0);
    }
    #endregion
    #endregion


}