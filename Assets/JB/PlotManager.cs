using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : ItemStorageParent
{
    [SerializeField]
    private List<ItemDataContainer> plots;
    public void AfterHarvest()
    {
        base.Initialize();
        plots = new List<ItemDataContainer>(this.GetComponentsInChildren<ItemDataContainer>());
        _data.SetItemList(LoadChangedDataList(plots));
    }

    public void GrowthPlant()
    {
        foreach (ItemObjectData plant in _data.GetList)
        {
            plant.SetDuration((short)(plant.GetDuration - 1));

            if (plant.GetDuration == 0)
                plant.SetItemID((ushort)(plant.GetItemID + 1));

        }
    }
}
