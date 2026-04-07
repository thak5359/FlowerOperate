using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : ItemStorageParent
{
    [SerializeField]
    private List<ItemDataContainer> plotItems;
    [SerializeField]
    private List<PlotData> plots;

    public List<ItemDataContainer> GetPlantedIist => plotItems;
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
        plotItems = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
        
        var plotComponents = this.GetComponentsInChildren<Plot>();
        plots = new List<PlotData>();
        foreach (var plot in plotComponents)
        {
            plots.Add(plot.GetSaveData());
        }

        // 현재 아이템 데이터를 리스트화하여 동기화
        List<ItemObjectData> itemDatas = new List<ItemObjectData>();
        foreach (var item in plotItems)
        {
            itemDatas.Add(item.GetData);
        }
        _data.SetItemList(itemDatas);
    }

    public override void Load(SaveDatas saveDatas)
    {
        // 1. 아이템 데이터 초기화
        base.Initialize(this, saveDatas.GetPlotItemData, null, ref plotItems);
        
        // 2. 플롯 상태 데이터 복구 (인덱스 기반 매칭으로 성능 개선)
        var plotComponents = this.GetComponentsInChildren<Plot>();
        var loadedPlots = saveDatas.GetPlotData;

        for (int i = 0; i < plotComponents.Length; i++)
        {
            if (i < loadedPlots.Count)
            {
                plotComponents[i].LoadFromData(loadedPlots[i]);
            }
        }
        
        plots = loadedPlots;
    }
        
    public void SyncItemState()
    {
        // 각 플롯의 최신 아이템 및 상태 데이터를 갱신
        List<ItemObjectData> itemDatas = new List<ItemObjectData>();
        foreach (var item in plotItems)
        {
            itemDatas.Add(item.GetData);
        }
        _data.SetItemList(itemDatas);

        var plotComponents = this.GetComponentsInChildren<Plot>();
        plots.Clear();
        foreach (var plot in plotComponents)
        {
            plots.Add(plot.GetSaveData());
        }
    }

    public void AfterHarvest()
    {
        base.ResetData();
        RefreshPlotCache();
    }

    public void GrowthPlant()
    {
        var plantList = _data.GetList;
        for (int i = 0; i < plantList.Count; i++)
        {
            ItemObjectData plant = plantList[i];
            
            if (plant.GetItemID == 0) continue; // 빈 공간 제외

            // 기간 감소
            short newDuration = (short)(plant.GetDuration - 1);
            plant.SetDuration(newDuration);

            // 성장 완료 시 ID 변경 (다음 단계로)
            if (newDuration <= 0)
            {
                // TODO: 성장 테이블(CSV)을 참조하도록 개선 필요. 현재는 임시로 ID+1
                plant.SetItemID((ushort)(plant.GetItemID + 1));
                // plant.SetDuration(GetDefaultDurationFromCSV(plant.GetItemID));
            }
            
            plantList[i] = plant; // struct이므로 다시 대입
        }
        
        // UI 갱신
        for (int i = 0; i < plotItems.Count; i++)
        {
            plotItems[i].SetData(plantList[i]);
        }
    }
}
