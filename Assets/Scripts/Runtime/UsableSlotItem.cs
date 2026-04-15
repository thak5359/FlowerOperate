using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// # 아이템 사용 알고리즘에 따라 서브 클래스를 나눌 예정.

[System.Serializable]
public class HoeItem : Item
{
    public int currentDuration = 100; // 내구도 기본값

    public HoeItem(ushort id, short count) : base(id, count)
    {
        // 변수 초기화
    }

    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
        #if UNITY_EDITOR
            Debug.Log("내구도가 부족해서 괭이질을 할 수 없습니다!");
        #endif
            //TODO: 내구도 부족 UI 팝업 연결
            return;
        }

        // 차징 시간에 따라 공격 범위 설정 (아래 PlayerController와 연동)
        ExecuteHoeAction(param);
    }

    private void ExecuteHoeAction(UseParam param)
    {
        // 1. 차징 단계에 따라 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 선택
        // 3. ObjectPool에서 각 아이템에 맞는 결과물들을 배치

        currentDuration--; // 사용 시 내구도 감소

        #if UNITY_EDITOR
        Debug.Log($"괭이질 성공! 현재 내구도: {currentDuration}");
        #endif
    }
}

public class HammerItem : Item
{
    public int currentDuration = 100;
    public HammerItem(ushort id, short count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("내구도가 부족해서 망치를 휘두를 수 없습니다!");
            #endif 
            return;
        }

        // 차징 시간에 따라 공격 범위 설정 (아래 PlayerController와 연동)
        ExecuteHammerAction(param);
    }

    private void ExecuteHammerAction(UseParam param)
    {
        // 1. 차징 단계에 따라 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 선택
        // 3. ObjectPool에서 각 아이템에 맞는 결과물들을 배치

        currentDuration--; // 사용 시 내구도 감소
        #if UNITY_EDITOR
        Debug.Log($"망치질 성공! 현재 내구도: {currentDuration}");
        #endif
    }
}

public class WateringCanItem : Item
{
    public int currentDuration = 100;
    public WateringCanItem(ushort id, short count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("내구도가 부족해서 물뿌리개를 사용할 수 없습니다!");
            #endif
            //TODO
            return;
        }

        // 차징 시간에 따라 공격 범위 설정 (아래 PlayerController와 연동)
        ExecuteWateringCanAction(param);
    }

    private void ExecuteWateringCanAction(UseParam param)
    {
        // 1. 차징 단계에 따라 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 선택
        // 3. ObjectPool에서 각 아이템에 맞는 결과물들을 배치
        #if UNITY_EDITOR
        currentDuration--; // 사용 시 내구도 감소
        Debug.Log($"물뿌리기 성공! 현재 내구도: {currentDuration}");
        #endif
    }
}

public class ConsumableSlotItem : Item
{
    public ConsumableSlotItem(ushort id, short count) : base(id, count)
    {
    }
    public override void OnUse(UseParam param)
    {
        // 차징 시간에 따라 공격 범위 설정 (아래 PlayerController와 연동)
        ExecuteConsumableAction(param);
        if (amount == 0)
        {
            Debug.Log("아이템을 모두 사용하였습니다!");
            Cleanup();
            return;
        }
    }

    private void ExecuteConsumableAction(UseParam param)
    {
        // 1. 차징 단계에 따라 범위 결정 (예: 1단계=1x1, 2단계=1x3 ...)
        // 2. SelectionArea를 이용한 타일 선택
        // 3. ObjectPool에서 각 아이템에 맞는 결과물들을 배치

        amount--; // 사용 시 수량 감소

        #if UNITY_EDITOR
        Debug.Log($"소모품 사용 완료. 남은 개수: {amount}");
        #endif
    }
}
