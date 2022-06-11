using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public static class UL_Renderer
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static bool HasLightsToRender => UL_FastLight.all.Count > 0 || UL_FastGI.all.Count > 0 || UL_RayTracedGI.all.Count > 0;
    public static int FastLightsCount => _fastLightsCount;
    public static int MaxFastLightsPerFrameCount => MAX_FAST_LIGHTS_PER_FRAME;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private const int MAX_FAST_LIGHTS_PER_FRAME = 96;
    private static int _fastLightsCount;
    private static readonly Vector4[] _fastLightsPositions = new Vector4[MAX_FAST_LIGHTS_PER_FRAME];
    private static readonly Vector4[] _fastLightsColors = new Vector4[MAX_FAST_LIGHTS_PER_FRAME];

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private class FastLightData
    {
        public float visibilityScore;
        public Vector4 position;
        public Vector4 color;
    }

    private static readonly Stack<FastLightData> _fastLightsPool = new Stack<FastLightData>();
    private static readonly List<FastLightData> _fastLightsUsed = new List<FastLightData>();

    private static int VisibilityScoresComparison(FastLightData x, FastLightData y) => x.visibilityScore < y.visibilityScore ? -1 : x.visibilityScore > y.visibilityScore ? 1 : 0;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal static Vector3 currentCameraPosition;
    private static Vector3 _currentCameraForward;

    private const float _currentCameraFOVAngle = 50; // deg
    private static readonly float _currentCameraFOVCos = Mathf.Cos(_currentCameraFOVAngle * Mathf.Deg2Rad);
    private static readonly float _currentCameraFOVSin = Mathf.Sin(_currentCameraFOVAngle * Mathf.Deg2Rad);

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static void Add(Vector3 position, float range, Color color, bool subtractive)
    {
        if (color.maxColorComponent < 0.001f) return; // ignore too dark lights

        var v = position - currentCameraPosition;
        var dot = Vector3.Dot(v, _currentCameraForward);
        var x = _currentCameraFOVCos * Mathf.Sqrt(Vector3.Dot(v, v) - dot * dot) - dot * _currentCameraFOVSin;
        if (x >= 0 && Mathf.Abs(x) >= range) return; // cull it as light is out of camera cone

        var fl = _fastLightsPool.Count > 0 ? _fastLightsPool.Pop() : new FastLightData();
        fl.visibilityScore = v.sqrMagnitude - (2 - dot) * range; // smaller are better
        fl.position.x = position.x;
        fl.position.y = position.y;
        fl.position.z = position.z;
        fl.position.w = range;
        fl.color.x = subtractive ? -color.r : color.r;
        fl.color.y = subtractive ? -color.g : color.g;
        fl.color.z = subtractive ? -color.b : color.b;
        _fastLightsUsed.Add(fl);
    }

    public static void ClearUsedList()
    {
        for (var i = _fastLightsUsed.Count - 1; i >= _fastLightsCount; i--) _fastLightsPool.Push(_fastLightsUsed[i]);
        _fastLightsUsed.Clear();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static void SetupForCamera(Camera camera, Material material)
    {
        // 1. Cache camera data
        var currentCameraTransform = camera.transform;
        currentCameraPosition = currentCameraTransform.position;
        _currentCameraForward = currentCameraTransform.forward;

        // 2. Generate lights data
        if (UL_Manager.instance == null || !UL_Manager.instance.HasPointLights())
        {
            for (var i = UL_FastLight.all.Count - 1; i >= 0; i--) UL_FastLight.all[i].GenerateRenderData();
            for (var i = UL_FastGI.all.Count - 1; i >= 0; i--) UL_FastGI.all[i].GenerateRenderData();
            for (var i = UL_RayTracedGI.all.Count - 1; i >= 0; i--) UL_RayTracedGI.all[i].GenerateRenderData();
        }

        // 3. Transfer lights data to arrays
        _fastLightsCount = Mathf.Min(_fastLightsUsed.Count, MAX_FAST_LIGHTS_PER_FRAME);
        _fastLightsUsed.Sort(VisibilityScoresComparison);
        for (var i = _fastLightsCount - 1; i >= 0; i--)
        {
            var ld = _fastLightsUsed[i];
            _fastLightsPositions[i] = ld.position;
            _fastLightsColors[i] = ld.color;
            _fastLightsPool.Push(ld);
        }
        ClearUsedList();
        material.SetInt("_LightsCount", _fastLightsCount);
        material.SetVectorArray("_LightsPositions", _fastLightsPositions);
        material.SetVectorArray("_LightsColors", _fastLightsColors);

        // 4. Prepare screen to world matrices
        var viewFromScreen = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true).inverse;
        viewFromScreen[1, 1] *= -1;
        material.SetMatrix("_ViewFromScreen", viewFromScreen);
        material.SetMatrix("_WorldFromView", camera.cameraToWorldMatrix);
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static void CreatePointLights(Transform parent, float intensityMultiplier)
    {
        // 1. Generate lights data
        for (var i = UL_FastLight.all.Count - 1; i >= 0; i--) UL_FastLight.all[i].GenerateRenderData();
        for (var i = UL_FastGI.all.Count - 1; i >= 0; i--) UL_FastGI.all[i].GenerateRenderData();
        for (var i = UL_RayTracedGI.all.Count - 1; i >= 0; i--) UL_RayTracedGI.all[i].GenerateRenderData();

        // 2. Generate point lights
        foreach (var data in _fastLightsUsed)
        {
            var color = new Color(data.color.x, data.color.y, data.color.z, 0);
            var intensity = color.maxColorComponent;
            if (intensity < 0.01f) continue;
            color *= 1 / intensity;

            var go = new GameObject("Point Light");
            go.transform.SetParent(parent, false);
            go.transform.position = data.position;

            var hdLight = go.AddHDLight(HDLightTypeAndShape.Point);
			hdLight.intensity = Mathf.Sqrt(intensity) * intensityMultiplier;
            hdLight.color = color.gamma;
            hdLight.range = data.position.w;
            hdLight.affectsVolumetric = false;

            var light = go.GetComponent<UnityEngine.Light>();
            light.bounceIntensity = 0;
            light.renderMode = LightRenderMode.ForceVertex;
        }

        // 3. Clear
        ClearUsedList();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static float CandelasToAbsoluteValue(float candelas) => Mathf.Sqrt(candelas) * 0.025f;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}