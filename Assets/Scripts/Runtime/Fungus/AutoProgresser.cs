using UnityEngine;
using Fungus;

public class AutoProgresser : MonoBehaviour
{
    //// CustomWriter는 Writer의 자식이므로, Writer의 모든 기능을 포함합니다.
    //public CustomWriter customWriter;
    //private float timer = 0f;
    //[SerializeField]
    //public float extraWaitPerCharacter = 0.01f; // 글자당 추가 대기 시간 (조정 가능)

    //void Awake()
    //{
    //    // 인스펙터에서 할당하지 않았을 경우를 대비해 자동으로 찾아옵니다.
    //    if (customWriter == null)
    //        customWriter = GetComponent<CustomWriter>();
    //}
    //void Update()
    //{
    //    // 1. 오토 모드 체크 (OptionManager 상태 확인)
    //    if (OptionManager.Instance == null || !OptionManager.Instance.settings.isAutoMode == true)
    //    {
    //        timer = 0f;
    //        return;
    //    }

    //    // 2. [핵심] Writer가 입력을 기다리는 상태(isWaitingForInput)인지 확인
    //    // 이 변수는 부모인 Writer 클래스에 정의되어 있어 CustomWriter에서도 바로 쓸 수 있습니다.
    //    if (customWriter != null && customWriter.IsWaitingForInput)
    //    {
    //        timer += Time.deltaTime;

    //        // 대기 시간 계산: 기본 설정값 + (글자 수 * 가중치)
    //        float baseWait = (OptionManager.Instance.settings.puncSliderRaw+1)*0.05f;
    //        float extraWait = customWriter.VisibleCharacterCount * extraWaitPerCharacter;
    //        float totalWaitTime = baseWait + extraWait;


    //        if (timer >= totalWaitTime)
    //        {
    //            timer = 0f;
    //            customWriter.ForceInput(); // 우리가 만든 강제 진행 함수 호출
    //        }
    //    }
    //    else
    //    {
    //        // 타이핑 중이거나 대기 상태가 아니면 타이머 초기화
    //        timer = 0f;
    //    }
    //}
}