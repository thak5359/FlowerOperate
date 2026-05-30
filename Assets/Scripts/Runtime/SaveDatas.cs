using AYellowpaper.SerializedCollections;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[MemoryPackable]
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial class SaveDatas
{
    [MemoryPackInclude, SerializeField] private string saveTime;
    [MemoryPackInclude, SerializeField] private int playDay;
    [MemoryPackInclude, SerializeField] private int money;
    [MemoryPackInclude, SerializeField] private int reputation;

    [MemoryPackInclude, SerializeField] private ItemInstantData itemData;

    [MemoryPackInclude, SerializeField] private List<PlotSaveEntry> plotDataList = new();
    [MemoryPackInclude, SerializeField] private ChunkDataIngame[] _FarmChunkDatas;
    [MemoryPackInclude, SerializeField] private ChunkDataIngame[] _FieldChunkDatas;
    [MemoryPackInclude, SerializeField] private ChunkDataIngame[] _ForestChunkDatas;
    [MemoryPackInclude, SerializeField] private ChunkDataIngame[] _MineChunkDatas;

    [MemoryPackIgnore]
    private SerializedDictionary<int, PlotData> plotDataDictCache;

    public string GetSaveTime => saveTime;
    public int GetPlayDay => playDay;
    public ItemInstantData GetItemData => itemData;

    public int GetMoney => money;
    public ref int GetRefMoney => ref money;
    public int GetReputation => reputation;

    [MemoryPackIgnore]
    public ref SerializedDictionary<int, PlotData> GetRefPlotData
    {
        get
        {
            if (plotDataDictCache == null)
                plotDataDictCache = ToSerializedDictionary(plotDataList);

            return ref plotDataDictCache;
        }
    }

    [MemoryPackIgnore]
    public SerializedDictionary<int, PlotData> GetPlotData
    {
        get
        {
            if (plotDataDictCache == null)
                plotDataDictCache = ToSerializedDictionary(plotDataList);

            return plotDataDictCache;
        }
    }

    [MemoryPackConstructor]
    public SaveDatas()
    {
    }

    public SaveDatas(
        int day,
        ItemInstantData itemData,
        SerializedDictionary<int, PlotData> plotData,
        int money = 0,
        int reputation = 0,
        ChunkDataIngame[] farmChunkDatas = null,
        ChunkDataIngame[] fieldChunkDatas = null,
        ChunkDataIngame[] forestChunkDatas = null,
        ChunkDataIngame[] mineChunkDatas = null
    )
    {
        saveTime = DateTime.Now.ToString("yyyy/MM/dd \n HH : mm");
        playDay = day;
        this.itemData = itemData;
        this.money = money;
        this.reputation = reputation;
        plotDataList = FromSerializedDictionary(plotData);
        plotDataDictCache = plotData;
        _FarmChunkDatas = farmChunkDatas;
        _FieldChunkDatas = fieldChunkDatas;
        _ForestChunkDatas = forestChunkDatas;
        _MineChunkDatas = mineChunkDatas;
    }

    private static List<PlotSaveEntry> FromSerializedDictionary(
        SerializedDictionary<int, PlotData> source
    )
    {
        List<PlotSaveEntry> result = new();

        if (source == null)
            return result;

        foreach (var pair in source)
        {
            result.Add(new PlotSaveEntry(pair.Key, pair.Value));
        }

        return result;
    }

    private static SerializedDictionary<int, PlotData> ToSerializedDictionary(
        List<PlotSaveEntry> source
    )
    {
        SerializedDictionary<int, PlotData> result = new();

        if (source == null)
            return result;

        foreach (PlotSaveEntry entry in source)
        {
            result[entry.Key] = entry.Value;
        }

        return result;
    }
}

[Serializable]
public partial struct PlotSaveEntry
{
    public int Key;
    public PlotData Value;

    [MemoryPackConstructor]
    public PlotSaveEntry(int key, PlotData value)
    {
        Key = key;
        Value = value;
    }
}