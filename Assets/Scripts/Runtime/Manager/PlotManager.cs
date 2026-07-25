using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;

public class PlotManager : MonoBehaviour
{
    // [플롯ID : 플롯데이터] 꼴의 해시테이블. 아이디로 플롯이 담고 있는 데이터에 접근할 수 있음
    [SerializedDictionary("PlotID", "PlotData")]
    [SerializeField] 
    private SerializedDictionary<int, PlotData> plotDataDict = new();

    [SerializeField]
    private GameObject plotPrefab;
    public SerializedDictionary<int, PlotData> GetPlotDataDict => this.plotDataDict;

    [Inject]private ISaveLoadManager _saveLoadManager;

    private DisposableBag disposableBag = new();

    private void Awake()
    {
        GlobalEventManager.OnNextDayObservable.Subscribe(_ => SyncItemState()).AddTo(ref disposableBag);

        RefreshPlotCache();
    }

    private void Start()
    {
        LoadSaveDataAfterDBInit().Forget();
    }

    private async UniTaskVoid LoadSaveDataAfterDBInit()
    {
        await UniTask.WaitUntil(() => GlobalItemDB.IsInitialized);
        Load(_saveLoadManager.GetSaveDatas);
    }

    /// <summary>
    /// 하이러키의 플롯 오브젝트들을 수집하고 캐싱합니다.
    /// </summary>
    private void RefreshPlotCache()
    {
        foreach (var plot in this.GetComponentsInChildren<PlotProp>())
        {
            // 0609 15시 밭에 심은 작물이 없다면 다음날에 자동 삭제
            if(plot._plotData.ItemId == 0)
            continue;

            
            plotDataDict[plot.Id] = plot.GetPlotData();
        }
    }

    public void Load(SaveDatas saveDatas)
    {
        if (saveDatas == null)
        {
            Debug.LogWarning("PlotManager: saveDatas is null. Skip loading.");
            return;
        }

        // 1. 기존에 배치되어 있는 PlotProp 들을 모두 비활성화 및 파괴 (Start 호출 오염 방지)
        var existingPlots = this.GetComponentsInChildren<PlotProp>();
        foreach (var plot in existingPlots)
        {
            plot.gameObject.SetActive(false);
            Destroy(plot.gameObject);
        }
        plotDataDict.Clear();

        // 2. 플롯 상태 데이터 복구
        ref var loadedPlots = ref saveDatas.GetRefPlotData;
        if (loadedPlots == null) return;

        // 어드레서블을 통해 흙더미 프리팹을 동기적으로 로드
        var prefab = AddressableManager.LoadAssetSync<GameObject>(Constant.ADDRESSABLE_PLOT);
        if (prefab == null)
        {
            Debug.LogError("PlotManager: Failed to load ADDRESSABLE_PLOT prefab via AddressableManager!");
            return;
        }

        foreach(var data in loadedPlots)
        {
            var plot = Instantiate(prefab, this.transform);
            var plotComponent = plot.GetComponent<PlotProp>();

            plotComponent.SetId(data.Key); // 고유 ID 복원
            // 수정 위치: 비동기 로드 계약의 예외가 유실되지 않도록 UniTask로 실행해요.
            plotComponent.OnLoadAsync(data.Value).Forget(); // 상태 로드
        }
    }

    public void SyncItemState()
    {
        plotDataDict.Clear();
        RefreshPlotCache();
        _saveLoadManager.SyncSaveData(GetPlotDataDict);
    }
}
