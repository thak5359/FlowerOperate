using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseAreaHoeFunc
{
    public int HoeFunc();

}
public interface IUseAreaWateringCanFunc
{
    public int WateringCanFunc();
}
public interface IUseAreaHammerFunc
{
    public int HammerFunc();
}
public interface IUseAreaSickleFunc
{
    public int SickleFunc();
}
public interface IUseAreaAxeFunc
{
    public int AxeFunc();
}
public interface IUseAreaConSumableFunc
{ public int ConsumableFunc();}


public class UseAreaFunction : MonoBehaviour, 
    IUseAreaAxeFunc, IUseAreaHoeFunc,IUseAreaWateringCanFunc, 
    IUseAreaSickleFunc, IUseAreaHammerFunc, IUseAreaConSumableFunc
{
    private Collision collision;

    private void Awake()
    {
        collision = GetComponent<Collision>();
    }


    private void OnDisable()
    {
        // 여기서 참고한 객체 초기화 하기
    }

    int IUseAreaHoeFunc.HoeFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaAxeFunc.AxeFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaWateringCanFunc.WateringCanFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaSickleFunc.SickleFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaHammerFunc.HammerFunc()
    {
        throw new System.NotImplementedException();
    }
    int IUseAreaConSumableFunc.ConsumableFunc()
    {
        throw new System.NotImplementedException();
    }


    void OnTriggerEnter(Collider other)
    {

    }
    void OnTriggerExit(Collider other)
    {

    }

}
