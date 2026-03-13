using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// # 아이템 사용 알고리즘은 향후 변경될 수 있음.

[System.Serializable]
public class HoeItem : Item
{
    public int currentDuration = 100; // 내구도 기본값

    public HoeItem(int? id, int count) : base(id, count)
    {
        // 별도 초기화
    }

    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
        #if UNITY_EDITOR

            Debug.Log("내구도가 다해서 괭이를 휘둘 수 없습니다!");
        #endif
            //TODO: 사용불가 UI 팝업 띄우기
            return;
        }

        // 차징 시간에 따른 범위 계산 로직 (아래 PlayerController와 연동)
        ExecuteHoeAction(param);
    }

    private void ExecuteHoeAction(UseParam param)
    {
        // 1. 차징 단계에 따른 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 감지
        // 3. ObjectPool에서 흙 프리팹 등을 가져와 배치

        currentDuration--; // 사용 시 내구도 감소

        #if UNITY_EDITOR
        Debug.Log($"괭이질 실행! 남은 내구도: {currentDuration}");
        #endif
    }
}

public class HammerItem : Item
{
    public int currentDuration = 100;
    public HammerItem(int? id, int count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("내구도가 다해서 망치를 휘둘 수 없습니다!");
            #endif 
            return;
        }

        // 차징 시간에 따른 범위 계산 로직 (아래 PlayerController와 연동)
        ExecuteHammerAction(param);
    }

    private void ExecuteHammerAction(UseParam param)
    {
        // 1. 차징 단계에 따른 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 감지
        // 3. ObjectPool에서 흙 프리팹 등을 가져와 배치



        currentDuration--; // 사용 시 내구도 감소
        #if UNITY_EDITOR
        Debug.Log($"망치질 실행! 남은 내구도: {currentDuration}");
        #endif
    }
}

public class WateringCanItem : Item
{
    public int currentDuration = 100;
    public WateringCanItem(int? id, int count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("내구도가 다해서 물뿌리개를 사용 할 수 없습니다!");
            #endif
            //TODO
            return;
        }

        // 차징 시간에 따른 범위 계산 로직 (아래 PlayerController와 연동)
        ExcuseWateringCanAction(param);
    }

    private void ExcuseWateringCanAction(UseParam param)
    {
        // 1. 차징 단계에 따른 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 감지
        // 3. ObjectPool에서 흙 프리팹 등을 가져와 배치
        #if UNITY_EDITOR
        currentDuration--; // 사용 시 내구도 감소
        Debug.Log($"해수스파우팅! 남은 내구도: {currentDuration}");
        #endif
    }


}
public class ConsumableSlotItem : Item
{
    public ConsumableSlotItem(int? id, int count) : base(id, count)
    {
    }
    public override void OnUse(UseParam param)
    {
        // 차징 시간에 따른 범위 계산 로직 (아래 PlayerController와 연동)
        ExcuseWateringCanAction(param);
        if (amount == 0)
        {
            Debug.Log("아이템을 모두 사용 했습니다!");
            Cleanup();
            return;
        }
    }

    private void ExcuseWateringCanAction(UseParam param)
    {
        // 1. 차징 단계에 따른 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 감지
        // 3. ObjectPool에서 흙 프리팹 등을 가져와 배치

        amount--; // 사용 시 내구도 감소

        #if UNITY_EDITOR
        Debug.Log($"아이템 사용. 남은 아이템 개수: {amount}");
        #endif
    }
}

