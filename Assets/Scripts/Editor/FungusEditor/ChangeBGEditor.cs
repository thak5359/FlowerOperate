#if UNITY_EDITOR
using Fungus;
using Fungus.EditorUtils;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ChangeBG))]
public class ChangeBGEditor : CommandEditor
{
    public override void DrawCommandGUI()
    {
        serializedObject.Update();

        var controllerProp = serializedObject.FindProperty("targetController");
        EditorGUILayout.PropertyField(controllerProp);

        BGImageController controller = controllerProp.objectReferenceValue as BGImageController;

        if (controller != null && controller.bg_images.Count > 0)
        {
            List<string> names = new List<string>();
            foreach (var sr in controller.bg_images) if (sr != null) names.Add(sr.name);

            var nameProp = serializedObject.FindProperty("targetImageName");
            int currentIndex = names.IndexOf(nameProp.stringValue);
            if (currentIndex == -1) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Select Image", currentIndex, names.ToArray());
            nameProp.stringValue = names[newIndex];
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif