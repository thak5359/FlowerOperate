using System.Collections.Generic;
using UnityEngine;

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

        foreach (var data in itemIdDatas)
        {
            if (data != null && data.ItemID >= 0 && data.ItemID <= LAST_ID)
            {
                masterDb[data.ItemID] = data;
            }
        }
        Debug.Log("ItemManager: 데이터베이스 초기화 완료!");
    }

    // --- 데이터 접근 엔진 ---

    //  기본 데이터 가져오기 
    public T GetIdData<T>(int id) where T : ItemIdData
    {
        if (id < 0 || id > LAST_ID) return null;
        return masterDb[id] as T;
    }

    #region 1. 공통 정보
    public string GetItemName(int id, int level = 0) => GetIdData<ItemIdData>(id)?.ItemName(level) ?? "알 수 없는 아이템";
    public string GetItemDescription(int id, int level = 0) => GetIdData<ItemIdData>(id)?.Description(level) ?? "";
    public string GetItemAddress(int id, int level = 0) => GetIdData<ItemIdData>(id)?.Address(level) ?? "";
    #endregion

    #region 2. 꽃 데이터 

    // 꽃 전체 이름 (색상 + 품종 조합) 시범적 기능
    public string GetFlowerFullName(int id)
    {
        var data = GetIdData<FlowerIdData>(id);
        if (data == null) return "알 수 없는 꽃";

        // 기획 가이드: Color에 '붉은', '푸른' 등으로 저장되어 있어야 자연스럽습니다.
        return $"{flowerDetail.Color(data.ColorIndex)} {flowerDetail.Species(data.SpeciesIndex)}";
    }

    // 꽃 이름
    public string GetFlowerColor(int id)
    {
        var data = GetIdData<FlowerIdData>(id);
        if (data == null) return "알 수 없는 꽃";

        return flowerDetail.Color(data.ColorIndex);
    }

    // 꽃말 리스트 가져오기
    public List<string> GetFlowerFloros(int id)
    {
        List<string> result = new List<string>();
        var data = GetIdData<FlowerIdData>(id);

        if (data != null)
        {
            foreach (int index in data.FloroIndex)
            {
                result.Add(flowerDetail.Floro(index));
            }
        }
        return result;
    }

    #region 3. 도구 데이터 (Usable)

    // 차징 정보 (시간, 최대 단계)
    public ChargeInfo GetChargeInfo(int id)
    {
        var data = GetIdData<UsableIdData>(id);
        // ID 데이터에서 찾은 chargeIndex로 Detail 테이블에서 ChargeInfo 구조체를 가져옵니다.
        return (data != null) ? usableDetail.ChargeInfo(data.ChargeIndex) : new ChargeInfo(0, 0);
    }

    // 도구 파워 (곡갱이 파워 등)
    public int GetUsablePower(int id)
    {
        var data = GetIdData<UsableIdData>(id);
        return (data != null) ? usableDetail.Power(data.PowerIndex) : 0;
    }

    // 도구 내구도 (기본 최대치)
    public int GetMaxDuration(int id)
    {
        var data = GetIdData<UsableIdData>(id);
        return (data != null) ? usableDetail.Duration(data.DuratIndex) : 0;
    }

    #endregion
    #endregion
}