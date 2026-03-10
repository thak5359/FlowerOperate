using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Fungus;
using Fungus.EditorUtils;

namespace Fungus
{
    [CustomEditor(typeof(SayMulti))]
    public class SayMultiEditor : SayEditor
    {
        protected SerializedProperty multiCharsProp;
        protected SerializedProperty customNameProp;
        protected SerializedProperty nameColorProp;

        public override void OnEnable()
        {
            base.OnEnable();
            multiCharsProp = serializedObject.FindProperty("multiCharacters");
            customNameProp = serializedObject.FindProperty("customCharacterName");
            nameColorProp = serializedObject.FindProperty("nameColor"); 
        }

        public override void DrawCommandGUI()
        {
            // 1. 기본 Say UI (텍스트 입력창 등)를 먼저 그림
            // (기존의 단일 character 필드는 숨기고 싶다면 DrawSayGUI를 직접 재구성해야 합니다)
            base.DrawCommandGUI();

            serializedObject.Update();
            SayMulti t = target as SayMulti;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Multi-Character Settings", EditorStyles.boldLabel);

            // 커스텀 이름 설정창
            EditorGUILayout.PropertyField(customNameProp, new GUIContent("Display Name Override"));
            EditorGUILayout.PropertyField(nameColorProp, new GUIContent("Name Text Color"));

            // 최대 3명까지만 리스트 관리
            for (int i = 0; i < 3; i++)
            {
                if (multiCharsProp.arraySize <= i) multiCharsProp.InsertArrayElementAtIndex(i);

                SerializedProperty element = multiCharsProp.GetArrayElementAtIndex(i);
                SerializedProperty charProp = element.FindPropertyRelative("character");
                SerializedProperty useLayerProp = element.FindPropertyRelative("useLayeredFace");
                SerializedProperty faceNameProp = element.FindPropertyRelative("facePortraitName");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Character {i + 1}", EditorStyles.miniBoldLabel);

                EditorGUILayout.PropertyField(charProp, new GUIContent("Character"));
                EditorGUILayout.PropertyField(useLayerProp, new GUIContent("Use Layered Face"));

                if (useLayerProp.boolValue && charProp.objectReferenceValue != null)
                {
                    LayeredCharacter lc = charProp.objectReferenceValue as LayeredCharacter;
                    if (lc != null)
                    {
                        // 콤보박스 로직
                        List<string> portraitNames = new List<string> { "<None>" };
                        foreach (var s in lc.FacePortraits) if (s != null) portraitNames.Add(s.name);

                        int currentIndex = portraitNames.IndexOf(faceNameProp.stringValue);
                        if (currentIndex == -1) currentIndex = 0;

                        int newIndex = EditorGUILayout.Popup("Face Portrait", currentIndex, portraitNames.ToArray());
                        faceNameProp.stringValue = (newIndex == 0) ? "" : portraitNames[newIndex];
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("이 캐릭터는 LayeredCharacter가 아닙니다.", MessageType.None);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}