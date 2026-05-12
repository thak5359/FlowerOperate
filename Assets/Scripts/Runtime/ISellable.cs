using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISellable
{
    int value { get; }


    public virtual int Sell()
    {
        return value;
    }

}
