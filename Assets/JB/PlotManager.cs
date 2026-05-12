using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;

public class PlotManager : MonoBehaviour
{
    // [플롯ID : 플롯데이터] 꼴의 해시테이블. 아이디로 플롯이 담고 있는 데이터에 접근할 수 있음
    [SerializedDictionary("PlotID", "PlotData")]
    [SerializeField] 
    private SerializedDictionary<int, PlotData> plotDataDict;

    [SerializeField]
    private GameObject plotPrefab;
    public SerializedDictionary<int, PlotData> GetPlotDataDict => this.plotDataDict;

    public static PlotManager Instance { get; private set; }

    private void Awake()
    {
        // 수정할 위치: 싱글톤 로직 추가
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // 기존 코드
        RefreshPlotCache();
    }

    /// <summary>
    /// 하이러키의 플롯 오브젝트들을 수집하고 캐싱합니다.
    /// </summary>
    private void RefreshPlotCache()
    {
        foreach (var plot in this.GetComponentsInChildren<PlotProp>())
        {
            plotDataDict.Add(plot.Id, plot.GetSaveData());
        }
    }

    public void Load(SaveDatas saveDatas)
    {
        // 2. 플롯 상태 데이터 복구
        ref var loadedPlots = ref saveDatas.GetRefPlotData;

        foreach(var data in loadedPlots)
        {
            var plot = Instantiate(plotPrefab, this.transform);
            plot.GetComponent<PlotProp>().LoadFromData(data.Value);
        }
    }

    public void SyncItemState()
    {
        plotDataDict.Clear();
        RefreshPlotCache();
    }

}
