using R3;
using UnityEngine;

public static class QuestProgressPublisher
{

    public static ReactiveProperty<int> G_Money = new ReactiveProperty<int>(0);


    // 타겟 세부 정보가 있을수 있는가 ? int : Unit


    #region  인벤토리

    // 아이템 일정 개수 이상 소지
    public static Subject<(int itemID, int amount)> ItemRetaining = new Subject<(int, int)>();

    public static Subject<int> ItemObtain = new Subject<int>();

    public static Subject<int> ItemSell = new Subject<int>();

    #endregion


    #region 상점

    // 아이템 납품
    public static Subject<int> SubmissItemID = new Subject<int>();
    #endregion


    #region 아이템 사용

    // 특정 번호의 소모성 아이템 사용
    public static Subject<int> UsedItemID = new Subject<int>();

    #endregion

    #region 농장
    #region 밭

    public static Subject<Unit> PlowPlot = new Subject<Unit>();

    public static Subject<int> PlotSowing = new Subject<int>();

    public static Subject<int> PlotWatering = new Subject<int>();


    public static Subject<Unit> PlotHammeringPlot = new Subject<Unit>();
    public static Subject<int> PlotHammeringFlower = new Subject<int>();


    public static Subject<int> PlotFertilizer = new Subject<int>();
    public static Subject<Unit> PlotBountyFertilizer = new Subject<Unit>();
    public static Subject<Unit> PlotQualityFertilizer = new Subject<Unit>();

    public static Subject<int> PlotReaping = new Subject<int>();

    #endregion

    #region 나무
    #endregion

    #region 광석
    #endregion

    #region 풀
    #endregion

    #endregion

    // 사용 예제 2번 : 특정 아이템 사용 추적

    //QuestProgressPublisher.UsedItemID
    //.Where(id => id == 105) // 진가 발휘! 105번 아이템일 때만 통과시킵니다.
    //.Subscribe(_ => 
    //{
    //    // 퀘스트 목표 카운트 증가 로직
    //    Debug.Log("퀘스트 목표: 특정 아이템 사용 달성!");
    //})
    //.AddTo(gameObject); // 수명 관리 잊지 마세요!


}
