using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;




public interface IInteractable
{
    void OnInteract();
}

public interface IItem
{
    Image Image { get; set; }
}

public class Item
{
    public int amount;
    
    public Image image { get; set; }
    virtual protected void useItem()
    {
        //기능 구현
    }

    virtual public void onUse()
    {
        useItem();
    }
}

public interface IUsable : IItem
{
    public void OnUse();
}



