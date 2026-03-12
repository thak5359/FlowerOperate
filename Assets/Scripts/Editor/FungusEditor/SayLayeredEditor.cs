using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Fungus;
using Fungus.EditorUtils;

namespace Fungus
{
    [CustomEditor(typeof(SayLayered))]
    public class SayLayeredEditor : SayEditor
    {
        public override void DrawCommandGUI()
        {
            // 1. 기존 Say UI (캐릭터 선택, 대사창 등) 출력
            base.DrawCommandGUI();

            serializedObject.Update();
            SayLayered t = target as SayLayered;

            // 2. 캐릭터가 지정되어 있고 LayeredCharacter인지 확인
            if (t._Character == null) return;
            LayeredCharacter layeredChar = t._Character as LayeredCharacter;
            if (layeredChar == null) return;

            // 3. 콤보박스 리스트 생성 (1번 인덱스부터)
            List<string> portraitNames = new List<string>();
            portraitNames.Add("<None>");

            if (layeredChar.FacePortraits != null)
            {
                foreach (var s in layeredChar.FacePortraits)
                {
                    if (s != null) portraitNames.Add(s.name);
                }
            }

            // 4. 드롭다운 그리기
            int currentIndex = portraitNames.IndexOf(t.facePortraitName);
            if (currentIndex == -1) currentIndex = 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Change Face");
            int newIndex = EditorGUILayout.Popup(currentIndex, portraitNames.ToArray());
            EditorGUILayout.EndHorizontal();

            if (newIndex != currentIndex)
            {
                Undo.RecordObject(t, "Change Face Selection");
                t.facePortraitName = (newIndex == 0) ? "" : portraitNames[newIndex];
                EditorUtility.SetDirty(t);
            }

            // 5. [복구] 선택된 표정 미리보기 그리기
            if (!string.IsNullOrEmpty(t.facePortraitName))
            {
                Sprite selectedFace = layeredChar.GetFacePortrait(t.facePortraitName);
                if (selectedFace != null)
                {
                    DrawFacePreview(selectedFace);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        // 아틀라스 텍스처에서 해당 스프라이트 조각만 뽑아서 그리는 함수
        private void DrawFacePreview(Sprite sprite)
        {
            EditorGUILayout.Space(10);

            // 스프라이트 비율에 맞게 영역 잡기 (가로 120px 기준)
            Rect rect = sprite.rect;
            float aspect = rect.width / rect.height;
            Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(120), GUILayout.ExpandWidth(true));

            // 아틀라스 텍스처 내의 상대적 좌표(UV) 계산
            var tex = sprite.texture;
            Rect uv = new Rect(rect.x / tex.width, rect.y / tex.height, rect.width / tex.width, rect.height / tex.height);

            // 인스펙터에 그리기
            GUI.Box(previewRect, ""); // 배경 박스
            GUI.DrawTextureWithTexCoords(previewRect, tex, uv, true);

            EditorGUILayout.LabelField($"Selected: {sprite.name}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(10);
        }
    }
}