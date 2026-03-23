using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Anniversary
{
    None,
    Valentein,

}

public class EventManager : MonoBehaviour
{
    [SerializeField]
    ProgressManager progressManager = ProgressManager.Instance;
    [SerializeField]
    List<Tuple<int, int>> dayList = new List<Tuple<int, int>>();
    
    private Tuple<int, int> today;

    private void Start()
    {
        if (progressManager == null)
            progressManager = this.gameObject.GetComponent<ProgressManager>();

        if(today == null)
        {
            today = Tuple.Create((progressManager.getDay() - 1) / 28 + 1, (progressManager.getDay() - 1) % 28 + 1);
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
