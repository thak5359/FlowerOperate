using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPhysics : MonoBehaviour
{
    // Start is called before the first frame update
    // DebugPhysics.cs
    void OnDrawGizmos()
    {
        // Scene 뷰에서 플레이어 주변에 상호작용 감지 범위를 시각적으로 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
