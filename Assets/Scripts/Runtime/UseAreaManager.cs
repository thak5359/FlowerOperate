using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAreamanager : MonoBehaviour
{

    [Header("기본 범위를 넣어주세요!")]
    [SerializeField]public GameObject defaultRange;

    [Header("범위 1의 1단계를 넣어주세요!")]
    [SerializeField] public GameObject Type1Range1;

    [Header("범위 1의 2단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type1Range2 = new(2);

    [Header("범위 1의 3단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type1Range3 = new(5);

    [Header("범위 1의 4단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type1Range4 = new(8);

    [Header("범위 1의 5단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type1Range5 = new(24);


    [Header("범위 2의 1단계를 넣어주세요!")]
    [SerializeField] public GameObject Type2Range1;

    [Header("범위 2의 2단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type2Range2 = new(2);

    [Header("범위 2의 3단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type2Range3 = new(5);

    [Header("범위 2의 4단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type2Range4 = new(8);

    [Header("범위 2의 5단계를 넣어주세요!")]
    [SerializeField] public List<GameObject> Type2Range5 = new(24);

    // 이런 형식으로 범위 등록하기

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

}
