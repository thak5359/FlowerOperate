using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class ShopLogic : MonoBehaviour
{
    [SerializeField] private int _money = 0;

    private SaveLoadManager _saveLoadManager;
    private PlayerOwnItemDataManager _playerOwnItem;

    [Inject]
    public void Construct(SaveLoadManager saveLoad, PlayerOwnItemDataManager playerOwn)
    {
        _saveLoadManager = saveLoad;
        _playerOwnItem = playerOwn;
    }

    private void Start()
    {
        this._money = _saveLoadManager.GetSaveDatas.GetMoney;
    }

    public void BuyItem(int id, int amount)
    {
        // SaveLoadManager.GetSaveData에서 Money 가져옴
        // 보유재화 >= 아이템 가격 => 인벤토리에 추가

        if (_money < GlobalItemDB.GetPrice(id) * amount)
        {
            Debug.LogError("재화가 부족합니다.");
            return;
        }

    }
}
