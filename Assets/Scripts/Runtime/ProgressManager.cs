using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Season
{
    SPRING,
    SUMMER,
    AUTUMN,
    WINTER
}

public class ProgressManager : MonoBehaviour
{
    List<int> list;
    //시간을 관리하는 매니저
    public static ProgressManager Instance;

    //13개월, 월 28일
    private int Day = 1; // 몇일차
    private int totalDay = 364;
    // 일정 관리하는 SO 데이터
    // 날짜 관리하는 알고리즘

    public string getDate() //날짜 반환
    {
        return $"{(Day / totalDay)+ 1}년차, {(Day - 1) / 28 + 1}월 {(Day - 1) % 28 + 1}일";
    }

    public int getDay()
    {
        return Day;
    }

    public void nextDay()
    {
        Day = Day + 1;
    }

    public Season getSeason(int day)  //날씨 enum 반환
    {
        float dayRatio = (day%totalDay)/totalDay;

        if (dayRatio > 3.25f && dayRatio <= 6.5f)
            return Season.SUMMER;
        else if(dayRatio <= 9.75f)
            return Season.AUTUMN;
        else if (dayRatio <= 13f)
            return Season.WINTER;
        else
            return Season.SPRING;
    }

    public struct ProgressData
    {
        private int day;
        // 그 외에 필요한 인스턴트 데이터
        public int Day => day;
    }

    public void LoadData(ProgressData saveData) // 세이브/로드 관리하는 쪽에서 진행 상황 불러오기
    {
        Day = saveData.Day;
    }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else { Destroy(Instance); }
    }
}
