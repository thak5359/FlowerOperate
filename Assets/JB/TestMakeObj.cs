using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMakeObj : MonoBehaviour
{
    public void BtnClick()
    {
        ItemGenerateManager.GenItem(3, 2, 2, 1);
    }

    public void ReturnObj()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Item");
        obj.GetComponent<ItemDataContainer>().DestroyItem();
    }
}
