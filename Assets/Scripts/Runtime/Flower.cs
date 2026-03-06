using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : Item
{
    new FlowerData itemData;
    protected int grade;


    override public void OnUse(Vector2 heading, Vector3 pos)
    {

    }

    override protected void Use()
    {

    }

    override public void LevelUp()
    {
        
    }
    

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
