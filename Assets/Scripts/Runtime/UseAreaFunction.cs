using Cysharp.Threading.Tasks;
using System;
using Unity.Collections;
using UnityEngine;
using VContainer;
using static Constant;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using System.Collections;
public interface IUseAreaHoeFunc
{
    FarmActionResult DoHoeFunc(GameObject plot);
}
public interface IUseAreaWateringCanFunc
{
    FarmActionResult DoWateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    FarmActionResult DoHammerFunc();
}
public interface IUseAreaSickleFunc
{
    FarmActionResult DoSickleFunc();
}
public interface IUseAreaAxeFunc
{
    FarmActionResult DoAxeFunc();
}
public interface IUseAreaConsumableFunc
{
    FarmActionResult DoConsumableFunc(int Id);
}
public interface IUseAreaConsumableFuncTest
{
    FarmActionResult DoSeedFunc(int itemID);
    FarmActionResult DoFertilizerFunc(int itemID);
}

public struct FarmActionResult
{
    public enum ResultType { Success, Failed, Error }

    private ResultType result;
    public FixedString128Bytes errorMessage { get; private set; }

    public ResultType Result() => result;

    public FarmActionResult(ResultType input_result, FixedString128Bytes input_errorCode = default)
    {
        result = input_result;
        errorMessage = input_errorCode;
    }
    public void Combine(FarmActionResult resultB)
    {
        if (resultB.Result() == ResultType.Error)
        {
            result = ResultType.Error;
            errorMessage = resultB.errorMessage;
        }
        else if (resultB.Result() == ResultType.Failed && result != ResultType.Error)
        {
            result = ResultType.Failed;
        }
    }

}


public class UseAreaFunction : MonoBehaviour,
    IUseAreaAxeFunc, IUseAreaHoeFunc, IUseAreaWateringCanFunc,
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConsumableFunc, IUseAreaConsumableFuncTest
{
    static readonly uint RandSeed = (uint)DateTime.Now.Ticks;

    Unity.Mathematics.Random mathRand = new Unity.Mathematics.Random(RandSeed);

    private PlotManager _plotManager;

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
        _hoeMask = LayerMask.GetMask(LAYER_PLOT, LAYER_OBSTACLE, LAYER_TREE, LAYER_INTERACTABLE);
        _treatMask = LayerMask.GetMask(LAYER_PLOT);
        _axeMask = LayerMask.GetMask(LAYER_TREE);
        _sickleMask = LayerMask.GetMask(LAYER_PLOT, LAYER_GRASS);
        _hammerMask = LayerMask.GetMask(LAYER_ORE, LAYER_PLOT);

        _plotManager = PlotManager.Instance;
    }

    FarmActionResult IUseAreaHoeFunc.DoHoeFunc(GameObject plot)
    {
        try
        {
            if (plot == null)
            {
                FixedString128Bytes errorCode = "DoHoeFunc error. plot is null";
                Debug.LogAssertion(errorCode);
                return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
            }
            Collider[] hits = GetHits(_hoeMask);


            if (hits.Length == 0)
            {
                GameObject created = Instantiate(plot, transform.position, Quaternion.identity);
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
    FarmActionResult IUseAreaAxeFunc.DoAxeFunc()
    {
        try
        {
            Collider[] hits = GetHits(_axeMask);

            if (hits.Length > 0)
            {
                // 나무 제거
                foreach (Collider hitCollider in hits)
                {
                    hitCollider.gameObject.SetActive(false);// TODO :: 나무에 데미지를 주게 만드는 함수 작성해서 여기에서 호출하기
                }
                return new FarmActionResult(FarmActionResult.ResultType.Success); // 제거 성공
            }
            else
            {
                Debug.Log("DoAxeFunc error. No tree detected.");
                return new FarmActionResult(FarmActionResult.ResultType.Failed); // 제거 실패
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
                Plot targetPlot = hits[0].gameObject.GetComponent<Plot>();
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
    FarmActionResult IUseAreaSickleFunc.DoSickleFunc()
    {
        try
        {
            Collider[] hits = GetHits(_sickleMask);
            if(hits.Length >= 1)
            {
                foreach(Collider hit in hits)
                {
                    Plot targetPlot = hit.gameObject.GetComponent<Plot>();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoSickleFunc Error : {e.Message}");
            return new FarmActionResult(FarmActionResult.ResultType.Error, "SICKLE_FUNC_EXCEPTION");
        }

        return new FarmActionResult(FarmActionResult.ResultType.Error, "Func doesn't coded");
    }
    FarmActionResult IUseAreaHammerFunc.DoHammerFunc()
    {
        try
        {
            Collider[] hits = GetHits(_hammerMask);


            if (hits.Length == 1)
            {
                // TODO: Ore 게임오브젝트 찾아서 박살내기

                Plot targetPlot = hits[0].gameObject.GetComponent<Plot>();
                if (targetPlot != null)
                {
                    return targetPlot.Ruining();
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
    FarmActionResult IUseAreaConsumableFunc.DoConsumableFunc(int Id)
    {

        return new FarmActionResult(FarmActionResult.ResultType.Error, "Func doesn't coded");
    }
    FarmActionResult IUseAreaConsumableFuncTest.DoSeedFunc(int itemID)
    {
        try
        {
            Debug.Log("DoSeedFunc has been Executed");
            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                Plot targetPlot = hits[0].gameObject.GetComponent<Plot>();
                if (targetPlot != null)
                {
                    return targetPlot.Sowing(itemID);
                }
                else
                {
                    FixedString128Bytes errorCode = $" DoSeedFunc error. Unexpected Error : {hits.Length} ";
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
    FarmActionResult IUseAreaConsumableFuncTest.DoFertilizerFunc(int itemID)
    {
        try
        {
            Debug.Log("DoSeedFunc has been Executed");
            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                Plot targetPlot = hits[0].gameObject.GetComponent<Plot>();
                if (targetPlot != null)
                {
                    if (QUALITY_FERTILIZER_START_ID <= itemID && itemID < BOUNTIFUL_FERTILIZER_START_ID)
                    {
                        if (mathRand.NextInt(1, 10) <= (itemID - QUALITY_FERTILIZER_START_ID + 1) * 2)
                        {
                            return targetPlot.QualityUp();
                        }
                        else
                        {
                            return new FarmActionResult(FarmActionResult.ResultType.Failed);
                        }
                    }
                    else if (BOUNTIFUL_FERTILIZER_START_ID <= itemID && itemID < ALLINONE_FERTILIZER_START_ID)
                    {
                        cachedItemLevel = itemID - BOUNTIFUL_FERTILIZER_START_ID + 1;

                        if (cachedItemLevel % 2 == 0)
                        {
                            cachedBountyAmount = cachedItemLevel / 2;
                            if (mathRand.NextFloat(0, 1) <= 0.5f)
                                cachedBountyAmount++;
                            return targetPlot.BountyUP(cachedBountyAmount);
                        }
                        else
                        {
                            cachedBountyAmount = (cachedItemLevel + 1) / 2;
                            return targetPlot.BountyUP(cachedBountyAmount);
                        }


                    }
                    else if (ALLINONE_FERTILIZER_START_ID <= itemID && itemID < ALLINONE_FERTILIZER_END_ID)
                    {
                        cachedItemLevel = itemID - ALLINONE_FERTILIZER_START_ID + 1;

                        FarmActionResult resultA;

                        if (mathRand.NextInt(1, 10) <= (cachedItemLevel * 2))
                        {
                            resultA = targetPlot.QualityUp();
                        }
                        else
                        {
                            resultA = new FarmActionResult(FarmActionResult.ResultType.Failed);
                        }

                        if (cachedItemLevel % 2 == 0)
                        {
                            cachedBountyAmount = cachedItemLevel / 2;
                            if (mathRand.NextFloat(0, 1) <= 0.5f)
                                cachedBountyAmount++;
                            resultA.Combine(targetPlot.BountyUP(cachedBountyAmount));
                        }
                        else
                        {
                            cachedBountyAmount = (cachedItemLevel + 1) / 2;
                            resultA.Combine(targetPlot.BountyUP(cachedBountyAmount));
                        }
                        return resultA;
                    }
                    else
                    {
                        FixedString128Bytes errorCode = $" DoSeedFunc error. Unexpected ItemID : {itemID} ";
                        Debug.Log(errorCode);
                        return new FarmActionResult(FarmActionResult.ResultType.Error);
                    }
                }
                else
                {
                    FixedString128Bytes errorCode = $" DoSeedFunc error. Unexpected Error : {hits.Length} ";
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

    public FarmActionResult FireFunc(int itemId, GameObject plot = null)
    {
        // TODO : 이전에 FireFuncTest로 실행된 부분을 FIreFunc로 바꾸기.
        if (itemId > MIN_HOE_ID && itemId < MAX_HOE_ID)
            return ((IUseAreaHoeFunc)this).DoHoeFunc(plot);
        else if (itemId > MIN_WATERINGCAN_ID && itemId < MAX_WATERINGCAN_ID)
            return ((IUseAreaWateringCanFunc)this).DoWateringCanFunc();

        else if (itemId > MIN_HAMMER_ID && itemId < MAX_HAMMER_ID)
            return ((IUseAreaHammerFunc)this).DoHammerFunc();

        else if (itemId > MIN_SICKLE_ID && itemId < MAX_SICKLE_ID)
            return ((IUseAreaSickleFunc)this).DoSickleFunc();

        else if (itemId > MIN_AXE_ID && itemId < MAX_AXE_ID)
            return ((IUseAreaAxeFunc)this).DoAxeFunc();

        else if (itemId >= QUALITY_FERTILIZER_START_ID && itemId <= ALLINONE_FERTILIZER_END_ID && itemId%2 == 0)
            return ((IUseAreaConsumableFunc)this).DoConsumableFunc(itemId);

        else
        {
            FixedString128Bytes errorCode = ("Fire Function error. Wrong itemId : " + itemId);
            Debug.Log(errorCode);
            return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
        }
    }

    /// <summary>
    /// 테스트용 함수! 나중에는 FireFunc()를 사용하라구!
    /// </summary>
    /// <param name="pointingslot"></param>
    /// <param name="plot"></param>
    /// <returns></returns>
    public FarmActionResult FireFuncTest(int pointingslot, GameObject plot = null)
    {
        switch (pointingslot)
        {
            case 1:
                {
                    return ((IUseAreaHoeFunc)this).DoHoeFunc(plot);
                }
            case 2:
                {
                    return ((IUseAreaSickleFunc)this).DoSickleFunc();
                }
            case 3:
                {
                    return ((IUseAreaAxeFunc)this).DoAxeFunc();
                }
            case 4:
                {
                    return ((IUseAreaWateringCanFunc)this).DoWateringCanFunc();
                }
            case 5:
                {
                    return ((IUseAreaHammerFunc)this).DoHammerFunc();
                }
            case 6:
                {
                    return ((IUseAreaConsumableFuncTest)this).DoSeedFunc(pointingslot);
                }
            case 7:
                {
                    return ((IUseAreaConsumableFuncTest)this).DoFertilizerFunc(pointingslot);
                }
            default:
                {
                    FixedString128Bytes errorCode = ("Fire Function Test error. 기능이할당 되었을 때만 동작함 : " + pointingslot);
                    Debug.Log(errorCode);
                    return new FarmActionResult(FarmActionResult.ResultType.Error, errorCode);
                }
        }
    }
}
