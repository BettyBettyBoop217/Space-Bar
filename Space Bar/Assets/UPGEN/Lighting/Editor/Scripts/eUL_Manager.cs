using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UL_Manager)), CanEditMultipleObjects]
public class eUL_Manager : Editor
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.layersToRayTrace)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.showDebugRays)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.showDebugGUI)));

        if (GUILayout.Button("Clear Cache"))
        {
            UL_Rays.Ray.ClearCache();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        GUILayout.Space(16);
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_Manager.pointLightsIntensityMultiplier)));

        var _target = (UL_Manager)target;
        if (_target.HasPointLights())
        { if (GUILayout.Button("Destroy Point Lights")) _target.DestroyPointLights(); }
        else if (GUILayout.Button("Generate Point Lights")) _target.GeneratePointLights(_target.pointLightsIntensityMultiplier);

        serializedObject.ApplyModifiedProperties();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
