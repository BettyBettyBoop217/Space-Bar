using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN Lighting Manager")]
[ExecuteInEditMode]
public sealed class UL_Manager : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static UL_Manager instance;

    void OnEnable()
    {
        if (instance == null) instance = this;
        else Debug.LogWarning("There are 2 audio UPGEN Lighting Managers in the scene. Please ensure there is always exactly one Manager in the scene.");
    }

    void OnDisable()
    {
        if (instance == this) instance = null;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(0, 2)] public float pointLightsIntensityMultiplier = 1;
    public LayerMask layersToRayTrace = -5;
    public bool showDebugRays;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public bool showDebugGUI = true;

    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUILayout.BeginArea(new Rect(0, 0, 200, Screen.height));
        {
            if (UL_Renderer.HasLightsToRender)
            {
                var cnt = UL_Renderer.FastLightsCount;
                if (cnt > 0) UL_GUI_Utils.Text($"Capacity: <b>{cnt} / {UL_Renderer.MaxFastLightsPerFrameCount}</b>");

                cnt = UL_FastLight.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"Fast Lights: <b>{cnt}</b>");

                cnt = UL_FastGI.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"Fast GI: <b>{cnt}</b>");

                cnt = UL_RayTracedGI.all.Count;
                if (cnt > 0) UL_GUI_Utils.Text($"RayTraced GI: <b>{cnt}</b>");
            }
        }
        GUILayout.EndArea();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public bool HasPointLights()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
            if (transform.GetChild(i).name == "Point Lights")
                return true;
        return false;
    }

    public void DestroyPointLights()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var tr = transform.GetChild(i);
            if (tr.name == "Point Lights")
            {
                DestroyImmediate(tr.gameObject);
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Destroy Point Lights");
#endif
            }
        }
    }

    public void GeneratePointLights(float intensityMultiplier)
    {
        DestroyPointLights();

        var go = new GameObject("Point Lights");
        go.transform.SetParent(transform, false);
        UL_Renderer.CreatePointLights(go.transform, intensityMultiplier);
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Point Lights");
#endif
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}