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

    //¼¼ÅÍ
    public void SetItemID(int itemID) => this.itemID = itemID;
    public void SetAmount(int amount) => this.amount = amount;
    public void SetDuration(int Dur) => this.Duration = Dur;
    public void SetGrade(int grade) => this.grade = grade;

    public ItemObjectData(int itemID, int amount, int duration, int grade)
    {
        this.itemID = itemID;
        this.amount = amount;
        Duration = duration;
        this.grade = grade;
    }
}