using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Properties;
using UnityEngine;
using VContainer.Unity;
using static Constant;

public class UseAreamanager : MonoBehaviour, IAsyncStartable
{

    private int currentChargeLevel = 0; // 기본, 1, 2, 3, 4
    private List<GameObject> pool = new List<GameObject>();

    Vector3 defaultArea = new Vector3(1, 0, 0);

    // 오른쪽으로 바라보는 기준으로 영역 전개

    #region 괭이, 물뿌리개, 망치, 소모품 영역범위
    List<Vector3> AreaA1 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(2f, 0f, 0f),
};

    List<Vector3> AreaA2 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaA3 = new List<Vector3>()
{
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, 1f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f)
};

    List<Vector3> AreaA4 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),

    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
};

    List<Vector3> AreaA5 = new List<Vector3>()
{
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),


    new Vector3(4f, 2f, 0f),
    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f),
    new Vector3(4f, -2f, 0f),


    new Vector3(5f, 2f, 0f),
    new Vector3(5f, 1f, 0f),
    new Vector3(5f, 0f, 0f),
    new Vector3(5f, -1f, 0f),
    new Vector3(5f, -2f, 0f)
};
    #endregion

    #region 낫 영역범위
    List<Vector3> AreaB1 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaB2 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

};

    List<Vector3> AreaB3 = new List<Vector3>()
{
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),

    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),
};
    #endregion

    #region 도끼 영역범위
    List<Vector3> AreaC1 = new List<Vector3>()
{
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),

    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),

    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f)
};

    List<Vector3> AreaC2 = new List<Vector3>()
{
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),

    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),

    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),

    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),

    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f)
};

    List<Vector3> AreaC3 = new List<Vector3>()
{
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),

    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),

    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),

    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),

    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),

    new Vector3(2f, -2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),

    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f)
};

    List<Vector3> AreaC4 = new List<Vector3>()
{
    new Vector3(-4f, 1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, -1f, 0f),

    new Vector3(-3f, 2f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, -2f, 0f),

    new Vector3(-2f, 3f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -3f, 0f),

    new Vector3(-1f, 4f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -4f, 0f),

    new Vector3(0f, 4f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -4f, 0f),

    new Vector3(1f, 4f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -4f, 0f),

    new Vector3(2f, 3f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -3f, 0f),

    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),

    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f)
};

    List<Vector3> AreaC5 = new List<Vector3>()
{

    new Vector3(-5f, 1f, 0f),
    new Vector3(-5f, 0f, 0f),
    new Vector3(-5f, -1f, 0f),

    new Vector3(-4f, 2f, 0f),
    new Vector3(-4f, 1f, 0f),
    new Vector3(-4f, 0f, 0f),
    new Vector3(-4f, -1f, 0f),
    new Vector3(-4f, -2f, 0f),

    new Vector3(-3f, 3f, 0f),
    new Vector3(-3f, 2f, 0f),
    new Vector3(-3f, 1f, 0f),
    new Vector3(-3f, 0f, 0f),
    new Vector3(-3f, -1f, 0f),
    new Vector3(-3f, -2f, 0f),
    new Vector3(-3f, 3f, 0f),

    new Vector3(-2f, 4f, 0f),
    new Vector3(-2f, 3f, 0f),
    new Vector3(-2f, 2f, 0f),
    new Vector3(-2f, 1f, 0f),
    new Vector3(-2f, 0f, 0f),
    new Vector3(-2f, -1f, 0f),
    new Vector3(-2f, -2f, 0f),
    new Vector3(-2f, -3f, 0f),
    new Vector3(-2f, 4f, 0f),

    new Vector3(-1f, 5f, 0f),
    new Vector3(-1f, 4f, 0f),
    new Vector3(-1f, 3f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(-1f, 1f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, -1f, 0f),
    new Vector3(-1f, -2f, 0f),
    new Vector3(-1f, -3f, 0f),
    new Vector3(-1f, -4f, 0f),
    new Vector3(-1f, -5f, 0f),

    new Vector3(0f, 5f, 0f),
    new Vector3(0f, 4f, 0f),
    new Vector3(0f, 3f, 0f),
    new Vector3(0f, 2f, 0f),
    new Vector3(0f, 1f, 0f),
    new Vector3(0f, -1f, 0f),
    new Vector3(0f, -2f, 0f),
    new Vector3(0f, -3f, 0f),
    new Vector3(0f, -4f, 0f),
    new Vector3(0f, -5f, 0f),

    new Vector3(1f, 5f, 0f),
    new Vector3(1f, 4f, 0f),
    new Vector3(1f, 3f, 0f),
    new Vector3(1f, 2f, 0f),
    new Vector3(1f, 1f, 0f),
    new Vector3(1f, 0f, 0f),
    new Vector3(1f, -1f, 0f),
    new Vector3(1f, -2f, 0f),
    new Vector3(1f, -3f, 0f),
    new Vector3(1f, -4f, 0f),
    new Vector3(1f, -5f, 0f),

    new Vector3(2f, 4f, 0f),
    new Vector3(2f, 3f, 0f),
    new Vector3(2f, 2f, 0f),
    new Vector3(2f, 1f, 0f),
    new Vector3(2f, 0f, 0f),
    new Vector3(2f, -1f, 0f),
    new Vector3(2f, -2f, 0f),
    new Vector3(2f, -3f, 0f),
    new Vector3(2f, -4f, 0f),

    new Vector3(3f, 3f, 0f),
    new Vector3(3f, 2f, 0f),
    new Vector3(3f, 1f, 0f),
    new Vector3(3f, 0f, 0f),
    new Vector3(3f, -1f, 0f),
    new Vector3(3f, -2f, 0f),
    new Vector3(3f, -3f, 0f),

    new Vector3(4f, 2f, 0f),
    new Vector3(4f, 1f, 0f),
    new Vector3(4f, 0f, 0f),
    new Vector3(4f, -1f, 0f),
    new Vector3(4f, -2f, 0f),

    new Vector3(5f, 1f, 0f),
    new Vector3(5f, 0f, 0f),
    new Vector3(5f, -1f, 0f),
};
    #endregion

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        
        //오브젝트 풀링으로 사용할 영역 50개 만들어놓기

    }

    public enum Type
    {
        TypeA, TypeB, TypeC
    }

    //ItemObjectData
    public void DimensionExpansion()
    {
        //ItemObjectData에서 데이터를 출력

        foreach( var obj in pool) obj.SetActive(false);




    }


    private List<Vector3> GetAreaList(int itemId, int level)
    {



        return level switch
        {
            1 => AreaC1,
            2 => AreaC2,
            3 => AreaC3,
            4 => AreaC4,
            5 => AreaC5,
            _ => null
        };
    }

}
