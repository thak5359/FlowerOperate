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
    public SerializedDictionary<int, PlotData> getPlotDataDict => this.plotDataDict;

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
        foreach (var plot in this.GetComponentsInChildren<Plot>())
        {
            plotDataDict.Add(plot.plotId, plot.GetSaveData());
        }
    }

    public void Load(SaveDatas saveDatas)
    {
        // 2. 플롯 상태 데이터 복구
        var loadedPlots = saveDatas.GetPlotData;
        plotDataDict = loadedPlots;

        int[] id = plotDataDict.Keys.ToArray();

        for (int i = 0; i < loadedPlots.Count; i++)
        {
            var plot = Instantiate(plotPrefab, this.transform);
            plot.GetComponent<Plot>().LoadFromData(loadedPlots[id[i]]);
        }
    }

    public void SyncItemState()
    {
        plotDataDict.Clear();
        RefreshPlotCache();
    }

    //public void GrowthPlant()
    //{
    //    for (int i = 0; i < plotItems.Count; i++)
    //    {
    //        ItemObjectData plant = plotItems[i];

    //        if (plant.GetItemID == 0) continue; // 빈 공간 제외

    //        // 기간 감소
    //        short newDuration = (short)(plant.GetDuration - 1);
    //        plant.SetDuration(newDuration);

    //        // 성장 완료 시 ID 변경 (다음 단계로)
    //        if (newDuration <= 0)
    //        {
    //            // TODO: 성장 테이블(CSV)을 참조하도록 개선 필요
    //            plant.SetItemID((ushort)(plant.GetItemID + 1));
    //        }

    //        plotItems[i] = plant; // struct이므로 다시 대입

    //        // 자식 Plot 오브젝트의 시각적 상태 업데이트 (필요 시)
    //        if (i < plotComponents.Count)
    //        {
    //            // Plot 컴포넌트가 데이터를 받아 표현하도록 설계되어야 함
    //            // plotComponents[i].UpdateVisual(plant); 
    //        }
    //    }

    //    if (_data != null)
    //        _data.SetItemList(plotItems);
    //}
}
