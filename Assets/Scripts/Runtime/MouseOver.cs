using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Fungus;

public class MouseOver : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    //어디에서 들어왔는지만 감지, 내부 움직임 감지 X
    public void OnPointerEnter(PointerEventData  eventdata)
    {
        Debug.Log($"마우스 올라감 감지됨 : {eventdata}");
    }
    public void OnPointerMove(PointerEventData pointerEventData)
    {
        Debug.Log($"마우스 움직임 감지됨 : {pointerEventData}");
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {

    }
    //TODO : Panel 하나 잡아서 아이템 데이터 띄우는 거 만들기
}



