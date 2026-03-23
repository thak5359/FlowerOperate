using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerateManager : MonoBehaviour
{
    private static ItemGenerateManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(instance);
    }

    public static ItemGenerateManager Instance
    {
        get 
        { 
            if( instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public void GenItem(int id, int amount, int duration, int grade)
    {

    }
}
