using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public SpriteRenderer plotRenderer;
    public SpriteRenderer flowerRenderer;
    public int? flowerId = null;

    
    private int LastActivedDay = 0;// 토지의 마지막 활성화된 날짜

    private int cachedDay; //캐싱용

    //토지의 위치 정보(데이터 처리용)
    public readonly int ChunkNumber;
    public readonly int plotNumber;

    // 토지의 인스턴스 데이터 = 저장해야하는거
    public int plotId; // 토지 고유 번호
    public bool isTilled = false; // 땅이 갈렸는가
    public bool isWatered = false; // 물을 뿌렸는가
    public int growth; // 꽃의 성장 단계 == item.level
    public int elapsed; // 심고 경과한 날짜 또는 페이즈.


    private void Awake()
    {
        plotId = Guid.NewGuid().GetHashCode(); // 고유한 ID 생성
    }

    //OnEnable일때 타 관리 클래스에서 loadData 실행하기!
    public void loadData(float input_posX, float input_posY, bool input_isTilled, bool input_isWatered, int input_itemID,
        bool input_isFertilized, int input_growth, int input_elapsed)//DB에서 데이터 로드
    {
        this.transform.position = new Vector3(input_posX, input_posY);
        isTilled = input_isTilled;
        isWatered = input_isWatered;
        flowerId = input_itemID;
        growth = input_growth;
        elapsed = input_elapsed;
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
