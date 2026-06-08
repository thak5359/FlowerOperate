using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using static Constant;
using R3;


[MemoryPackable]
[Serializable]
public partial struct PlotData : IPropData // 저장용 데이터 바구니
{
    public Vector3 Position { get; private set; }
    public int ItemId { get; set; }
    public FlowerGrowth Growth { get; set; }
    public FlowerState State { get; set; }
    public FlowerGrade Grade { get; set; }

    public int4 GrowthDays { get; set; }

    public int harvestAmount { get; set; }
    public bool IsAppliedBountyFert { get; private set; }

    [field: SerializeField] public FertilizerGrade AppliedQualityFert { get; set; }

    /// <summary>
    /// 로드용 생성자
    /// </summary>
    public PlotData(Vector3 input_pos, int input_flowerId,
        FlowerGrade input_grade, FlowerGrowth input_growth, FlowerState input_state,
        FertilizerType input_fertilizerType, FertilizerGrade input_fertilizerGrade,
        int input_BountyAmount, bool input_IsAppliedBountyFert, int4 input_growthDays)
    {
        Position = input_pos;
        ItemId = input_flowerId;
        Grade = input_grade;

        Growth = input_growth;
        State = input_state;

        AppliedQualityFert = input_fertilizerGrade;
        harvestAmount = input_BountyAmount;
        IsAppliedBountyFert = input_IsAppliedBountyFert;

        GrowthDays = input_growthDays;
    }


    /// <summary>
    ///  기본 생성자.
    /// </summary>
    public PlotData(int input_flowerId)
    {
        Position = default;
        ItemId = input_flowerId;
        Growth = FlowerGrowth.Unknown;
        State = FlowerState.Vivid;
        Grade = FlowerGrade.Unknown;
        harvestAmount = 0;
        IsAppliedBountyFert = false;
        AppliedQualityFert = FertilizerGrade.Unknown;

        if (GlobalItemDB.HasFlower(ItemId) == true)
        {
            FlowerItemBlobData data = GlobalItemDB.GetFlowerRef(ItemId);
            harvestAmount = data.HarvestAmount;
            GrowthDays = int4.zero;
        }
        else
        {
            harvestAmount = 0;

            GrowthDays = int4.zero;
            //Debug.LogAssertion($"PlotData Constructor Error. itemID : {Id}");
        }
    }

    public void SetPosition(Vector3 input_position) => Position = input_position;

    /// <summary>
    /// FlowerItem을 기반으로 내부 데이터를 덮어씁니다.
    /// </summary>
    public FarmActionResult Sowing(in FlowerItem seed)
    {
        if (seed.SubType != ItemSubType.Seed)
        {
            FixedString128Bytes errorCode = ($" Item which one isn't Seed is sowed on Plot... item : {seed}");
            Debug.LogAssertion(errorCode);
            return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
        }

        ItemId = seed.Id + 1000;

        if (seed.Grade != FlowerGrade.Unknown)

            Grade = seed.Grade;
        else
            Grade = FlowerGrade.Lv0;

        Growth = FlowerGrowth.Seed;
        if (State != FlowerState.Moist)
        {
            State = FlowerState.Vivid;
        }

        if (GlobalItemDB.HasFlower(ItemId) == true)
        {
            FlowerItemBlobData data = GlobalItemDB.GetFlowerRef(ItemId);
            harvestAmount = data.HarvestAmount;
        }
        else
        {
            harvestAmount = 1;
        }

        int useAmount = 1;
        seed.SubCount(ref useAmount);
        if (useAmount == 1) return new FarmActionResult(FarmActionResult.ResultType.Failed, " seed amount is not enough");

        return new FarmActionResult(FarmActionResult.ResultType.Success);
    }

    public void GrowUp()
    {
        // 식물이 이미 시든 상태이거나 ITEMID_DEADCROPS 이면 불필요한 연산 없이 그대로 반환
        if (State == FlowerState.Wilted || ItemId == ITEMID_DEADCROPS)
        {
            return;
        }

        if (State == FlowerState.Moist)
        {
            // 물을 준 상태 -> 성장하고 마른 상태(Vivid)로 변경
            if (ItemId != 0 && Growth < FlowerGrowth.Bloom && ItemId != ITEMID_DEADCROPS)
            {
                Growth++;
            }
            State = FlowerState.Vivid;
        }
        else
        {
            // 물을 주지 않은 상태 -> 작물이 있다면 건조 단계 진행 (Vivid -> Dried -> Wilted)
            // 단, 밭에 심어진 것이 씨앗(Seed) 상태일 경우에는 건조 악화 및 썩음 단계를 진행하지 않음.
            if (ItemId != 0 && ItemId != ITEMID_DEADCROPS && Growth != FlowerGrowth.Seed)
            {
                if (State != FlowerState.Wilted)
                {
                    State = State.Next<FlowerState>();
                }

                // 이틀 연속 물을 주지 않으면 시든 작물로 사망
                if (State == FlowerState.Wilted)
                {
                    ItemId = ITEMID_DEADCROPS;
                }
            }
        }
    }
}

[Serializable]
public class PlotProp : Prop
{
    [SerializeField]
    public PlotData _plotData = new(0);

    public ref PlotData plotData => ref _plotData; //ref For access _plotData directly


    // 밭 더미는 base에 존재하는 스프라이트로!
    [SerializeField] public SpriteRenderer PlotTileRenderer; // 바닥 타일용 스프라이트
    [SerializeField] public SpriteRenderer FlowerSpriteRenderer; // 꽃 스프라이트 
    
    [field: SerializeField] public Sprite plotTileSprite { get; private set; }
    [field: SerializeField] public Sprite flowerSprite { get; private set; }

    private Unity.Mathematics.Random _random;

    private IDisposable disposable;

    private FlowerState lastPlotVisualState = FlowerState.Unknown;
    private int lastFlowerItemId = -1;
    private FlowerGrowth lastFlowerGrowth = FlowerGrowth.Unknown;
    private FlowerState lastFlowerState = FlowerState.Unknown;
    private bool isLoaded = false;

    private void ResetVisualCache()
    {
        lastPlotVisualState = FlowerState.Unknown;
        lastFlowerItemId = -1;
        lastFlowerGrowth = FlowerGrowth.Unknown;
        lastFlowerState = FlowerState.Unknown;
    }

    public void OnEnable()
    {
        disposable = GlobalEventManager.OnNextDayObservable.Subscribe(_ => OnNextDay().Forget()).AddTo(GlobalEventManager.disposables);
        if (plotData.Position == Vector3.zero)
        {
            plotData.SetPosition(this.transform.position);
        }

        changePlotSpr().Forget();
    }

    private void Start()
    {
        var manager = GetComponentInParent<PlotManager>();
        if (manager != null)
        {
            manager.GetPlotDataDict[this.Id] = plotData;
        }
        else
        {
            Debug.LogWarning($"[PlotProp] 부모 객체에서 PlotManager를 찾을 수 없습니다. ID: {this.Id}");
        }
        _random = new Unity.Mathematics.Random((uint)(DateTime.Now.Ticks + this.Id));

        if (!isLoaded)
        {
            QuestProgressPublisher.PlowPlot.OnNext(Unit.Default);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        disposable?.Dispose();
        disposable = null;

        if (flowerSprite != null) AddressableManager.ReleaseAsset(flowerSprite);
    }

    /// <summary>
    /// 다음 날이 되기 전에 미리 다음날에 맞춰 성장 단계를 변경한뒤 저장됩니다. (물 주기, 시듦, 성장)
    /// </summary>
    private async UniTask OnNextDay()
    {
        if (GrowUp().Result == FarmActionResult.ResultType.Success)
        {
            await changeFlowerSpr();
            if (this == null) return;
            await changePlotSpr();
        }
    }

    private FarmActionResult GrowUp()
    {
        try
        {
            // 식물이 이미 시든 상태이거나 ITEMID_DEADCROPS인 경우 성장은 안 하지만 흙은 다음 날 마르도록 처리
            if (_plotData.State == FlowerState.Wilted || _plotData.ItemId == ITEMID_DEADCROPS)
            {
                if (_plotData.State == FlowerState.Moist)
                {
                    _plotData.State = FlowerState.Vivid;
                }
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }

            if (_plotData.State == FlowerState.Moist)
            {
                // 물을 준 상태 -> 성장하고 마른 상태(Vivid)로 변경
                if (_plotData.ItemId != 0 && _plotData.Growth < FlowerGrowth.Bloom && _plotData.ItemId != ITEMID_DEADCROPS)
                {
                    _plotData.Growth++;
                }
                _plotData.State = FlowerState.Vivid;
            }
            else
            {
                // 물을 주지 않은 상태 -> 작물이 있다면 건조 단계 진행 (Vivid -> Dried -> Wilted)
                // 단, 밭에 심어진 것이 씨앗(Seed) 상태일 경우에는 건조 악화 및 썩음 단계를 진행하지 않음.
                if (_plotData.ItemId != 0 && _plotData.ItemId != ITEMID_DEADCROPS && _plotData.Growth != FlowerGrowth.Seed)
                {
                    if (_plotData.State != FlowerState.Wilted)
                    {
                        _plotData.State = _plotData.State.Next<FlowerState>();
                    }

                    // 이틀 연속 물을 주지 않으면 시든 작물로 사망
                    if (_plotData.State == FlowerState.Wilted)
                    {
                        _plotData.ItemId = ITEMID_DEADCROPS;
                    }
                }
            }
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            Debug.Log($"GrowUp Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "GROWUP_EXCEPTION");
        }
    }


    #region Method for Farming
    /// <summary>
    /// 씨를 뿌립니다.
    /// </summary>
    /// <returns></returns>
    public FarmActionResult Sowing(ref FlowerItem item)
    {
        try
        {
            if (plotData.ItemId != 0 || item.Count <= 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed); ;
            }
            else if (plotData.ItemId == 0)
            {
                FarmActionResult result = _plotData.Sowing(in item);
                if (result.Result == FarmActionResult.ResultType.Success)
                {
                    changeFlowerSpr().Forget();
                    QuestProgressPublisher.PlotSowing.OnNext(_plotData.ItemId);
                }
                return result;
            }
            else
            {
                FixedString128Bytes errorMsg = "Sowing Error : Flower ID is Unavailable value!";
                Debug.LogError(errorMsg);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorMsg);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Sowing Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "WATERING_EXCEPTION");
        }
    }
    /// <summary>
    /// 밭에 물을 줍니다
    /// </summary>
    public FarmActionResult Watering()
    {
        try
        {
            Debug.Log("Watering method is called");
            // 시든 작물이어도 흙에 물을 줄 수 있도록 허용 (이전의 Wilted / ITEMID_DEADCROPS 시 물주기 차단 조건 제거)

            if (_plotData.State != FlowerState.Moist)
            {
                Debug.Log("switching sprite...");
                _plotData.State = FlowerState.Moist;
                changePlotSpr().Forget();
                Debug.Log("switching Clear");
                QuestProgressPublisher.PlotWatering.OnNext(_plotData.ItemId);
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }
        catch (Exception e)
        {
            Debug.Log($"Watering Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "WATERING_EXCEPTION");
        }
    }
    /// <summary>
    /// 망치로 내리쳐 꽃을 파괴합니다. 꽃이 없다면 밭을 파괴합니다.
    /// </summary>
    public FarmActionResult Hammering()
    {
        try
        {
            Debug.Log("Runing has been called");
            if (plotData.ItemId == 0)
            {
                GetComponentInParent<PlotManager>().GetPlotDataDict.Remove(this.Id);
                QuestProgressPublisher.PlotHammeringPlot.OnNext(Unit.Default);
                Destroy(this.gameObject);
            }
            else
            {
                AddressableManager.ReleaseAsset(flowerSprite);
                flowerSprite = default;
                plotData.ItemId = default;
                plotData.State = FlowerState.Unknown;
                FlowerSpriteRenderer.sprite = null;

            }
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            Debug.Log($"Ruining Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "Ruining_EXCEPTION");
        }
    }

    /// <summary>
    /// 비료 LV에 따라 수확량을 늘립니다.
    /// </summary>
    public FarmActionResult ApplyBountyFertilizer(in FertilizerGrade FertLV) // 수확 개수를 증가
    {
        if (_plotData.State == FlowerState.Wilted || _plotData.ItemId == ITEMID_DEADCROPS)
        {
            return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }

        switch (FertLV)
        {
            case FertilizerGrade.Lv1:
                {
                    _plotData.harvestAmount += 1;

                    return new FarmActionResult(FarmActionResult.ResultType.Success);
                }
            case FertilizerGrade.Lv3:
                {
                    _plotData.harvestAmount += 2;
                    return new FarmActionResult(FarmActionResult.ResultType.Success);
                }
            case FertilizerGrade.Lv5:
                {
                    _plotData.harvestAmount += 3;
                    return new FarmActionResult(FarmActionResult.ResultType.Success);
                }
            case FertilizerGrade.Lv2:
            case FertilizerGrade.Lv4:
                {
                    _plotData.harvestAmount += FertLV.ToValue() + ((_random.NextFloat() < 0.5f) ? 0 : 1);
                    return new FarmActionResult(FarmActionResult.ResultType.Success);
                }
            default:
                Debug.LogError($"BountyUP Error : Invalid FertilizerGrade is {FertLV}");
                return new FarmActionResult(FarmActionResult.ResultType.Error, "INVALID_FERTILIZER_GRADE");
        }
    }

    /// <summary>
    /// 비료 LV에 따라 등급을 높입니다.
    /// </summary>
    public FarmActionResult ApplyQualityFertilizer(in FertilizerGrade FertLV)
    {
        if (_plotData.State == FlowerState.Wilted || _plotData.ItemId == ITEMID_DEADCROPS)
        {
            return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }

        if (_plotData.AppliedQualityFert != FertilizerGrade.Unknown)
        {
            return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }

        if (FertLV != FertilizerGrade.Unknown)
        {
            _plotData.AppliedQualityFert = FertLV;
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        FixedString128Bytes ErrorCode = "INVALID_FERTILIZER_GRADE";
        Debug.Log(ErrorCode);
        return new FarmActionResult(FarmActionResult.ResultType.Error, ErrorCode);
    }
    #endregion

    #region  수확 메서드 및 관련 유틸리티

    /// <summary>
    /// 다 자란 작물을 수확합니다.
    /// </summary>
    public FarmActionResult Reaping(ref GearItem gear)
    {
        try
        {
            if ( plotData.ItemId != ITEMID_DEADCROPS && plotData.Growth == FlowerGrowth.Bloom)
            {
                // 기본 5% 확률로 등급업을 시도함
                if (_random.NextFloat() < 0.05f)
                {
                    _plotData.harvestAmount++;

                }
                //5% 확률로 장비가 플래티넘 이상일 경우 다음 작물을 등급으로 올림
                if (_random.NextFloat() < 0.05f)
                {
                    if (gear.Grade.ToValue() >= 5)
                        _plotData.Grade = _plotData.Grade.Next<FlowerGrade>();
                }

               

                for (int i = 0; i < _plotData.harvestAmount; i++)
                {
                    ItemFactory.CreateItemPrefab(ItemFactory.CreateItem(_plotData.ItemId, 1, _plotData.Grade), _plotData.Position);
                }

                int harvestedItemId = _plotData.ItemId;

                _plotData.ItemId = 0;
                _plotData.Growth = FlowerGrowth.Unknown;
                _plotData.State = (_plotData.State == FlowerState.Moist) ? FlowerState.Moist : FlowerState.Vivid;
                _plotData.harvestAmount = 0;
                _plotData.AppliedQualityFert = FertilizerGrade.Unknown;

                if (flowerSprite != null)
                {
                    AddressableManager.ReleaseAsset(flowerSprite);
                    flowerSprite = null;
                }
                FlowerSpriteRenderer.sprite = null;

                changePlotSpr().Forget();
                changeFlowerSpr().Forget();

                QuestProgressPublisher.PlotReaping.OnNext(harvestedItemId);

                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            else if (plotData.State == FlowerState.Wilted || plotData.ItemId == ITEMID_DEADCROPS)
            {
                ItemFactory.CreateItemPrefab(ItemFactory.CreateItem(ITEMID_DEADCROPS, 1), _plotData.Position);

                // 시든 작물 수확 후 밭 리셋
                _plotData.ItemId = 0;
                _plotData.Growth = FlowerGrowth.Unknown;
                _plotData.State = FlowerState.Vivid;
                _plotData.harvestAmount = 0;
                _plotData.AppliedQualityFert = FertilizerGrade.Unknown;

                if (flowerSprite != null)
                {
                    AddressableManager.ReleaseAsset(flowerSprite);
                    flowerSprite = null;
                }
                FlowerSpriteRenderer.sprite = null;

                changePlotSpr().Forget();
                changeFlowerSpr().Forget();

                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }
        catch (Exception e)
        {
            Debug.Log($"Reaping Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "REAPING_EXCEPTION");
        }
    }

    /// <summary>
    /// 비료 등급에 비례한 확률로 작물 등급 상승 연산을 합니다.
    /// </summary>
    private FlowerGrade TryQualityUp()
    {
        if (_random.NextFloat() < _plotData.AppliedQualityFert.ToValue() * 0.2f)
        {
            return _plotData.Grade.Next<FlowerGrade>();
        }
        else return _plotData.Grade;
    }
    #endregion

    private async UniTask changePlotSpr()
    {
        if (this == null) return;
        Debug.Log("changePlotSpr has been called");
        bool wantWatered = (_plotData.State == FlowerState.Moist);
        FlowerState targetVisualState = wantWatered ? FlowerState.Moist : FlowerState.Vivid;

        if (lastPlotVisualState == targetVisualState && base.SpriteRenderer.sprite != null && PlotTileRenderer.sprite != null)
        {
            return;
        }

        lastPlotVisualState = targetVisualState;

        if (base.DisplaySprite != null)
            AddressableManager.ReleaseAsset(base.DisplaySprite);
        if (plotTileSprite != null)
            AddressableManager.ReleaseAsset(plotTileSprite);


        if (wantWatered)
        {
            var wateredProp = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOTPROP_WATERED);
            if (this == null) return;
            base.DisplaySprite = wateredProp;

            var wateredTile = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOTTILE_WATERED);
            if (this == null) return;
            plotTileSprite = wateredTile;
        }
        else
        {
            var defaultProp = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOTPROP_DEFAULT);
            if (this == null) return;
            base.DisplaySprite = defaultProp;

            var defaultTile = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOTTILE_DEFAULT);
            if (this == null) return;
            plotTileSprite = defaultTile;
        }

        base.SpriteRenderer.sprite = base.DisplaySprite;
        PlotTileRenderer.sprite = plotTileSprite;
    }

    private async UniTask changeFlowerSpr()
    {
        if (this == null) return;
        if (_plotData.ItemId == 0)
        {
            Debug.Log("Id is 0 but changeFlowerSpr called");
            if (flowerSprite != null)
            {
                AddressableManager.ReleaseAsset(flowerSprite);
                flowerSprite = null;
            }
            if (FlowerSpriteRenderer != null)
            {
                FlowerSpriteRenderer.sprite = null;
            }
            lastFlowerItemId = 0;
            lastFlowerGrowth = FlowerGrowth.Unknown;
            lastFlowerState = FlowerState.Unknown;
            return;
        }

        bool isWilted = (_plotData.ItemId == ITEMID_DEADCROPS || _plotData.State == FlowerState.Wilted);

        if (lastFlowerItemId == _plotData.ItemId &&
            lastFlowerGrowth == _plotData.Growth &&
            lastFlowerState == _plotData.State &&
            FlowerSpriteRenderer != null && FlowerSpriteRenderer.sprite != null)
        {
            return;
        }

        lastFlowerItemId = _plotData.ItemId;
        lastFlowerGrowth = _plotData.Growth;
        lastFlowerState = _plotData.State;

        if (flowerSprite != null)
            AddressableManager.ReleaseAsset(flowerSprite);

        Sprite loadedSprite = null;

        if (isWilted)
        {
            loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW4);
            if (this == null) return;
        }
        else
        {
            switch (_plotData.Growth)
            {
                case FlowerGrowth.Unknown:
                    loadedSprite = null;
                    break;
                case FlowerGrowth.Seed:
                    loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW0);
                    break;
                case FlowerGrowth.Sprout:
                    loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW1);
                    break;
                case FlowerGrowth.Stalk:
                    loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW2);
                    break;
                case FlowerGrowth.Bud:
                    loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW3);
                    break;
                case FlowerGrowth.Bloom:
                    ItemBaseBlobData data = GlobalItemDB.GetBaseRef(_plotData.ItemId);
                    loadedSprite = await AddressableManager.LoadAssetAsync<Sprite>(data.SpriteAddress);
                    break;
            }
            if (this == null) return;
        }

        flowerSprite = loadedSprite;

        if (FlowerSpriteRenderer != null)
        {
            FlowerSpriteRenderer.sprite = flowerSprite;
        }
    }

    public PlotData GetPlotData()
    {
        return _plotData;
    }
    public void LoadFromData(PlotData data)
    {
        isLoaded = true;
        this.transform.position = data.Position;
        _plotData = data;
        ResetVisualCache();
        changePlotSpr().Forget();
        changeFlowerSpr().Forget();
    }

    public override void OnLoadAsync(IPropData propData)
    {
        isLoaded = true;
        GetComponentInParent<PlotManager>().GetPlotDataDict.Remove(this.Id); // 기존 데이터 제거
        ResetVisualCache();
        this.plotData = (PlotData)propData;
        this.transform.position = plotData.Position;
        GetComponentInParent<PlotManager>().GetPlotDataDict[this.Id] = this.plotData;
        changePlotSpr().Forget();
        changeFlowerSpr().Forget();
    }
}