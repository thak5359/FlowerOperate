using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MakerDataSetSO", menuName = "Dataset/MakerData", order = 5)]
public class MakerDataSet : ScriptableObject
{
    [SerializeField] private MakerData[] makerDataList;

    public ref MakerData GetMakerData(ref int makerNo)
    {
        return ref makerDataList[makerNo - 1];
    }

    public int GetLength()
    {
        return makerDataList.Length;
    }

#if UNITY_EDITOR
    public void SetMakerDataList(MakerData[] list)
    {
        makerDataList = list;
    }
#endif

    public ref MakerData GetMakerDataByTypeAndTier(MakerType type, MakerTier tier)
    {
        for (int i = (int)type; i < makerDataList.Length; i++)
        {
            if (makerDataList[i].GetMakerTier == tier)
            {
                return ref makerDataList[i];
            }
        }

        throw new System.Exception($"No MakerData found for Type: {type} and Tier: {tier}");
    }
}
