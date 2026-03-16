// HeartColliderBaker.cs (부모 오브젝트용)
using UnityEngine;

public class ColliderBaker : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D sourcePoly; // 자식의 PolygonCollider2D를 드래그해서 넣어주세요.

    void Awake()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        if (sourcePoly != null && meshCollider != null)
        {
            // 1. 자식의 2D 데이터를 기반으로 3D 메시 생성
            Mesh heartMesh = sourcePoly.CreateMesh(false, false);

            Debug.Log($"heartMesh : {(heartMesh!=null? true: false)}");
            // 2. 생성된 메시를 부모의 Mesh Collider에 할당
            meshCollider.sharedMesh = heartMesh;

            Debug.Log($"meshCollider : {(meshCollider != null ? true : false)}");
            // 3. 사용이 끝난 자식 오브젝트는 꺼버립니다. (최적화)
            sourcePoly.gameObject.SetActive(false);

            Debug.Log("파트너! 자식의 데이터를 빌려와서 하트 3D 콜라이더를 만들었습니다.");
        }
    }
}