using R3;
using UnityEngine;

public static class QuestProgressPublisher
{

    public static ReactiveProperty<int> G_Money = new ReactiveProperty<int>(0);

    
    // 아이템 일정 개수 이상 소지
    public static Subject<(int itemID, int amount)> ItemRetaining = new Subject<(int, int)>();
    
    public static Subject<int> ItemObtain = new Subject<int>();

    public static Subject<int> ItemSell = new Subject<int>();



    // 특정 번호의 소모성 아이템 사용
    public static Subject<int> UsedItemID = new Subject<int>();
    // 아이템 납품
    public static Subject<int> SubmissItemID = new Subject<int>();

    // 밭에 물 주기
    public static Subject<int> PlotWatering = new Subject<int>();

    public static Subject<int> PlotSowing = new Subject<int>();
    
    public static Subject<int> PlotHammeringFlower = new Subject<int>();
    public static Subject<int> PlotHammeringPlot = new Subject<int>();

    public static Subject<int> PlotBountyFertilizer = new Subject<int>();





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
