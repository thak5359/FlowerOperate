
using UnityEngine.SceneManagement;
using static Constant;

public enum MonthName
{
    마갈,
    보병,
    유어,
    백양,

    금우,
    쌍령,
    거해,
    영사,

    순백,
    권형,
    천갈,
    사부,
    인마
}


public partial class ProgressManager
{
    public static void GoNextDay()
    {
        GlobalEventManager.InvokeNextDay();
        SceneManager.LoadScene(FARM_SCENE_NAME.ToString());
        nextDay();
    }
}
