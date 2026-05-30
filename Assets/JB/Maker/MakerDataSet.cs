using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakerDataSetSO", menuName = "Dataset/MakerData", order = 5)]
public class MakerDataSet : ScriptableObject
{
    [SerializeField] private MakerDataSO[] makerDataList;

    public ref MakerDataSO GetMakerData(ref int makerNo)
    {
        return ref makerDataList[makerNo - 1];
    }

    public int GetLength()
    {
        return makerDataList.Length;
    }

#if UNITY_EDITOR
    public void SetMakerDataList(MakerDataSO[] list)
    {
        makerDataList = list;
    }
#endif
}
