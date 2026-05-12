using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public enum Anniversary
{
    None,
    Valentein,

}

public class EventManager : MonoBehaviour
{
    [SerializeField]
    List<Tuple<int, int>> dayList = new List<Tuple<int, int>>();
    
    private Tuple<int, int> today;


    private void Start()
    {
        if(today == null)
        {
            today = Tuple.Create((ProgressManager.getPlayDay() - 1) / 28 + 1, (ProgressManager.getPlayDay() - 1) % 28 + 1);
        }

        Debug.Log(today.Item1 + ", " +  today.Item2);
    }

    public void EventLoopFunc()
    {
        while (true)
        {
            switch(AnniversaryCheck(today))
            {
                case Anniversary.Valentein:
                    break;

                default:
                    break;

            }
        }
    }

    private Anniversary AnniversaryCheck(Tuple<int, int> Today)
    {


        return Anniversary.None;
    }

    private void ValenteinEvent()
    {

    }
}
