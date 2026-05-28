using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenTest : MonoBehaviour
{
    public void OnClick()
    {
        ItemFactory.CreateItemPrefab(new FlowerItem(402003, 1), new Vector3(1, 1, 20));
    }
}
