using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UPGEN Lighting/UPGEN Fast GI")]
[RequireComponent(typeof(Light)), ExecuteInEditMode]
public sealed class UL_FastGI : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(1, 10)] public float expand = 3;
    [Range(0, 1)] public float intensity = 0.1f;

    [Header("Culling")]
    [Range(0, 400)] public float softCullingDistance = 40;
    [Range(0, 500)] public float hardCullingDistance = 50;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_FastGI> all = new List<UL_FastGI>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Light BaseLight => _light == null ? _light = GetComponent<Light>() : _light;
    private Light _light;

    private float _lastIntensity;
    private float _lastRealIntensity;

    internal void GenerateRenderData()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null) return;
        }

        Vector3 pos;
        var range = expand;
        var tr = transform;
        switch (_light.type)
        {
            case LightType.Spot: pos = tr.position + tr.forward; range *= 3; break;
            case LightType.Point: pos = tr.position; range *= _light.range; break;
            default: return;
        }

        var lightColor = _light.useColorTemperature && Math.Abs(_light.colorTemperature - 6570) > 5 ? _light.color.linear * Mathf.CorrelatedColorTemperatureToRGB(_light.colorTemperature) : _light.color.linear;
        var lightIntensity = _light.intensity;

        if (softCullingDistance > hardCullingDistance - 0.1f) // no culling, bad culling distances
        {
            UL_Renderer.Add(pos, range, intensity * UL_Renderer.CandelasToAbsoluteValue(lightIntensity) * lightColor.linear, false);
            return;
        }

        if (_lastIntensity != lightIntensity) _lastRealIntensity = lightIntensity; // remember real light intensity if it was modified externally

        var sqrDistance = (UL_Renderer.currentCameraPosition - pos).sqrMagnitude;
        var sqrSoftDist = softCullingDistance * softCullingDistance;
        if (sqrDistance < sqrSoftDist) // no culling, light is close enough
        {
            UL_Renderer.Add(pos, range, intensity * UL_Renderer.CandelasToAbsoluteValue(lightIntensity) * lightColor.linear, false);
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                _light.intensity = _lastIntensity = _lastRealIntensity;
                _light.enabled = true;
            }
            return;
        }

        var sqrHardDist = hardCullingDistance * hardCullingDistance;
        if (sqrDistance > sqrHardDist) // light is fully culled
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                _light.intensity = _lastIntensity = 0;
                _light.enabled = false;
            }
            return;
        }

        var alpha = 1 - (sqrDistance - sqrSoftDist) / (sqrHardDist - sqrSoftDist);
        var cubicAlpha = alpha * alpha * alpha;
        UL_Renderer.Add(pos, range, cubicAlpha * intensity * UL_Renderer.CandelasToAbsoluteValue(lightIntensity) * lightColor.linear, false); // smooth fade of light
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            _light.intensity = _lastIntensity = _lastRealIntensity * cubicAlpha;
            _light.enabled = true;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}