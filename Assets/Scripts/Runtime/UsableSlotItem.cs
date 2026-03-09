using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HoeItem : SlotItem
{
    public int currentDuration = 100; // 내구도 기본값

    // 오류 해결: 부모 생성자(base)를 호출해야 합니다.
    public HoeItem(int? id, int count) : base(id, count)
    {
        // 괭이 전용 초기화가 필요하다면 여기서 수행
    }

    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            Debug.Log("내구도가 다해서 괭이를 휘둘 수 없습니다!");
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
        Debug.Log($"괭이질 실행! 남은 내구도: {currentDuration}");
    }
}
