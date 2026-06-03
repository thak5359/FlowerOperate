using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

namespace Fungus
{
    /// <summary>
    /// A simple struct wrapping a reference to a Fungus Command.
    /// </summary>
    [System.Serializable]
    public struct CommandReference
    {
        [SerializeField]
        public Command command;

        public static implicit operator Command(CommandReference reference)
        {
            return reference.command;
        }
    }

    /// <summary>
    /// A custom Fungus variable that stores a serialized dictionary with int keys and CommandReference values.
    /// </summary>
    [VariableInfo("Collection", "Dictionary (Int, Command)", isPreviewedOnly: false)]
    [AddComponentMenu("")]
    [System.Serializable]
    public class DictionaryIntCommandVariable : VariableBase<SerializedDictionary<int, CommandReference>>
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
    /// Helper struct for referring to a DictionaryIntCommandVariable or a constant value.
    /// </summary>
    [System.Serializable]
    public struct DictionaryIntCommandData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(DictionaryIntCommandVariable))]
        public DictionaryIntCommandVariable dictionaryRef;

        [SerializedDictionary("Key (Int)", "Command (Ref)")]
        [SerializeField]
        public SerializedDictionary<int, CommandReference> dictionaryVal;

        public static implicit operator SerializedDictionary<int, CommandReference>(DictionaryIntCommandData data)
        {
            return data.Value;
        }

        public DictionaryIntCommandData(SerializedDictionary<int, CommandReference> v)
        {
            dictionaryVal = v;
            dictionaryRef = null;
        }

        public SerializedDictionary<int, CommandReference> Value
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

    [CustomPropertyDrawer(typeof(CommandReference))]
    public class CommandReferenceDrawer : PropertyDrawer
    {
        private static Dictionary<string, Flowchart> flowchartCache = new Dictionary<string, Flowchart>();
        private static Dictionary<string, Block> blockCache = new Dictionary<string, Block>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Render label
            position = EditorGUI.PrefixLabel(position, label);

            string path = property.propertyPath;
            var commandProp = property.FindPropertyRelative("command");
            Command currentCommand = commandProp.objectReferenceValue as Command;

            Flowchart flowchart = null;
            Block block = null;

            if (currentCommand != null)
            {
                flowchart = currentCommand.GetFlowchart();
                if (flowchart != null)
                {
                    foreach (var b in flowchart.GetComponents<Block>())
                    {
                        if (b.CommandList.Contains(currentCommand))
                        {
                            block = b;
                            break;
                        }
                    }
                }
                flowchartCache[path] = flowchart;
                blockCache[path] = block;
            }
            else
            {
                flowchartCache.TryGetValue(path, out flowchart);
                blockCache.TryGetValue(path, out block);
            }

            float singleHeight = EditorGUIUtility.singleLineHeight;
            Rect drawRect = new Rect(position.x, position.y, position.width, singleHeight);

            // 1. Flowchart Field
            EditorGUI.BeginChangeCheck();
            flowchart = EditorGUI.ObjectField(drawRect, flowchart, typeof(Flowchart), true) as Flowchart;
            if (EditorGUI.EndChangeCheck())
            {
                flowchartCache[path] = flowchart;
                block = null;
                blockCache[path] = null;
                commandProp.objectReferenceValue = null;
            }

            drawRect.y += singleHeight + 2;

            // 2. Block Field
            if (flowchart != null)
            {
                EditorGUI.BeginChangeCheck();
                block = Fungus.EditorUtils.BlockEditor.BlockField(drawRect, new GUIContent("None"), flowchart, block);
                if (EditorGUI.EndChangeCheck())
                {
                    blockCache[path] = block;
                    commandProp.objectReferenceValue = null;
                }
            }
            else
            {
                EditorGUI.LabelField(drawRect, "Select Flowchart");
                block = null;
            }

            drawRect.y += singleHeight + 2;

            // 3. Command Field
            if (block != null)
            {
                List<GUIContent> commandNames = new List<GUIContent>();
                commandNames.Add(new GUIContent("None"));
                var commands = block.CommandList;
                int selectedIndex = 0;

                for (int i = 0; i < commands.Count; i++)
                {
                    var cmd = commands[i];
                    if (cmd == null) continue;

                    string summary = cmd.GetSummary();
                    string labelText = $"[{i}] {cmd.GetType().Name}";
                    if (!string.IsNullOrEmpty(summary))
                    {
                        labelText += $" ({summary})";
                    }
                    commandNames.Add(new GUIContent(labelText));

                    if (currentCommand == cmd)
                    {
                        selectedIndex = i + 1;
                    }
                }

                EditorGUI.BeginChangeCheck();
                selectedIndex = EditorGUI.Popup(drawRect, selectedIndex, commandNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    if (selectedIndex == 0)
                    {
                        commandProp.objectReferenceValue = null;
                    }
                    else
                    {
                        commandProp.objectReferenceValue = commands[selectedIndex - 1];
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(drawRect, "Select Block");
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + 2) * 3;
        }
    }

    [CustomPropertyDrawer(typeof(DictionaryIntCommandData))]
    public class DictionaryIntCommandDataDrawer : PropertyDrawer
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
