using UnityEngine;
using UnityEngine.UI;

public class AutoIconController : MonoBehaviour
{
    [Tooltip("초당 회전 속도")]
    public float rotateSpeed = 200f;

    public Image iconImage;

    private bool isTurned;


    //void Update()
    //{
    //    // OptionManager의 상태를 실시간 감시
    //    if (OptionManager.Instance != null && OptionManager.Instance.settings.isAutoMode == true)
    //    {
    //        // 1. 아이콘 표시
    //        if (!iconImage.enabled) iconImage.enabled = true;

    //        // 2. Z축 기준 회전 (시계 방향은 음수)
    //        transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
    //        isTurned = true;
    //    }
    //    else
    //    {
    //        if ( isTurned == true)
    //        {
    //            transform.localRotation= Quaternion.identity;
    //            isTurned = false;
    //        }
    //    }
    //}
}
