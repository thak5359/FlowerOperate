using UnityEngine;

[System.Serializable]
public class ItemObjectData
{
    [SerializeField] int itemID;
    [SerializeField] int amount;
    [SerializeField] int Duration;
    [SerializeField] int grade;

    public int GetItemID => itemID;
    public int GetAmount => amount;
    public int GetDuration => Duration;
    public int GetGrade => grade;

    public ItemObjectData(int itemID, int amount, int duration, int grade)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}