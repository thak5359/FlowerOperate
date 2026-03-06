using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{
    //토지의 위치 정보(데이터 처리용)
    public int StageNumber;
    public int plotNumber;

    // 토지의 인스턴스 데이터 = 저장해야하는거
    public bool isTilled = false; // 땅이 갈렸는가
    public bool isWatered = false; // 물을 뿌렸는가
    public int itemId; // 꽃이 없다면 0, 있다면 몇번 아이템인지
    public bool isFertilized; // 비료를 뿌렸는가
    public int growth; // 꽃의 성장 단계 == item.level
    public int Days; // 심고 경과한 날짜
    
    








}
