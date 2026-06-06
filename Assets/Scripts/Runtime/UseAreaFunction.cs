using System;
using Unity.Collections;
using UnityEngine;
using static Constant;

# region interface for each tool function 

public interface IUseAreaHoeFunc
{
    FarmActionResult DoHoeFunc(ref GearItem gear, PlotManager plotManager, GameObject plot);
}
public interface IUseAreaWateringCanFunc
{
    FarmActionResult DoWateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    FarmActionResult DoHammerFunc(ref GearItem gear);
}
public interface IUseAreaSickleFunc
{
    FarmActionResult DoSickleFunc(ref GearItem gear);
}
public interface IUseAreaAxeFunc
{
    FarmActionResult DoAxeFunc(ref GearItem gear);
}
public interface IUseAreaConsumableFunc
{
    FarmActionResult DoSeedFunc(ref GameItem item);
    FarmActionResult DoFertilizerFunc(ref FertilizerItem item);
}

#endregion

public struct FarmActionResult
{
    public enum ResultType { Success, Failed, Error }

    private ResultType result;
    public ResultType Result => result;

    public FixedString128Bytes errorMessage { get; private set; }

    public FarmActionResult(ResultType input_result, FixedString128Bytes input_errorCode = default)
    {
        result = input_result;
        errorMessage = input_errorCode;
    }

    public void Combine(FarmActionResult resultB)
    {
        if (resultB.Result == ResultType.Error)
        {
            result = ResultType.Error;
            errorMessage = resultB.errorMessage;
        }
        else if (resultB.Result == ResultType.Failed && result != ResultType.Error)
        {
            result = ResultType.Failed;
        }
    }
}




public class UseAreaFunction : MonoBehaviour,
    IUseAreaAxeFunc, IUseAreaHoeFunc, IUseAreaWateringCanFunc,
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConsumableFunc
{
    static readonly uint RandSeed = (uint)DateTime.Now.Ticks;



    private static int _hoeMask;
    private static int _treatMask;
    private static int _hammerMask;
    private static int _sickleMask;
    private static int _axeMask;

    private readonly Vector3 _smallBox = new Vector3(0.1f, 0.1f, 0.1f);

    private int cachedItemLevel;
    private int cachedBountyAmount;

    // TODO :: 아이템 사용 성공/ 실패 구조체 추가

    private Collider[] GetHits(int mask)
    {
        return Physics.OverlapBox(transform.position, _smallBox, Quaternion.identity, mask);
    }


    private void Awake()
    {
        _hoeMask = LayerMask.GetMask(LAYER_PLOT, LAYER_OBSTACLE, LAYER_TREE, LAYER_INTERACTABLE, LAYER_ORE);
        _treatMask = LayerMask.GetMask(LAYER_PLOT);
        _axeMask = LayerMask.GetMask(LAYER_TREE);
        _sickleMask = LayerMask.GetMask(LAYER_PLOT, LAYER_GRASS);
        _hammerMask = LayerMask.GetMask(LAYER_ORE, LAYER_PLOT);
    }

    #region  gameItem기반 동작 함수 분리.
    /// <summary>
    /// 게임 아이템의 분류에 따라 기능을 분리하는 함수. 장비는 별도 함수로 분리.
    /// </summary>
    public FarmActionResult FireFunc(ref GameItem gameItem, PlotManager plotManager, GameObject plot = null)
    {

        switch (gameItem.SubType)
        {
            case ItemSubType.Equipment:
                if (gameItem is GearItem gearItem)
                    return FireGearFunc(ref gearItem, plotManager, plot);
                else
                {
                    FixedString128Bytes errorCode =
                        $"FireFunc error. Item is not GearItem. itemId : {gameItem.Id}, SubType : {gameItem.SubType}";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                }

            case ItemSubType.Fertilizer:
                {
                    if (gameItem is FertilizerItem fertItem)
                        return ((IUseAreaConsumableFunc)this).DoFertilizerFunc(ref fertItem);
                    else
                    {
                        FixedString128Bytes errorCode =
                            $"FireFunc error. Item is not FertilizerItem. itemId : {gameItem.Id}, SubType : {gameItem.SubType}";
                        Debug.Log(errorCode);
                        return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                    }
                }

            case ItemSubType.Seed:
                return ((IUseAreaConsumableFunc)this).DoSeedFunc(ref gameItem);

            default:
                {
                    FixedString128Bytes errorCode =
                        $"FireFunc error. Unsupported SubType : {gameItem.SubType}, itemId : {gameItem.Id}";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                }
        }
    }

    private FarmActionResult FireGearFunc(ref GearItem gearItem, PlotManager plotManager, GameObject plot = null)
    {
        Debug.Log($"gear : {gearItem.ItemName}, gear.GearType : {gearItem.GearType}");

        return gearItem.GearType switch
        {
            GearType.Hoe =>
                ((IUseAreaHoeFunc)this).DoHoeFunc(ref gearItem, plotManager, plot),

            GearType.WateringCan =>
                ((IUseAreaWateringCanFunc)this).DoWateringCanFunc(),

            GearType.Hammer =>
                ((IUseAreaHammerFunc)this).DoHammerFunc(ref gearItem),

            GearType.Sickle =>
                ((IUseAreaSickleFunc)this).DoSickleFunc(ref gearItem),

            _ => new FarmActionResult(
                FarmActionResult.ResultType.Error,
                $"FireGearFunc error. Unsupported GearType : {gearItem.GearType}, itemId : {gearItem.Id}"
            )
        };
    }
    #endregion


    FarmActionResult IUseAreaHoeFunc.DoHoeFunc(ref GearItem gear, PlotManager plotManager, GameObject plot)
    {
        
        try
        {
            if (plot == null)
            {
                FixedString128Bytes errorCode = "DoHoeFunc error. plot is null";
                Debug.LogAssertion(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
            }
            if (plotManager == null)
            {
                FixedString128Bytes errorCode = "DoHoeFunc error. plotManager is null";
                Debug.LogAssertion(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
            }

            Collider[] hits = GetHits(_hoeMask);

            if (hits.Length == 0)
            {
                Vector3 spawnPos = transform.position;
                spawnPos.y = 0f; // Force y to 0 (ground level) instead of UseArea's Y offset (0.15f)
                GameObject created = Instantiate(plot, spawnPos, Quaternion.identity, plotManager.transform);
                int IID = created.GetInstanceID();

                Debug.Log($"<color=green>DoHoeFunc success! targetID = {IID}</color>");
                return new FarmActionResult(FarmActionResult.ResultType.Success);
            }
            else
            {
                Debug.Log($"<color=red>DoHoeFunc failed. Something is already there.</color>");
                return new FarmActionResult(FarmActionResult.ResultType.Failed); // 설치 실패}
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoHoeFuncError : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "HOE_FUNC_EXCEPTION");
        }
    }


    FarmActionResult IUseAreaAxeFunc.DoAxeFunc(ref GearItem gear)
    {
        try
        {
            Collider[] hits = GetHits(_axeMask);

            if (hits.Length > 0)
            {
                // 나무에 타격
                foreach (Collider hitCollider in hits)
                {
                    TreeProp tree = hitCollider.gameObject.GetComponent<TreeProp>();
                    tree.Damaged((gear.Efficiency.ToValue()));
                    gear.CurrentDurability -= 1; // 내구도 1 감소
                }
                return new FarmActionResult(FarmActionResult.ResultType.Success); // 나무 패기 성공
            }
            else
            {
                Debug.Log("DoAxeFunc error. No tree detected.");
                return new FarmActionResult(FarmActionResult.ResultType.Failed); // 나무 패기 실패
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoAxeFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "AXE_FUNC_EXCEPTION");
        }
    }

    FarmActionResult IUseAreaWateringCanFunc.DoWateringCanFunc()
    {
        try
        {
            Debug.Log("DoWateringcanFun has been Executed");
            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                PlotProp targetPlot = hits[0].gameObject.GetComponent<PlotProp>();
                if (targetPlot != null)
                {
                    return targetPlot.Watering();
                }
                else
                {
                    FixedString128Bytes errorCode = $" DoWateringCanFunc error. Unexpected Error : {hits.Length} ";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error);
                }
            }
            else if (hits.Length == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoWateringCanFunc error. Unexpected amount of target : {hits.Length} ";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoWateringCanFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "WATERINGCAN_FUNC_EXCEPTION");
        }
    }
    FarmActionResult IUseAreaSickleFunc.DoSickleFunc(ref GearItem gear)
    {
        try
        {
            Collider[] hits = GetHits(_sickleMask);
            if (hits.Length == 1)
            {

                PlotProp targetPlot = hits[0].gameObject.GetComponent<PlotProp>();
                GrassProp targetGrass = hits[0].gameObject.GetComponent<GrassProp>();
                if (targetPlot != null)
                {
                    FarmActionResult result = targetPlot.Reaping(ref gear); // 꽃 수확 성공
                    if(result.Result == FarmActionResult.ResultType.Success)
                    {
                        gear.CurrentDurability -= 1;
                    }
                }
                if (targetGrass != null)
                {
                    if (gear.CurrentDurability <= 0)
                    {
                        FixedString128Bytes errorCode = $"DoSickleFunc error. Durability is {gear.CurrentDurability}";
                        Debug.LogAssertion(errorCode);
                        return new FarmActionResult(FarmActionResult.ResultType.Failed, errorCode);
                    }
                    else
                    {
                        targetGrass.Reaping();
                        gear.CurrentDurability -= 1; // 내구도 1 감소
                        return new FarmActionResult(FarmActionResult.ResultType.Success); // 풀 베기 성공
                    }

                }
            }
            else if (hits.Length == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoSickleFunc error. Unexpected amount of target : {hits.Length} ";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoSickleFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "SICKLE_FUNC_EXCEPTION");
        }

        return new FarmActionResult(FarmActionResult.ResultType.Error, "Func doesn't coded");
    }

    FarmActionResult IUseAreaHammerFunc.DoHammerFunc(ref GearItem gear)
    {
        try
        {
            Collider[] hits = GetHits(_hammerMask);


            if (hits.Length == 1)
            {
                // TODO: Ore 게임오브젝트 찾아서 박살내기

                PlotProp targetPlot = hits[0].gameObject.GetComponent<PlotProp>();
                OreProp targetOre = hits[0].gameObject.GetComponent<OreProp>();

                // PlotProp이 대상일 경우
                if (targetPlot != null)
                {
                    return targetPlot.Hammering();
                }
                // OreProp이 대상일 경우
                else if (targetOre != null)
                {
                    if (gear.CurrentDurability <= 0)
                    {
                        FixedString128Bytes errorCode = $"DoHammerFunc error. Durability is {gear.CurrentDurability}";
                        Debug.LogAssertion(errorCode);
                        return new FarmActionResult(FarmActionResult.ResultType.Failed, errorCode);
                    }

                    return targetOre.Damaged(gear.Efficiency.ToValue());
                }
                else
                {
                    FixedString128Bytes errorCode = $" DoHammerFunc error. Unexpected Error : {hits.Length} ";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error);
                }
            }
            else if (hits.Length == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoHammerFunc error. Unexpected amount of target : {hits.Length} ";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoHammerFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "HAMMER_FUNC_EXCEPTION");
        }
    }

    FarmActionResult IUseAreaConsumableFunc.DoSeedFunc(ref GameItem item)
    {
        try
        {
            Debug.Log("DoSeedFunc has been Executed");
            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                PlotProp targetPlot = hits[0].gameObject.GetComponent<PlotProp>();
                if (targetPlot != null && item is FlowerItem seed)
                {
                    if(seed.SubType == ItemSubType.Seed)
                    return targetPlot.Sowing(ref seed);
                }

                    FixedString128Bytes errorCode = $" DoSeedFunc error. Unexpected Error : {hits.Length} ";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error);
            }
            else if (hits.Length == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoSeedFunc error. Unexpected amount of target : {hits.Length} ";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoSeedFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "SEED_FUNC_EXCEPTION");
        }
    }
    FarmActionResult IUseAreaConsumableFunc.DoFertilizerFunc(ref FertilizerItem item)
    {
        try
        {
            Debug.Log("DoFertilizerFunc has been Executed");

            if (item.Count <= 0)
            {
                FixedString128Bytes errorCode = $"DoFertilizerFunc error. Item count is {item.Count}";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
            }

            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                PlotProp targetPlot = hits[0].gameObject.GetComponent<PlotProp>();
                FarmActionResult result;
                if (targetPlot != null)
                {
                    switch (item.FertilizerType)
                    {
                        
                        case FertilizerType.Quality:
                            result = targetPlot.ApplyQualityFertilizer(item.FertilizerGrade);
                            if (result.Result == FarmActionResult.ResultType.Success)
                            {
                                item.Count -= 1;
                            }
                            return result;

                        case FertilizerType.Bountiful:
                            result = targetPlot.ApplyBountyFertilizer(item.FertilizerGrade);
                            if (result.Result == FarmActionResult.ResultType.Success)
                            {
                                item.Count -= 1;
                            }
                            return result;


                        case FertilizerType.AllInOne:
                            result = targetPlot.ApplyQualityFertilizer(item.FertilizerGrade);
                            if (result.Result == FarmActionResult.ResultType.Success)
                            {
                                FarmActionResult bountyResult = targetPlot.ApplyBountyFertilizer(item.FertilizerGrade);

                                result.Combine(bountyResult);
                                if (bountyResult.Result == FarmActionResult.ResultType.Success)
                                {
                                    item.Count -= 1;
                                }
                            }

                            return result;
                        default:
                            FixedString128Bytes errorCode = $"DoFertilizerFunc error. Unsupported FertilizerType : {item.FertilizerType}, itemId : {item.Id}";
                            Debug.Log(errorCode);
                            return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                    }
                }
                else
                {
                    FixedString128Bytes errorCode = $"DoFertilizerFunc error. Plot component not found.";
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                }
            }
            else if (hits.Length == 0)
            {
                return new FarmActionResult(FarmActionResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode =
                    $"DoFertilizerFunc error. Unexpected amount of target : {hits.Length}";
                Debug.Log(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoFertilizerFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "FERTILIZER_FUNC_EXCEPTION");
        }
    }
}
