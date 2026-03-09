using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : SlotItem
{
    new FlowerData itemData;
    protected int grade;

    public int GetGrade()
    {
        return grade;
    }

    public bool UpGrade()
    {
        if (grade < 10)
        {
            grade++;
            return true;
        }
        else
            return false;
    }
}
