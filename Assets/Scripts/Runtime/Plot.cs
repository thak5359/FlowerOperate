using Cysharp.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using VContainer;
using static Constant;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct PlotData // 저장용 데이터 바구니
{
    public int plotId;

    public Vector3 Position;
    public bool isWatered;
    public int flowerId; // 0 == null
    public int growth;
    public bool isDried;
}

[Serializable]
public class Plot : MonoBehaviour
{


    public PlotData data = new PlotData();
    [Header("땅의 SpriteRender와 꽃의 SpriteRenderer를 드래그 해주세요! GetComponentInChildren으로는 좀 힘듦!")]
    public SpriteRenderer plotRenderer;
    public SpriteRenderer flowerRenderer;
    public int flowerId = 0;

    private Sprite plotSprite;
    private Sprite flowerSprite;

    int cachedInt; //캐싱용

    public int plotId; // 토지 고유 번호 plot Unique ID
    private int growth; // 꽃의 성장 단계 == item.level
    private int grade;
    int bonusAmount = 0; // 보너스 양   

    private bool isWatered = false;
    private bool isDried = false; // 물을 주지 않은 채 하루가 경과함.

    private void Awake()
    {
        plotId = Guid.NewGuid().GetHashCode(); // 고유한 ID 생성
    }

    private void OnEnable()
    {
        GlobalEventManager.NextDay += OnNextDay;
    }

    private void OnDisable()
    {
        if (plotSprite != null) AddressableManager.ReleaseAsset(plotSprite);
        if (flowerSprite != null) AddressableManager.ReleaseAsset(flowerSprite);
    }

    private async void OnNextDay()
    {
        if(GrowUp().Result() == FarmActionResult.ResultType.Success)
        {
            await changeFlowerSpr();
        }
    }
    #region Method for Farming
    public FarmActionResult Sowing(int seedID) // 씨뿌리기
    {
        try
        {
            if (flowerId > 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed); ;
            }
            else if (flowerId == 0)
            {
                flowerId = seedID;
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
            if (isWatered == true && growth == MAX_GROWTH)
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
            if (flowerId == 0)
            {
                Destroy(this.gameObject);
            }
            else
            {
                AddressableManager.ReleaseAsset(flowerSprite);
                flowerSprite = default;
                flowerId = 0;
                growth = 0;
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
            if (grade < 10)
            {
                grade++;
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

                if (growth < MAX_GROWTH && flowerId != 0) growth++;
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
        if (plotSprite != null)
            AddressableManager.ReleaseAsset(plotSprite);

        if (isWatered == false)
            plotSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_DEFAULT);
        else
            plotSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_PLOT_WATERED);

        plotRenderer.sprite = plotSprite;
    }

    private async UniTask changeFlowerSpr()
    {
               if (flowerSprite != null)
            AddressableManager.ReleaseAsset(flowerSprite);

        if( flowerId != 0 && growth == 0)
        {
            flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(ADDRESSABLE_SPR_FLOWER_SEED);
            flowerRenderer.sprite = flowerSprite;
        }
        else if (flowerId != 0)
        {
            FixedString128Bytes address =   GlobalItemDB.GetAddressString((short)flowerId);
            flowerSprite = await AddressableManager.LoadAssetAsync<Sprite>(address);
            flowerRenderer.sprite = flowerSprite;
        }
        else
        {
            flowerRenderer.sprite = null;
        }
    }



    public PlotData GetSaveData()
    {
        data.plotId = this.plotId;
        data.Position = this.transform.position;
        data.isWatered = this.isWatered;
        data.flowerId = this.flowerId; // null이면 0으로 저장
        data.growth = this.growth;
        data.isDried = this.isDried;

        return data;
    }
    public void LoadFromData(PlotData data)
    {
        this.plotId = data.plotId;
        this.transform.position = data.Position;
        this.isWatered = data.isWatered;
        this.flowerId = data.flowerId;
        this.growth = data.growth;
        this.isDried = data.isDried;
    }
}
