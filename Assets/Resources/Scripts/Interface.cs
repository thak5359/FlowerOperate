using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public interface IInteractable
{
    void OnInteract();
}

public interface IItem
{
    enum ItemType
    {
        Gear,
        Consumable,
        QuestItem,
        Flower,
    }

    ItemType Type { get; set; }

}
public interface IUsable : IItem
{
    public void OnUse();
}



