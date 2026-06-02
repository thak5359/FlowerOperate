using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace Fungus
{
    /// <summary>
    /// A custom Fungus variable that stores a serialized dictionary with int keys and BlockReference values.
    /// </summary>
    [VariableInfo("Collection", "Dictionary (Int, BlockRef)", isPreviewedOnly: false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class DictionaryIntBlockRefVariable : VariableBase<SerializedDictionary<int, BlockReference>>
    {
        public override void OnReset()
        {
            // Dictionaries are not reset automatically on flowchart reset by default.
        }

        public override string ToString()
        {
            int count = (Value != null) ? Value.Count : 0;
            return $"Dictionary ({count} items)";
        }
    }

    /// <summary>
    /// Helper struct for referring to a DictionaryIntBlockRefVariable or a constant value.
    /// </summary>
    [System.Serializable]
    public struct DictionaryIntBlockRefData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(DictionaryIntBlockRefVariable))]
        public DictionaryIntBlockRefVariable dictionaryRef;

        [SerializedDictionary("Key (Int)", "Block (BlockRef)")]
        [SerializeField]
        public SerializedDictionary<int, BlockReference> dictionaryVal;

        public static implicit operator SerializedDictionary<int, BlockReference>(DictionaryIntBlockRefData data)
        {
            return data.Value;
        }

        public DictionaryIntBlockRefData(SerializedDictionary<int, BlockReference> v)
        {
            dictionaryVal = v;
            dictionaryRef = null;
        }

        public SerializedDictionary<int, BlockReference> Value
        {
            get { return (dictionaryRef == null) ? dictionaryVal : dictionaryRef.Value; }
            set { if (dictionaryRef == null) { dictionaryVal = value; } else { dictionaryRef.Value = value; } }
        }

        public string GetDescription()
        {
            if (dictionaryRef == null)
            {
                return dictionaryVal != null ? $"Dictionary ({dictionaryVal.Count} items)" : "Null";
            }
            else
            {
                return dictionaryRef.Key;
            }
        }
    }
}

#if UNITY_EDITOR
namespace Fungus
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(DictionaryIntBlockRefData))]
    public class DictionaryIntBlockRefDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty referenceProp = property.FindPropertyRelative("dictionaryRef");
            SerializedProperty valueProp = property.FindPropertyRelative("dictionaryVal");

            if (referenceProp == null || valueProp == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            // Draw multi-line property for SerializedDictionary
            const int popupWidth = 100;
            
            Rect controlRect = position;
            Rect valueRect = controlRect;
            Rect popupRect = controlRect;
            popupRect.height = EditorGUIUtility.singleLineHeight;
            
            if (referenceProp.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(valueRect, valueProp, label, true);
                popupRect.x = position.width - popupWidth + 5;
                popupRect.width = popupWidth;
            }
            else
            {
                popupRect = EditorGUI.PrefixLabel(position, label);
            }

            EditorGUI.PropertyField(popupRect, referenceProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty referenceProp = property.FindPropertyRelative("dictionaryRef");
            if (referenceProp != null && referenceProp.objectReferenceValue != null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            SerializedProperty valueProp = property.FindPropertyRelative("dictionaryVal");
            if (valueProp != null)
            {
                return EditorGUI.GetPropertyHeight(valueProp, label, true);
            }

            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
