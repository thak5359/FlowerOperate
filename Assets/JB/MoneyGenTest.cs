using UnityEngine;
using VContainer;

public class MoneyGenTest : MonoBehaviour
{
    [Inject] private ItemManager itemManager;
    [Inject] private PlayerOwnItemDataManager _inven;
    public void OnClick()
    {
        _inven.GetData.AddMoney(1000000);
    }
}
