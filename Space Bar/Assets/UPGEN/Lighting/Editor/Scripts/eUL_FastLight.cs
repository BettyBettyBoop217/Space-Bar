using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UL_FastLight)), CanEditMultipleObjects]
public class eUL_FastLight : Editor
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override void OnInspectorGUI()
    {
        var fastLight = (UL_FastLight)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.range)));
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.colorTemperature)));
            if (GUILayout.Button("X", GUILayout.Width(18))) serializedObject.FindProperty(nameof(UL_FastLight.colorTemperature)).floatValue = 6570;
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.color)));

        var intensityProperty = serializedObject.FindProperty(nameof(UL_FastLight.intensity));
        var unitProperty = serializedObject.FindProperty(nameof(UL_FastLight.unit));
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(intensityProperty);

            GUI.changed = false;
            var oldUnit = unitProperty.enumValueIndex;
            EditorGUILayout.PropertyField(unitProperty, GUIContent.none, GUILayout.Width(64));
            if (GUI.changed) intensityProperty.floatValue = ConvertIntensity(intensityProperty.floatValue, oldUnit, unitProperty.enumValueIndex);
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.subtractive)));

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.softCullingDistance)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UL_FastLight.hardCullingDistance)));

        if (fastLight.softCullingDistance > fastLight.hardCullingDistance - 0.1f) EditorGUILayout.HelpBox("Culling is disabled", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static float ConvertIntensity(float intensity, int oldUnit, int newUnit)
    {
        if (oldUnit == 0) // Lumen
            switch (newUnit)
            {
                case 1: return intensity / (4 * Mathf.PI); // Candela
                case 2: return intensity / (4 * Mathf.PI); // Lux
                case 3: return intensity / (4 * Mathf.PI); // Nits
                case 4: return Mathf.Log(intensity / (4 * Mathf.PI) * 100f / 12.5f, 2); // Candela > Ev100
            }
        else if (oldUnit == 1) // Candela
            switch (newUnit)
            {
                case 0: return intensity * (4 * Mathf.PI); // Lumen
                case 4: return Mathf.Log(intensity * 100f / 12.5f, 2); // Ev100
            }
        else if (oldUnit == 2) // Lux
            switch (newUnit)
            {
                case 0: return intensity * (4 * Mathf.PI); // Lumen
                case 4: return Mathf.Log(intensity * 100f / 12.5f, 2); // Ev100
            }
        else if (oldUnit == 3) // Nits
            switch (newUnit)
            {
                case 0: return intensity * (4 * Mathf.PI); // Lumen
                case 4: return Mathf.Log(intensity * 100f / 12.5f, 2); // Ev100
            }
        else if (oldUnit == 4) // Ev100
            switch (newUnit)
            {
                case 0: return Mathf.Pow(2, intensity - 3) * (4 * Mathf.PI); // Lumen
                case 1: return Mathf.Pow(2, intensity - 3); // Candela
                case 2: return Mathf.Pow(2, intensity - 3); // Lux
                case 3: return Mathf.Pow(2, intensity - 3); // Nits
            }

        return intensity;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
