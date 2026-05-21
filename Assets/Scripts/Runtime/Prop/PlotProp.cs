using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using static Constant;
using Fungus.EditorUtils;


[MemoryPackable]
[Serializable]
public partial struct PlotData : IPropData // 저장용 데이터 바구니
{
    public Vector3 Position { get; private set; }
    public int Id { get; set; }
    public FlowerGrowth Growth { get; set; }
    public FlowerState State { get; set; }
    public FlowerGrade Grade { get; set; }

    public int4 GrowthDays;

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
        Id = input_flowerId;
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
    public PlotData(int input_flowerId )
    {
        Position = default;
        Id = input_flowerId;
        Growth = FlowerGrowth.Unknown;
        State = FlowerState.Vivid;
        Grade = FlowerGrade.Unknown;
        harvestAmount = 0;
        IsAppliedBountyFert = false;
        AppliedQualityFert = FertilizerGrade.Unknown;

        if (GlobalItemDB.HasFlower(Id) == true)
        {
            FlowerItemBlobData data = GlobalItemDB.GetFlowerRef(Id);
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

        Id = seed.Id;

        if (seed.Grade != FlowerGrade.Unknown)

            Grade = seed.Grade;
        else
            Grade = FlowerGrade.Lv0;

        Growth = FlowerGrowth.Seed;
        State = FlowerState.Vivid;

        return new FarmActionResult(FarmActionResult.ResultType.Success);
    }
}

[Serializable]
public class PlotProp : Prop
{
    [SerializeField]
    public PlotData _plotData = new(0);

    public ref PlotData plotData => ref _plotData; //ref For access _plotData directly

    [SerializeField] public SpriteRenderer FlowerSpriteRenderer;
    [field: SerializeField] public Sprite flowerSprite { get; private set; }

    private Unity.Mathematics.Random _random;

    private bool isWatered = false;
    private bool isDried = false; // 물을 주지 않은 채 하루가 경과함.

    public void OnEnable()
    {
        GlobalEventManager.NextDay += OnNextDay;
        plotData.SetPosition(this.transform.position);
    }

    private void Start()
    {
        PlotManager.Instance.GetPlotDataDict.Add(this.Id, plotData);
        _random = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GlobalEventManager.NextDay -= OnNextDay;

        if (flowerSprite != null) AddressableManager.ReleaseAsset(flowerSprite);
    }

    /// <summary>
    /// 다음 날이 되기 전에 미리 다음날에 맞춰 성장 단계를 변경한뒤 저장됩니다. (물 주기, 시듦, 성장)
    /// </summary>
    private async void OnNextDay()
    {
        if (GrowUp().Result() == FarmActionResult.ResultType.Success)
        {
            await changeFlowerSpr();
        }
    }

    private FarmActionResult GrowUp()
    {
        try
        {
            if (isWatered == true)
            {
                changePlotSpr().Forget();
                _plotData.State = _plotData.State.Next<FlowerState>();
                isWatered = false;

                isDried = false;

                if (plotData.Growth < FlowerGrowth.Bloom && plotData.Id != 0) plotData.Growth++;
            }
            else
            {
                if (isDried == true)
                {
                    // 시든 꽃 ID 로 변경함.
                }
                else
                    isDried = true;
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
            if (plotData.Id > 0 || item.Count <= 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed); ;
            }
            else if (plotData.Id == 0)
            {
                FarmActionResult result = _plotData.Sowing(in item);
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
            if (isWatered == false)
            {
                Debug.Log("switching sprite...");
                isWatered = true;
                changePlotSpr().Forget();
                Debug.Log("switching Clear");
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
            if (plotData.Id == 0)
            {
                Destroy(this.gameObject);
            }
            else
            {
                AddressableManager.ReleaseAsset(flowerSprite);
                flowerSprite = default;
                plotData.Id = default;
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


            if (_random.NextFloat() < 0.05f)
            {
                _plotData.harvestAmount++;
                if (gear.Grade.ToValue() >= 5)
                    _plotData.Grade = _plotData.Grade.Next<FlowerGrade>();
            }


            if (isWatered == true && plotData.Growth == FlowerGrowth.Bloom)
            {
                for (int i = 0; i < _plotData.harvestAmount; i++)
                {


                    ItemFactory.CreateItemPrefab(new FlowerItem(_plotData.Id, 1, TryQualityUp()), _plotData.Position);
                }
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            else
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
        Debug.Log("changePlotSpr has been called");
        if (base.DisplaySprite != null)
            AddressableManager.ReleaseAsset(base.DisplaySprite);

        if (isWatered == true)
            base.DisplaySprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_WATERED);
        else
            base.DisplaySprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_DEFAULT);

        base.SpriteRenderer.sprite = base.DisplaySprite;
    }

    private async UniTask changeFlowerSpr()
    {
        if (_plotData.Id == 0)
        {
            Debug.Log("Id is 0 but changeFlowerSpr called");
            return;
        }

        if (flowerSprite != null)
            AddressableManager.ReleaseAsset(flowerSprite);

        switch (_plotData.Growth)
        {
            case FlowerGrowth.Unknown:
                flowerSprite = null;
                break;
            case FlowerGrowth.Seed:
                flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW0);
                break;
            case FlowerGrowth.Sprout:
                flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW1);
                break;
            case FlowerGrowth.Stalk:
                flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_GROW2);
                break;
            case FlowerGrowth.Bloom:
                ItemBaseBlobData data = GlobalItemDB.GetBaseRef(_plotData.Id);
                flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(data.SpriteAddress);
                break;
        }
    }

    public PlotData GetPlotData()
    {
        return _plotData;
    }
    public void LoadFromData(PlotData data)
    {
        this.transform.position = data.Position;
        _plotData = data;
    }

    public override void OnLoadAsync(IPropData propData)
    {
        PlotManager.Instance.GetPlotDataDict.Remove(this.Id); // 기존 데이터 제거
        base.OnLoadAsync(propData);
        this.plotData = (PlotData)propData;
        this.transform.position = plotData.Position;
    }
}