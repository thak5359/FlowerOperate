using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlotData // 저장용 데이터 바구니
{
    public bool isTilled;
    public bool isWatered;
    public bool isFertilized;
    public int flowerId; // int? 대신 int 사용 (0이면 없는 것으로 처리)
    public int growth;
    public int elapsed;
    public int lastActivedDay;
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
    public bool isTilled = false; // 땅이 갈렸는가
    public bool isWatered = false; // 물을 뿌렸는가
    public bool isFertilized; // 비료를 뿌렸는가
    public int growth; // 꽃의 성장 단계 == item.level
    public int elapsed; // 심고 경과한 날짜 또는 페이즈.


    private void Awake()
    {
        flowerId = this.gameObject.GetComponent<ItemDataContainer>().GetItemID;
    }

    //OnEnable일때 타 관리 클래스에서 loadData 실행하기!
    public void loadData(bool input_isTilled, bool input_isWatered, int input_itemID,
        bool input_isFertilized, int input_growth, int input_elapsed)//DB에서 데이터 로드
    {
        isTilled = input_isTilled;
        isWatered = input_isWatered;
        flowerId = input_itemID;
        growth = input_growth;
        elapsed = input_elapsed;
    }

    //TODO! 시간 관리 클래스 만들어 이 친구에게 오늘 날짜 갖다주기.
    //public void OnEnable()
    //{
    //    turnOn();
    //}

    //public void OnDisable()
    //{
    //    turnOff();
    //}

    public PlotData GetSaveData()
    {
        data.isTilled = this.isTilled;
        data.isWatered = this.isWatered;
        data.isFertilized = this.isFertilized;
        data.flowerId = this.flowerId ?? 0; // null이면 0으로 저장
        data.growth = this.growth;
        data.elapsed = this.elapsed;
        data.lastActivedDay = this.LastActivedDay;

        return data;
    }
    public void LoadFromData(PlotData data)
    {
        this.isTilled = data.isTilled;
        this.isWatered = data.isWatered;
        this.isFertilized = data.isFertilized;
        this.flowerId = data.flowerId == 0 ? (int?)null : data.flowerId;
        this.growth = data.growth;
        this.elapsed = data.elapsed;
        this.LastActivedDay = data.lastActivedDay;
    }

    public void turnOn(int currentDay)
    {
        if (flowerId != 0)
        {
            cachedDay = currentDay - LastActivedDay;
            if (cachedDay > 0)
            {

                for (int i = 0; i < cachedDay; i++)
                {
                    if (growth < 5)
                    {
                        growth++;
                    }
                }
            }
        }

    }
    public void turnOff(int currentDay)
    {
        if (flowerId != 0)
        {
            LastActivedDay = currentDay;
        }
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
