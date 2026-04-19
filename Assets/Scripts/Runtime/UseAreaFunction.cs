using UnityEditor.Build;
using UnityEngine;
using VContainer;
using static Constant;
public interface IUseAreaHoeFunc
{
    int DoHoeFunc(GameObject plot);
}
public interface IUseAreaWateringCanFunc
{
    int DoWateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    int DoHammerFunc();
}
public interface IUseAreaSickleFunc
{
    int DoSickleFunc();
}
public interface IUseAreaAxeFunc
{
    int DoAxeFunc();
}
public interface IUseAreaConsumableFunc
{
    int DoConsumableFunc(int Id);
}



public class UseAreaFunction : MonoBehaviour,
    IUseAreaAxeFunc, IUseAreaHoeFunc, IUseAreaWateringCanFunc,
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConsumableFunc
{

    private PlotManager _plotManager;

    private int _hoeMask;
    private int _treatMask;
    private int _hammerMask;
    private int _sickleMask;
    private int _axeMask;

    private readonly Vector3 _smallBox = new Vector3(0.1f, 0.1f, 0.1f);
    private LayerMask detectionLayer;

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

    int IUseAreaHoeFunc.DoHoeFunc(GameObject plot)
    {

        //TODO : 여기에 참고한 객체로 할 행동 구현하기. 성공하면 1반환, 실패했다면 0 반환, 오류는 -100 반환!
        if (plot == null)
        {
            Debug.LogAssertion("DoHoeFunc error. plot is null");
            return -100;
        }
        Collider[] hits = GetHits(_hoeMask);


        if (hits.Length == 0)
        {
            Instantiate(plot, transform.position, Quaternion.identity);
            return 1;
        }
        else
        {
            Debug.Log("DoHoeFunc failed. Something is already there.");
            return 0; // 설치 실패}
        }
    }


    int IUseAreaAxeFunc.DoAxeFunc()
    {

        Collider[] hits = GetHits(_axeMask);    


        if (hits.Length > 0)
        {
            // 나무 제거
            foreach (Collider hitCollider in hits)
            {
                // TODO : 나무 제거 및 아이템 드랍 구현
            }
            return 1; // 제거 성공
        }
        else
        {
            Debug.Log("DoAxeFunc error. No tree detected.");
            return 0; // 제거 실패
        }
    }
    int IUseAreaWateringCanFunc.DoWateringCanFunc()
    {
        Collider[] hits = GetHits(_treatMask);

        if (hits.Length > 0)
        {
            foreach (Collider hitCollider in hits)
            {
                Plot targetPlot = hitCollider.gameObject.GetComponent<Plot>();
                if (targetPlot != null)
                {
                    targetPlot.isWatered = true;
                }
            }


        }
        return 1;
    }
    int IUseAreaSickleFunc.DoSickleFunc()
    {
        return 1;
    }
    int IUseAreaHammerFunc.DoHammerFunc()
    {
        return 1;
    }
    int IUseAreaConsumableFunc.DoConsumableFunc(int Id)
    {
        
        return 0;
    }
    public int FireFunc(int itemId, GameObject plot = null)
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
            Debug.Log("Fire Function error. Wrong itemId : " + itemId);
        return -100;
    }

    
    /// <summary>
    /// 테스트용 함수! 나중에는 FireFunc()를 사용하라구!
    /// </summary>
    /// <param name="pointingslot"></param>
    /// <param name="plot"></param>
    /// <returns></returns>
    public int FireFuncTest(int pointingslot, GameObject plot = null)
    {
        if (pointingslot == 1)
            return ((IUseAreaHoeFunc)this).DoHoeFunc(plot);
        else
            Debug.Log("Fire Function Test error. 1번일때만 동작함 : " + pointingslot);
        return -100;
    }



}
