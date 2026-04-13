using UnityEngine;
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
    int DoConsumableFunc();
}


public class UseAreaFunction : MonoBehaviour, 
    IUseAreaAxeFunc, IUseAreaHoeFunc,IUseAreaWateringCanFunc, 
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConsumableFunc
{
    public GameObject _currentTarget;
    public string innerTag;

    private Collider collision;

    private void Awake()
    {
        collision = GetComponent<Collider>();
    }
   

    private void OnDisable()
    {
        // 여기서 참고한 객체 초기화 하기
        innerTag = null;
        _currentTarget = null;
    }

    int IUseAreaHoeFunc.DoHoeFunc(GameObject plot)
    {
        //TODO : 여기에 참고한 객체로 할 행동 구현하기. 성공하면 1반환, 실패했다면 0 반환, 오류는 -100 반환!
        if(plot == null)
        {
            Debug.LogAssertion("DoHoeFunc error. plot is null");
            return -100;
        }

        if( plot.tag != null)
        {
            Debug.Log("DoHoeFunc : 이미 해당칸에 사물이 존재하므로 돌아갑니다! " + plot.tag);
            return 0;
        }
        else
        {
            //밭 생성
            Instantiate(plot, this.gameObject.transform.position, Quaternion.identity);
            return 1;
        }
    }
    int IUseAreaAxeFunc.DoAxeFunc()
    {
        //TODO : 여기에 참고한 객체로 할 행동 구현하기
        return 1;
    }
    int IUseAreaWateringCanFunc.DoWateringCanFunc()
    {
        //TODO : 여기에 참고한 객체로 할 행동 구현하기
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
    int IUseAreaConsumableFunc.DoConsumableFunc()
    {
        return 1;
    }

    public int FireFunc(int itemId, GameObject plot = null)
    {
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

        else if (itemId > MIN_CONSUMABLE_ID && itemId < MAX_CONSUMABLE_ID)
            return ((IUseAreaConsumableFunc)this).DoConsumableFunc();

        else
            Debug.LogAssertion("Fire Function error. Wrong itemId : " + itemId);
            return -100;
    }



    void OnTriggerEnter(Collider other)
    {
        if (other == this|| other == null ) return;
        _currentTarget = other.gameObject;
        innerTag = _currentTarget.gameObject.tag;
    }
    
    void OnTriggerExit(Collider other)
    {
        if (_currentTarget == other.gameObject) _currentTarget = null;
    }


    

}
