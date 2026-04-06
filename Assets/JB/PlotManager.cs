using Cysharp.Threading.Tasks;
using Fungus;
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
        plotItems = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
        plots = new List<PlotData>();
        foreach (var item in this.GetComponentsInChildren<Plot>())
            plots.Add(item.GetSaveData());

        _data.SetItemList(plotItems.ConvertAll(item => item.GetData));
    }
    
    private void OnEnable()
    {
        SaveLoadManager.OnLoadData += Load;
    }

    private void OnDisable()
    {
        SaveLoadManager.OnLoadData -= Load;
    }

    private void Load(SaveDatas saveDatas)
    {
        base.Initialize(StorageType.PLOT, saveDatas.GetPlotItemData, null, ref plotItems);
        plots = saveDatas.GetPlotData;
        for (int i = 0; i < plots.Count; i++)
            foreach (var item in this.GetComponentsInChildren<Plot>())
                item.LoadFromData(plots[i]);
    }

    public void AfterHarvest()
    {
        base.ResetData();
        plotItems = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
        foreach (var item in this.GetComponentsInChildren<Plot>())
            plots.Add(item.GetSaveData());
    }

    public void GrowthPlant()
    {
        foreach (ItemObjectData plant in _data.GetList)
        {
            //수정 요망
            plant.SetDuration((short)(plant.GetDuration - 1));

            if (plant.GetDuration == 0)
                plant.SetItemID((ushort)(plant.GetItemID + 1));

        }
    }
}
