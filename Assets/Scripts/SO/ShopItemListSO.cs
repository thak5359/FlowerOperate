// 수정할 위치: ProductData 구조체 (ProductData.cs)
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct ProductData
{
    [SerializeField] int productNO; // [SerializeField] 추가
    public int ProductNo => productNO;

    [SerializeField] string productName; // [SerializeField] 추가
    public string ProductName => productName;

    [SerializeField] int cost; // [SerializeField] 추가
    public int Cost => cost;

    [SerializeField] int unlockDay; // [SerializeField] 추가
    public int UnlockDay => unlockDay;

    // 에디터 스크립트에서 값을 할당하기 위한 생성자 추가
    public ProductData(int productNO, string productName, int cost, int unlockDay)
    {
        this.productNO = productNO;
        this.productName = productName;
        this.cost = cost;
        this.unlockDay = unlockDay;
    }
}

[CreateAssetMenu(fileName = "ShopItemListSO", menuName = "Dataset/ShopItemList", order = 5)]
public class ShopItemListSO : ScriptableObject
{
    [field: SerializeField] private ProductData[] products;

    public ref ProductData getProductData(ref int idx)
    {
        return ref products[idx];
    }

    public int GetLength()
    {
        return products.Length;
    }
}
