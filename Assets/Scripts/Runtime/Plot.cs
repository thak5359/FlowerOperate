using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using static Constant;

[MemoryPackable]
public partial struct PlotData // 저장용 데이터 바구니
{
    public Vector3 position { get; private set; }
    public int flowerId { get; set; }
    public FlowerGrowth Growth { get; set; }
    public FlowerState State { get; set; }
    public int grade { get; set; }
    public int bonusAmount { get; set; }


    public PlotData(Vector3 input_pos, int input_flowerId, int input_grade, int input_bonusAmount, FlowerGrowth input_growth, FlowerState input_state)
    {
        position = input_pos;
        flowerId = input_flowerId;
        grade = input_grade;

        Growth = input_growth;
        State = input_state;
        bonusAmount = input_bonusAmount;
    }

    public PlotData(Vector3 input_pos)
    {
        position = input_pos;
        flowerId = 0;
        grade = 0;
        Growth = FlowerGrowth.Unknown;
        State = FlowerState.Unknown;
        bonusAmount = 0;
    }
    public PlotData(int input_flowerId)
    {
        position = default;
        flowerId = input_flowerId;
        grade = 0;

        Growth = FlowerGrowth.Unknown;
        State = FlowerState.Unknown;
        bonusAmount = 0;
    }
    public void SetPosition(Vector3 input_position) => position = input_position;
}

[Serializable]
public class Plot : Prop, IGameResource
{
    private PlotData _plotData = new(0);
    public ref PlotData plotData => ref _plotData;

    [SerializeField] public SpriteRenderer FlowerSpriteRenderer;

    [field: SerializeField] public Sprite flowerSprite { get; private set; }

    int bonusAmount = 0; // 보너스 양   

    private bool isWatered = false;
    private bool isDried = false; // 물을 주지 않은 채 하루가 경과함.

    public void OnEnable()
    {
        GlobalEventManager.NextDay += OnNextDay;

        plotData.SetPosition(this.transform.position);
        plotData.State = FlowerState.Vivid;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GlobalEventManager.NextDay -= OnNextDay;

        if (flowerSprite != null) AddressableManager.ReleaseAsset(flowerSprite);
    }

    private async void OnNextDay()
    {
        if (GrowUp().Result() == FarmActionResult.ResultType.Success)
        {
            await changeFlowerSpr();
        }
    }
    #region Method for Farming
    public FarmActionResult Sowing(int seedID) // 씨뿌리기
    {
        try
        {
            if (plotData.flowerId > 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed); ;
            }
            else if (plotData.flowerId == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Success);
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

    public FarmActionResult Watering() // 물 주기
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
    public FarmActionResult Reaping() // 수확
    {
        try
        {
            if (isWatered == true && plotData.Growth == FlowerGrowth.Bloom)
            {
                //TODO:: 인스턴스 만들고 자멸하는 함수 넣기
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
    public FarmActionResult Ruining() // 파멸의 일격!!
    {
        try
        {
            Debug.Log("Runing has been called");
            if (plotData.flowerId == 0)
            {
                Destroy(this.gameObject);
            }
            else
            {
                AddressableManager.ReleaseAsset(flowerSprite);
                flowerSprite = default;
                plotData.flowerId = default;
                plotData.State = FlowerState.Unknown;
            }
            return new FarmActionResult(FarmActionResult.ResultType.Success);
        }
        catch (Exception e)
        {
            Debug.Log($"Ruining Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "Ruining_EXCEPTION");
        }
    }
    public FarmActionResult QualityUp() // 등급 업
    {
        try
        {
            if (plotData.grade < 10)
            {
                plotData.grade++;
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            else
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }
        catch (Exception e)
        {
            Debug.Log($"QualityUp Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "QUALITYUP_EXCEPTION");
        }
    }
    public FarmActionResult BountyUP(int increaseAmount) // 수확 개수를 증가
    {
        try
        {
            if (bonusAmount != 0)
            {
                bonusAmount += increaseAmount;
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            else
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
        }
        catch (Exception e)
        {

            Debug.Log($"BountyUP Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "BOUNTYUP_EXCEPTION");
        }
    }
    private FarmActionResult GrowUp()
    {
        try
        {
            if (isWatered == true)
            {
                changePlotSpr().Forget();
                isWatered = false;
                isDried = false;

                if (plotData.Growth < FlowerGrowth.Bloom && plotData.flowerId != 0) plotData.Growth++;
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
    #endregion
    private async UniTask changePlotSpr()
    {
        Debug.Log("changePlotSpr has been called");
        if (base.PropSprite != null)
            AddressableManager.ReleaseAsset(base.PropSprite);

        if (isWatered == true)
            base.PropSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_WATERED);
        else
            base.PropSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_DEFAULT);

        base.SpriteRenderer.sprite = base.PropSprite;
    }
    private async UniTask changeFlowerSpr()
    {
        if (flowerSprite != null)
            AddressableManager.ReleaseAsset(flowerSprite);

        if (_plotData.flowerId != 0 && _plotData.Growth == FlowerGrowth.Unknown)
        {
            flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_FLOWER_SEED);
            FlowerSpriteRenderer.sprite = flowerSprite;
        }
        else if (_plotData.flowerId != 0)
        {
            FixedString128Bytes address = GlobalItemDB.GetAddressString((short)_plotData.flowerId);
            flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(address);
            FlowerSpriteRenderer.sprite = flowerSprite;
        }
        else
        {
            FlowerSpriteRenderer.sprite = null;
        }
    }
    public PlotData GetSaveData()
    {
        return _plotData;
    }
    public void LoadFromData(PlotData data)
    {
        this.transform.position = data.position;
        _plotData = data;
    }
}