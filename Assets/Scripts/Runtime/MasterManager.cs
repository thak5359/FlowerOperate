using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterManager : MonoBehaviour
{
    //private ProgressManager progressManager;
    //private IAmapManager iaMapManager;
    // 세이브 로드 관리 매니저
    
    public static MasterManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else { Destroy(this.gameObject); }
    }

    MasterManager Instance => instance;


    //public void setProgressM(ProgressManager input_manager)
    //{
    //    progressManager = input_manager;
    //}

    //public void setIAmapM(IAmapManager input_manager)
    //{
    //    iaMapManager = input_manager;
    //}

    //public void setSaveLoadM(ProgressManager input_manager)
    //{
    //    progressManager = input_manager;
    //}
}
