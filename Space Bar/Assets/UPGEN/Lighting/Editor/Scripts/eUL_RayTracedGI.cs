using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UL_RayTracedGI)), CanEditMultipleObjects]
public class eUL_RayTracedGI : Editor
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        var rayTracedGI = (UL_RayTracedGI)target;
        var baseLight = rayTracedGI.BaseLight;
        if (baseLight == null) return;
        if (baseLight.type != LightType.Spot && baseLight.type != LightType.Point)
        {
            EditorGUILayout.HelpBox("Unsupported Light Type", MessageType.Warning);
            return;
        }
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_RayTracedGI.intensity)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_RayTracedGI.raysMatrixSize)));

        if (rayTracedGI.enabled)
        { if (GUILayout.Button("Expose Fast Lights")) rayTracedGI.ExposeFastLights(); }
        else if (GUILayout.Button("Collapse Fast Lights")) rayTracedGI.CollapseFastLights();

        if (GUILayout.Button("Clear Cache"))
        {
            UL_Rays.Ray.ClearCache();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.softCullingDistance)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastGI.hardCullingDistance)));

        if (rayTracedGI.softCullingDistance > rayTracedGI.hardCullingDistance - 0.1f) EditorGUILayout.HelpBox("Culling is disabled", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
