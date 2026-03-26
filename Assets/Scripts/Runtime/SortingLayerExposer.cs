// MeshRenderer의 소팅 레이어를 수정하는 스크립트
using UnityEngine;

[ExecuteInEditMode]
public class SortingLayerExposer : MonoBehaviour
{
    public string sortingLayerName = "Background";
    public int sortingOrder = 0;

    void Update()
    {
        // 수정 위치: MeshRenderer의 내부 소팅 속성을 강제로 업데이트
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }
    }
}