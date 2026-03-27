using UnityEngine;

public interface IUseAreaHoeFunc
{
    public int DoHoeFunc();

}


public interface IUseAreaWateringCanFunc
{
    public int DoWateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    public int DoHammerFunc();
}
public interface IUseAreaSickleFunc
{
    public int DoSickleFunc();
}
public interface IUseAreaAxeFunc
{
    public int DoAxeFunc();
}
public interface IUseAreaConSumableFunc
{
    public int DoConsumableFunc();
}


public class UseAreaFunction : MonoBehaviour, 
    IUseAreaAxeFunc, IUseAreaHoeFunc,IUseAreaWateringCanFunc, 
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConSumableFunc
{

    public string InnerTag;

    private Collision collision;

    private void Awake()
    {
        collision = GetComponent<Collision>();
    }


    private void OnDisable()
    {
        // 여기서 참고한 객체 초기화 하기
    }

    int IUseAreaHoeFunc.DoHoeFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaAxeFunc.DoAxeFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaWateringCanFunc.DoWateringCanFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaSickleFunc.DoSickleFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaHammerFunc.DoHammerFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaConSumableFunc.DoConsumableFunc()
    {
        throw new System.NotImplementedException();
    }


    void OnTriggerEnter(Collider other)
    {
        InnerTag = other.gameObject.tag;
    }
    
    void OnTriggerExit(Collider other)
    {

    }


    

}
