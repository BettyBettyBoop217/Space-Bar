using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UL_FastGI)), CanEditMultipleObjects]
public class eUL_FastGI : Editor
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        var fastGI = (UL_FastGI)target;
        var baseLight = fastGI.BaseLight;
        if (baseLight == null) return;
        if (baseLight.type != LightType.Spot && baseLight.type != LightType.Point)
        {
            EditorGUILayout.HelpBox("Unsupported Light Type", MessageType.Warning);
            return;
        }
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.expand)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.intensity)));

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.softCullingDistance)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.hardCullingDistance)));

        if (fastGI.softCullingDistance > fastGI.hardCullingDistance - 0.1f) EditorGUILayout.HelpBox("Culling is disabled", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
