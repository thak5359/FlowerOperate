using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{
    public class FaceColorSync : MonoBehaviour
    {
        public Image parentImage;
        private Image myImage;
        private Color lastColor; // 이전 프레임의 색상을 기억

        void Start()
        {
            myImage = GetComponent<Image>();
            if (parentImage != null) lastColor = parentImage.color;
        }

        void LateUpdate()
        {
            if (parentImage == null || myImage == null) return;

            // [최적화] 부모의 색상이 바뀌었을 때만 내 색상을 업데이트
            // 단순히 색상을 대입하는 것보다 "비교"가 CPU 입장에선 더 빠를 때가 많습니다.
            if (parentImage.color != lastColor)
            {
                myImage.color = parentImage.color;
                lastColor = parentImage.color;
            }
        }
    }
}