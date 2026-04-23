using System;
using UnityEngine;
using static Constant;
using Unity.Mathematics;

[System.Serializable]
public struct PlotData // 저장용 데이터 바구니
{
    public int plotId;

    public Vector3 Position;
    public bool isWatered;
    public int flowerId; // int? 대신 int 사용 (0이면 없는 것으로 처리)
    public int growth;
    public int elapsed;
}

[Serializable]
public class Plot : MonoBehaviour
{
    public PlotData data = new PlotData();
    [Header("땅의 SpriteRender와 꽃의 SpriteRenderer를 드래그 해주세요! GetComponentInChildren으로는 좀 힘듦!")]
    public SpriteRenderer plotRenderer;
    public SpriteRenderer flowerRenderer;
    public int? flowerId = null;

    int cachedInt; //캐싱용

    //int bonusAmount = 0; // 보너스 양   


    private int cachedDay; //캐싱용

    private readonly int plotID; // 토지의 고유 ID

    // 토지의 인스턴스 데이터 = 저장해야하는거
    public int plotId; // 토지 고유 번호
    public bool isWatered = false; // 물을 뿌렸는가
    public int growth; // 꽃의 성장 단계 == item.level
    public int elapsed; // 심고 경과한 날짜 또는 페이즈.
    public int grade;


    private void Awake()
    {
        plotId = Guid.NewGuid().GetHashCode(); // 고유한 ID 생성
    }

    //OnEnable일때 타 관리 클래스에서 loadData 실행하기!
    public void loadData(float input_posX, float input_posY, bool input_isTilled, bool input_isWatered, int input_itemID,
        bool input_isFertilized, int input_growth, int input_elapsed)//DB에서 데이터 로드
    {
        this.transform.position = new Vector3(input_posX, input_posY);
        isWatered = input_isWatered;
        flowerId = input_itemID;
        growth = input_growth;
        elapsed = input_elapsed;
    }

    public int Watering()
    {
        if (isWatered == false)
        {
            isWatered = true;
            return 1;
        }
        else return 0;
    }

    public void Fertilizer_Quality(int itemID)
    {
        cachedInt = itemID - QUALITY_FERTILIZER_START_ID;

        switch (cachedInt)
        {
            case 0:
                grade += 1;
                break;
            case 1:
                grade += 2;
                break;
            case 2:
                grade += 3;
                break;
            case 3:
                grade += 4;
                break;
            case 4:
                grade += 5;
                break;
        }
    }

    public void Fertilizer_Bountiful(int itemID)
    {
        cachedInt = itemID - BOUNTIFUL_FERTILIZER_START_ID;

        uint seed = (uint)DateTime.Now.Ticks;


        switch (cachedInt)
        {
            case 0:
                grade += 1;
                break;
            case 1:
                grade += 2;
                break;
            case 2:
                grade += 3;
                break;
            case 3:
                grade += 4;
                break;
            case 4:
                grade += 5;
                break;
        }
    }
    public void Fertilizer_AllInOne(int itemID)
    {

    }

    public PlotData GetSaveData()
    {
        data.plotId = this.plotId;
        data.Position = this.transform.position;
        data.isWatered = this.isWatered;
        data.flowerId = this.flowerId ?? 0; // null이면 0으로 저장
        data.growth = this.growth;
        data.elapsed = this.elapsed;

        return data;
    }
    public void LoadFromData(PlotData data)
    {
        this.plotId = data.plotId;
        this.transform.position = data.Position;
        this.isWatered = data.isWatered;
        this.flowerId = data.flowerId == 0 ? (int?)null : data.flowerId;
        this.growth = data.growth;
        this.elapsed = data.elapsed;
    }

    public int sowSeed(int input_itemId)
    {
        if (flowerId == 0)
        {
            flowerId = input_itemId;
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
