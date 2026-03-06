using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{

    public SpriteRenderer plotRenderer;
    public SpriteRenderer flowerRenderer;
    public FlowerData flowerdata;


    // 토지의 마지막 활성화된 날짜와 페이즈(또는 시간)
    private int LastActivedDay;
    private int LastActivedPhase;

    //토지의 위치 정보(데이터 처리용)
    [SerializeField] public readonly int ChunkNumber;
    [SerializeField] public readonly int plotNumber;

    // 토지의 인스턴스 데이터 = 저장해야하는거
    public bool isTilled = false; // 땅이 갈렸는가
    public bool isWatered = false; // 물을 뿌렸는가
    public int itemId; // 꽃이 없다면 0, 있다면 몇번 아이템인지
    public bool isFertilized; // 비료를 뿌렸는가
    public int growth; // 꽃의 성장 단계 == item.level
    public int elapsed; // 심고 경과한 날짜 또는 페이즈.

    //OnEnable일때 타 관리 클래스에서 loadData 실행하기!
    public void loadData( bool input_isTilled ,bool input_isWatered, int input_itemID,
        bool input_isFertilized, int input_growth, int input_elapsed)//DB에서 데이터 로드
    {
        isTilled = input_isTilled;
        isWatered = input_isWatered;
        itemId = input_itemID;
        growth = input_growth;
        elapsed = input_elapsed;
    }
    
    //TODO : 다시 활성화 되었을때에 지난 시간만큼 식물을 성장시키거나, 죽이는 코드 작성! 실행은 부모 오브젝트에서 OnDisabled일때 List로 실행시키기.
    private void Refresh()
    {
        
    }



    public int sowSeed(int input_itemId)
    {
        if(itemId == 0)
        {
            itemId = input_itemId;
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
