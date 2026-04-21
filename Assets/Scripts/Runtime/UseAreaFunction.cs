using System;
using Unity.Collections;
using UnityEngine;
using static Constant;
public interface IUseAreaHoeFunc
{
    UseAreaResult DoHoeFunc(GameObject plot);
}
public interface IUseAreaWateringCanFunc
{
    UseAreaResult DoWateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    UseAreaResult DoHammerFunc();
}
public interface IUseAreaSickleFunc
{
    UseAreaResult DoSickleFunc();
}
public interface IUseAreaAxeFunc
{
    UseAreaResult DoAxeFunc();
}
public interface IUseAreaConsumableFunc
{
    UseAreaResult DoConsumableFunc(int Id);
}

public struct UseAreaResult
{
    public enum ResultType { Success, Failed, Error }

    private ResultType result;
    FixedString128Bytes errorCode { get; }

    public UseAreaResult(ResultType input_result, FixedString128Bytes input_errorCode = default)
    {
        result = input_result;
        errorCode = input_errorCode;
    }
}


public class UseAreaFunction : MonoBehaviour,
    IUseAreaAxeFunc, IUseAreaHoeFunc, IUseAreaWateringCanFunc,
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConsumableFunc
{
    
    private PlotManager _plotManager;

    private static int _hoeMask;
    private static int _treatMask;
    private static int _hammerMask;
    private static int _sickleMask;
    private static int _axeMask;

    private readonly Vector3 _smallBox = new Vector3(0.1f, 0.1f, 0.1f);


    //[Inject]
    //void Construct(PlotManager input_plotManagear) =>
    //    _plotManager = input_plotManagear;

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
        _sickleMask = LayerMask.GetMask(LAYER_PLOT,LAYER_GRASS);
        _hammerMask = LayerMask.GetMask(LAYER_ORE);

        
    }

    UseAreaResult IUseAreaHoeFunc.DoHoeFunc(GameObject plot)
    {
        try { 
        if (plot == null)
        {
            FixedString128Bytes errorCode = "DoHoeFunc error. plot is null";
            Debug.LogAssertion(errorCode);
            return new UseAreaResult(UseAreaResult.ResultType.Error, errorCode);
        }
        Collider[] hits = GetHits(_hoeMask);


        if (hits.Length == 0)
        {
            GameObject created = Instantiate(plot, transform.position, Quaternion.identity);
                int IID = created.GetInstanceID();

            Debug.Log($"<color=green>DoHoeFunc success! targetID = {IID}</color>");
            return new UseAreaResult(UseAreaResult.ResultType.Success);
        }
        else
        {
            Debug.Log($"<color=red>DoHoeFunc failed. Something is already there.</color>");
            return new UseAreaResult(UseAreaResult.ResultType.Failed); // 설치 실패}
        }
        }
        catch(Exception e)
        {
            Debug.Log($"DoHoeFuncError : {e.Message}");
            return new UseAreaResult(UseAreaResult.ResultType.Error, "HOE_FUNC_EXCEPTION");
        }
    }


    UseAreaResult IUseAreaAxeFunc.DoAxeFunc()
    {
        try
        {
            Collider[] hits = GetHits(_axeMask);

            
            if (hits.Length > 0)
            {
                // 나무 제거
                foreach (Collider hitCollider in hits)
                {
                    hitCollider.gameObject.SetActive(false);

                }
                return new UseAreaResult(UseAreaResult.ResultType.Success); // 제거 성공
            }
            else
            {
                Debug.Log("DoAxeFunc error. No tree detected.");
                return new UseAreaResult(UseAreaResult.ResultType.Failed); // 제거 실패
            }
        }
        catch( Exception e)
        {
            Debug.Log($"DoAxeFunc Error : {e.Message}");
            return new UseAreaResult(UseAreaResult.ResultType.Error, "AXE_FUNC_EXCEPTION");
        }
    }
    UseAreaResult IUseAreaWateringCanFunc.DoWateringCanFunc()
    {
        try
        {
            Collider[] hits = GetHits(_treatMask);

            if (hits.Length == 1)
            {
                foreach (Collider hitCollider in hits)
                {
                    Plot targetPlot = hitCollider.gameObject.GetComponent<Plot>();
                    if (targetPlot != null)
                    {
                        targetPlot.isWatered = true;
                    }
                }

                return new UseAreaResult(UseAreaResult.ResultType.Success);

            }
            else if ( hits.Length == 0)
            {
                return new UseAreaResult(UseAreaResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoWateringCanFunc error. Unexpected amount of target : {hits.Length} ";
                return new UseAreaResult(UseAreaResult.ResultType.Error);
            }
        }
        catch(Exception e)
        {
            Debug.Log($"DoWateringCanFunc Error : {e.Message}");
            return new UseAreaResult(UseAreaResult.ResultType.Error, "WATERINGCAN_FUNC_EXCEPTION");
        }


    }
    UseAreaResult IUseAreaSickleFunc.DoSickleFunc()
    {
        return new UseAreaResult(UseAreaResult.ResultType.Error, "Func doesn't coded");
    }
    UseAreaResult IUseAreaHammerFunc.DoHammerFunc()
    {
        try
        {
            Collider[] hits = GetHits(_hammerMask);

            if (hits.Length == 1)
            {
                foreach (Collider hitCollider in hits)
                {
                    Plot targetPlot = hitCollider.gameObject.GetComponent<Plot>();
                    if (targetPlot != null)
                    {
                        targetPlot.isWatered = true;
                    }
                }

                return new UseAreaResult(UseAreaResult.ResultType.Success);

            }
            else if (hits.Length == 0)
            {
                return new UseAreaResult(UseAreaResult.ResultType.Failed);
            }
            else
            {
                FixedString128Bytes errorCode = $" DoHammerFunc error. Unexpected amount of target : {hits.Length} ";
                return new UseAreaResult(UseAreaResult.ResultType.Error);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"DoWateringCanFunc Error : {e.Message}");
            return new UseAreaResult(UseAreaResult.ResultType.Error, "HAMMER_FUNC_EXCEPTION");
        }
    }
    UseAreaResult IUseAreaConsumableFunc.DoConsumableFunc(int Id)
    {

        return new UseAreaResult(UseAreaResult.ResultType.Error, "Func doesn't coded");
    }
    public UseAreaResult FireFunc(int itemId, GameObject plot = null)
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

        else if (itemId >= MIN_CONSUMABLE_ID && itemId <= MAX_CONSUMABLE_ID)
            return ((IUseAreaConsumableFunc)this).DoConsumableFunc(itemId);

        else
        {
            FixedString128Bytes errorCode = ("Fire Function error. Wrong itemId : " + itemId);
            Debug.Log(errorCode);
            return new UseAreaResult(UseAreaResult.ResultType.Error,errorCode);
        }
    }
    
    /// <summary>
    /// 테스트용 함수! 나중에는 FireFunc()를 사용하라구!
    /// </summary>
    /// <param name="pointingslot"></param>
    /// <param name="plot"></param>
    /// <returns></returns>
    public UseAreaResult FireFuncTest(int pointingslot, GameObject plot = null)
    {
        if (pointingslot == 1)
            return ((IUseAreaHoeFunc)this).DoHoeFunc(plot);
        else
        {
            FixedString128Bytes errorCode = ("Fire Function Test error. 1번일때만 동작함 : " + pointingslot);
            Debug.Log(errorCode);
            return new UseAreaResult(UseAreaResult.ResultType.Error, errorCode);
        }
    }
}
