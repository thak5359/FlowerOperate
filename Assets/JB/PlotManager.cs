using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : ItemStorageParent
{
    // 실제 아이템 데이터를 직접 관리하는 리스트
    [SerializeField]
    private List<ItemObjectData> plotItems = new List<ItemObjectData>();
    
    // 시각적 상태와 작물 성장을 담당하는 플롯 컴포넌트 리스트 (자식 오브젝트)
    private List<Plot> plotComponents = new List<Plot>();
    
    [SerializeField]
    private List<PlotData> plots = new List<PlotData>();

    public List<ItemObjectData> GetPlantedList => plotItems;
    public List<PlotData> GetPlots => plots;

    private void Awake()
    {
        RefreshPlotCache();
    }

    /// <summary>
    /// 하이러키의 플롯 오브젝트들을 수집하고 캐싱합니다.
    /// </summary>
    private void RefreshPlotCache()
    {
        // 자식 Plot 컴포넌트 수집
        plotComponents = new List<Plot>(this.GetComponentsInChildren<Plot>());
        
        plots = new List<PlotData>();
        foreach (var plot in plotComponents)
        {
            plots.Add(plot.GetSaveData());
        }

        // 데이터 리스트 초기화 (개수는 플롯 개수에 맞춤)
        if (plotItems.Count != plotComponents.Count)
        {
            plotItems = new List<ItemObjectData>(new ItemObjectData[plotComponents.Count]);
        }
        
        if (_data != null)
            _data.SetItemList(plotItems);
    }

    public override void Load(SaveDatas saveDatas)
    {
        // 1. 아이템 데이터 초기화 (List<ItemObjectData>로 직접 로드)
        base.Initialize(this, saveDatas.GetPlotItemData, null, ref plotItems);
        
        // 2. 플롯 상태 데이터 복구
        plotComponents = new List<Plot>(this.GetComponentsInChildren<Plot>());
        var loadedPlots = saveDatas.GetPlotData;

        for (int i = 0; i < plotComponents.Count; i++)
        {
            if (i < loadedPlots.Count)
            {
                plotComponents[i].LoadFromData(loadedPlots[i]);
                // 시각적 데이터 업데이트가 필요하다면 여기서 호출
            }
        }
        
        plots = loadedPlots;
    }

    public void SyncItemState()
    {
        // 데이터 리스트를 SaveData에 반영
        if (_data != null)
            _data.SetItemList(plotItems);

        // 플롯 상태(수분, 비료 등) 데이터 동기화
        plots.Clear();
        foreach (var plot in plotComponents)
        {
            plots.Add(plot.GetSaveData());
        }
    }

    public void AfterHarvest()
    {
        base.ResetData();
        // 데이터 리스트 초기화
        for (int i = 0; i < plotItems.Count; i++)
            plotItems[i] = default;
            
        RefreshPlotCache();
    }

    public void GrowthPlant()
    {
        for (int i = 0; i < plotItems.Count; i++)
        {
            ItemObjectData plant = plotItems[i];
            
            if (plant.GetItemID == 0) continue; // 빈 공간 제외

            // 기간 감소
            short newDuration = (short)(plant.GetDuration - 1);
            plant.SetDuration(newDuration);

            // 성장 완료 시 ID 변경 (다음 단계로)
            if (newDuration <= 0)
            {
                // TODO: 성장 테이블(CSV)을 참조하도록 개선 필요
                plant.SetItemID((ushort)(plant.GetItemID + 1));
            }
            
            plotItems[i] = plant; // struct이므로 다시 대입
            
            // 자식 Plot 오브젝트의 시각적 상태 업데이트 (필요 시)
            if (i < plotComponents.Count)
            {
                // Plot 컴포넌트가 데이터를 받아 표현하도록 설계되어야 함
                // plotComponents[i].UpdateVisual(plant); 
            }
        }
        
        if (_data != null)
            _data.SetItemList(plotItems);
    }
}
