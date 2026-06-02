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


public static partial class ProgressManager
{
    //13개월, 월 28일
    private static int Day = 999; // 몇일차
    private static int DayInYear = 364;
    // 일정 관리하는 SO 데이터
    // 날짜 관리하는 알고리즘

    public static int getYear()
    {
        return (Day / DayInYear) + 3026;
    }

    public static int getMonth() => ((Day - 1) % DayInYear / 28 + 1);
    public static int getDay() => (Day - 1) % 28 + 1;

    public static int getPlayedDayOnGameSystem()
    {
        return Day;
    }

    public static void nextDay()
    {
        Day = Day + 1;
    }

    public static Season getSeason(int day)  //날씨 enum 반환
    {
        float dayRatio = (day%DayInYear)/DayInYear;

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

    public static void LoadData(ProgressData saveData) // 세이브/로드 관리하는 쪽에서 진행 상황 불러오기
    {
        Day = saveData.Day;
    }
}
