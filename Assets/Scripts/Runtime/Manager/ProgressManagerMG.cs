using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ProgressManager
{
    public void GoNextDay()
    {
        GlobalEventManager.InvokeNextDay();
        nextDay();
        // SceneManager.Load()로 다음 씬으로!
    }

    
}
