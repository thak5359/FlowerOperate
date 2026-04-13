using UnityEngine;
using UnityEngine.UI;
using Fungus;
using System.Collections.Generic;

namespace Fungus
{
    public class LayeredCharacter : Character
    {
        [Header("Layered Face Parts")]
        [SerializeField] protected List<Sprite> facePortraits = new List<Sprite>();
        public List<Sprite> FacePortraits => facePortraits;

        [Header("Adjustment Settings")]
        public Vector2 faceOffset = Vector2.zero;
        [Range(0.1f, 5f)] public float faceScale = 1.0f;
        public float bodyTargetHeight = 200f;

        public const string FaceLayerName = "Face_Layer";

        // 얼굴을 적용하고 피벗을 맞추는 핵심 함수
        public void ApplyFace(string portraitName)
        {
            if (string.IsNullOrEmpty(portraitName) || portraitName == "<None>") return;

            Sprite faceSprite = GetFacePortrait(portraitName);
            if (faceSprite == null) return;

            // 레이어를 가져오거나 생성
            Component faceComp = GetOrCreateFaceLayer();
            if (faceComp == null) return;

            if (faceComp is Image img)
            {
                img.sprite = faceSprite;
                img.SetNativeSize();

                RectTransform faceRT = img.rectTransform;

                // [핵심] 스프라이트 에디터에서 찍은 피벗(코)을 UI 피벗으로 변환
                // Pivot(Pixel) / Rect(Pixel) = Normalized Pivot(0~1)
                Vector2 normalizedPivot = new Vector2(
                    faceSprite.pivot.x / faceSprite.rect.width,
                    faceSprite.pivot.y / faceSprite.rect.height
                );

                faceRT.pivot = normalizedPivot; // 이제 '코'가 기준점이 됩니다.
                faceRT.localScale = new Vector3(faceScale, faceScale, 1f);
                faceRT.anchoredPosition = faceOffset;
            }
        }

        public Component GetOrCreateFaceLayer()
        {
            if (State.holder == null) return null;

            Transform activeBody = null;
            foreach (Transform child in State.holder.transform)
            {
                if (child.gameObject.activeSelf) { activeBody = child; break; }
            }
            if (activeBody == null) return null;

            // 1. 몸체 교정 (기존 로직 유지)
            Image bodyImg = activeBody.GetComponent<Image>();
            if (bodyImg != null && bodyImg.sprite != null)
            {
                bodyImg.preserveAspect = true;
                RectTransform bodyRT = activeBody.GetComponent<RectTransform>();
                Sprite s = bodyImg.sprite;
                float aspectRatio = s.rect.width / s.rect.height;
                bodyRT.sizeDelta = new Vector2(bodyTargetHeight * aspectRatio, bodyTargetHeight);
                bodyRT.pivot = new Vector2(s.pivot.x / s.rect.width, s.pivot.y / s.rect.height);
                bodyRT.anchoredPosition = Vector2.zero;
            }

            // 2. 얼굴 레이어 생성
            Transform faceTransform = activeBody.Find(FaceLayerName);
            bool isUI = bodyImg != null;

            if (faceTransform == null)
            {
                GameObject faceObj = new GameObject(FaceLayerName);
                faceObj.transform.SetParent(activeBody);
                faceObj.transform.localPosition = Vector3.zero;
                faceObj.transform.localScale = Vector3.one;

                if (isUI)
                {
                    var img = faceObj.AddComponent<Image>();
                    img.raycastTarget = false;
                    img.preserveAspect = true;

                    var sync = faceObj.AddComponent<FaceColorSync>();
                    sync.parentImage = bodyImg; // 몸체 이미지를 부모로 설정

                    RectTransform faceRT = faceObj.GetComponent<RectTransform>();
                    faceRT.anchorMin = new Vector2(0.5f, 0.5f);
                    faceRT.anchorMax = new Vector2(0.5f, 0.5f);
                    // 초기 피벗은 ApplyFace에서 갱신하므로 일단 중앙 설정
                    faceRT.pivot = new Vector2(0.5f, 0.5f);
                }
                faceTransform = faceObj.transform;
            }

            return isUI ? (Component)faceTransform.GetComponent<Image>() : faceTransform.GetComponent<SpriteRenderer>();
        }

        public virtual Sprite GetFacePortrait(string portraitName)
        {
            if (string.IsNullOrEmpty(portraitName)) return null;
            return facePortraits.Find(s => s != null && string.Compare(s.name, portraitName, true) == 0);
        }
    }
}