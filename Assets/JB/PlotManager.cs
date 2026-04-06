using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : ItemStorageParent
{
    [SerializeField]
    private List<ItemObjectData> plotItems;
    [SerializeField]
    private List<Plot> plots;
    public void AfterHarvest()
    {
        base.Initialize();
        plotItems = new List<ItemObjectData>(this.GetComponentsInChildren<ItemObjectData>());
        _data.SetItemList(plotItems);
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
