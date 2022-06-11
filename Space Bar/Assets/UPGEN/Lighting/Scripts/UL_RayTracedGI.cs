using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN RayTraced GI")]
[RequireComponent(typeof(Light)), ExecuteInEditMode]
public sealed class UL_RayTracedGI : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(0, 5)] public float intensity = 1;
    [Range(2, 25)] public int raysMatrixSize = 7;

    [Header("Culling")]
    [Range(0, 400)] public float softCullingDistance = 40;
    [Range(0, 500)] public float hardCullingDistance = 50;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private const float BOUNCED_LIGHT_RANGE = 8;
    private const float BOUNCED_LIGHT_BOOST = 2;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_RayTracedGI> all = new List<UL_RayTracedGI>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Light BaseLight => _light == null ? _light = GetComponent<Light>() : _light;
    private Light _light;

    private float _lastUpdateTime;
    private float _lastTime;
    private float _lastIntensity;
    private float _lastRealIntensity;

    private static readonly Color _black = new Color(0, 0, 0, 1);

    internal void GenerateRenderData()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null) return;
        }

        var lightType = _light.type;
        if (lightType != LightType.Point && lightType != LightType.Spot) return;

        var lightEnabled = _light.enabled;
        var lightIntensity = _light.intensity;
        var pos = transform.position;
        var alpha = 1f;
        if (softCullingDistance + 0.1f < hardCullingDistance) // has culling
        {
            if (_lastIntensity != lightIntensity) _lastRealIntensity = lightIntensity; // remember real light intensity if it was modified externally
            var sqrDistance = (UL_Renderer.currentCameraPosition - pos).sqrMagnitude;
            var sqrSoftDist = softCullingDistance * softCullingDistance;
            if (sqrDistance < sqrSoftDist) // no culling, light is close enough
            {
                alpha = 1f;
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    _light.intensity = _lastIntensity = _lastRealIntensity;
                    _light.enabled = lightEnabled = true;
                }
            }
            else
            {
                var sqrHardDist = hardCullingDistance * hardCullingDistance;
                if (sqrDistance > sqrHardDist) // light is fully culled
                {
                    alpha = 0f;
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                    {
                        if (_light.enabled)
                            for (var j = _rays.Length - 1; j >= 0; j--)
                            {
                                var ray = _rays[j];
                                ray.interpolatedColor = _black;
                                ray.hit = false;
                            }
                        _light.intensity = _lastIntensity = 0;
                        _light.enabled = lightEnabled = false;
                    }
                }
                else // smooth fade of light
                {
                    alpha = 1 - (sqrDistance - sqrSoftDist) / (sqrHardDist - sqrSoftDist);
                    alpha = alpha * alpha * alpha;
#if UNITY_EDITOR
                    if (Application.isPlaying)
#endif
                    {
                        _light.intensity = _lastIntensity = _lastRealIntensity * alpha;
                        _light.enabled = lightEnabled = true;
                    }
                }
            }
        }

#if UNITY_EDITOR
        var time = (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        var time = Time.unscaledTime;
#endif
        var delta = time - _lastTime;
        _lastTime = time;
        if (!lightEnabled) return;
        if (time - _lastUpdateTime > 0.2f) // Accumulate 5 updates per second
        {
            _lastUpdateTime = time;
            UpdateRaysMatrix();
            if (_rays == UL_Rays.EMPTY_RAYS) return;

            var lightColor = alpha * intensity * UL_Renderer.CandelasToAbsoluteValue(lightIntensity) * BOUNCED_LIGHT_BOOST / (raysMatrixSize * raysMatrixSize)
                * (_light.useColorTemperature && Math.Abs(_light.colorTemperature - 6570) > 5 ? _light.color.linear * Mathf.CorrelatedColorTemperatureToRGB(_light.colorTemperature) : _light.color.linear);

            if (lightColor.maxColorComponent < 0.001f) // light is disabled
            {
                for (var j = _rays.Length - 1; j >= 0; j--) _rays[j].hit = false;
                return;
            }

            var range = _light.range;
            var layersToRayTrace = UL_Manager.instance ? (int)UL_Manager.instance.layersToRayTrace : -5;

            switch (lightType)
            {
                case LightType.Spot:
                    var spotRadius = Mathf.Tan(Mathf.Deg2Rad * _light.spotAngle * 0.4f) * _light.range;
                    var spotRotation = transform.rotation;
                    var spotForward = transform.forward;
                    for (var j = _rayMatrix2D.Length - 1; j >= 0; j--)
                        _rays[j].Trace(pos, (spotForward * range + spotRotation * (_rayMatrix2D[j] * spotRadius)).normalized, range, lightColor, layersToRayTrace);
                    break;

                case LightType.Point:
                    for (var j = _rayMatrix3D.Length - 1; j >= 0; j--)
                        _rays[j].Trace(pos, _rayMatrix3D[j], range, lightColor, layersToRayTrace);
                    break;
            }
        }

        // Add lights and interpolate their values
        for (var j = _rays.Length - 1; j >= 0; j--)
        {
            var ray = _rays[j];
            if ((ray.interpolatedPosition - ray.position).sqrMagnitude > 9) // ray position changed too much (this is a new ray)
            {
                if (ray.interpolatedColor.maxColorComponent > 0.01f) ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, _black, delta * 10f);
                else
                {
                    ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, ray.hit ? ray.color : _black, delta * 10f);
                    ray.interpolatedPosition = ray.position;
                }
                UL_Renderer.Add(ray.interpolatedPosition, BOUNCED_LIGHT_RANGE, ray.interpolatedColor, false);
            }
            else
            {
                ray.interpolatedColor = Color.Lerp(ray.interpolatedColor, ray.hit ? ray.color : _black, delta * 5f);
                ray.interpolatedPosition = Vector3.Lerp(ray.interpolatedPosition, ray.position, delta * 10f);
                UL_Renderer.Add(ray.interpolatedPosition, BOUNCED_LIGHT_RANGE, ray.interpolatedColor, false);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private UL_Rays.Ray[] _rays;
    private Vector2[] _rayMatrix2D;
    private Vector3[] _rayMatrix3D;

    private void UpdateRaysMatrix()
    {
        switch (_light.type)
        {
            case LightType.Spot:
                var newMatrixSpot = UL_RayMatrices.GRID[raysMatrixSize - 2];
                if (_rayMatrix2D == newMatrixSpot) return;
                _rayMatrix2D = newMatrixSpot;
                _rays = UL_Rays.GenerateRays(newMatrixSpot.Length);
                return;

            case LightType.Point:
                var newMatrixPoint = UL_RayMatrices.SPHERE[raysMatrixSize - 2];
                if (_rayMatrix3D == newMatrixPoint) return;
                _rayMatrix3D = newMatrixPoint;
                _rays = UL_Rays.GenerateRays(newMatrixPoint.Length);
                return;

            default:
                _rays = UL_Rays.EMPTY_RAYS;
                return;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void ExposeFastLight(UL_Rays.Ray ray)
    {
        if (!ray.hit) return;

        var go = new GameObject("Fast Light");

        go.transform.SetParent(transform, false);
        go.transform.position = ray.position;

        var cmp = go.AddComponent<UL_FastLight>();
        cmp.intensity = 1;
        cmp.range = BOUNCED_LIGHT_RANGE;
        cmp.color = ray.color.gamma;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Expose Fast Lights");
#endif
    }

    public void ExposeFastLights()
    {
        if (!enabled) return;
        GenerateRenderData();
        UL_Renderer.ClearUsedList();

        if (_rays == null) return;
        for (var j = _rays.Length - 1; j >= 0; j--) ExposeFastLight(_rays[j]);

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Expose Fast Lights");
#endif
        enabled = false;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void CollapseFastLights()
    {
        if (enabled) return;
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var tr = transform.GetChild(i);
            if (tr.GetComponent<UL_FastLight>())
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(tr.gameObject);
#else
                DestroyImmediate(tr.gameObject);
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Collapse Fast Lights");
#endif
        enabled = true;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}