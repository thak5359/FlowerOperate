using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] protected ItemData itemData;
    public int amount =1;
    [SerializeField] private int level = 0; 


    public string GetName()
    {
        return itemData.GetName(level);
    }
    public string GetDescription()
    {

        return itemData.GetDescription(level);
    }
    public Sprite GetSprite()
    { 
        return itemData.GetSprite(level);
    }
    
    virtual public void OnUse(Vector3 pos)
    {
        
    }

    virtual protected void Use()
    {

    }
    public void LevelUp()
    {
        level++;
    }
    
    public int GetLevel()
    {
        return level;
    }
    


}


    

