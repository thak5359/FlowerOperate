using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randFlower : MonoBehaviour
{
    [SerializeField]
    FlowerIdData FlowerIdData;
    // Start is called before the first frame update
    public void Onclick()
    {
        int randId = Random.Range(0, FlowerIdData.itemName.Count);
        Debug.Log(FlowerIdData.itemName[randId]);
    }
}
