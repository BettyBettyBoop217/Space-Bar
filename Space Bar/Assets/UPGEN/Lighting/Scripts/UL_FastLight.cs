using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[AddComponentMenu("UPGEN Lighting/UPGEN Fast Light")]
[ExecuteInEditMode]
public sealed class UL_FastLight : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Range(1000, 20000)] public float colorTemperature = 6570;
    [ColorUsage(false)] public Color color = Color.white;
    [Range(0, 100000)] public float intensity = 1000;
    public LightUnit unit = LightUnit.Lumen;
    [Range(0, 15)] public float range = 10;
    public bool subtractive;

    [Header("Culling")]
    [Range(0, 400)] public float softCullingDistance = 40;
    [Range(0, 500)] public float hardCullingDistance = 50;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly List<UL_FastLight> all = new List<UL_FastLight>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal void GenerateRenderData()
    {
        var pos = transform.position;
        var clr = Math.Abs(colorTemperature - 6570) > 5 ? color.linear * Mathf.CorrelatedColorTemperatureToRGB(colorTemperature) : color.linear;
        if (softCullingDistance > hardCullingDistance - 0.1f) { UL_Renderer.Add(pos, range, UL_Renderer.CandelasToAbsoluteValue(intensity) * clr, subtractive); return; } // no culling, bad culling distances

        var sqrDistance = (UL_Renderer.currentCameraPosition - pos).sqrMagnitude;
        var sqrSoftDist = softCullingDistance * softCullingDistance;
        if (sqrDistance < sqrSoftDist) { UL_Renderer.Add(pos, range, UL_Renderer.CandelasToAbsoluteValue(intensity) * clr, subtractive); return; } // no culling, light is close enough

        var sqrHardDist = hardCullingDistance * hardCullingDistance;
        if (sqrDistance > sqrHardDist) return; // light is fully culled

        var alpha = 1 - (sqrDistance - sqrSoftDist) / (sqrHardDist - sqrSoftDist);
        UL_Renderer.Add(pos, range, alpha * alpha * alpha * UL_Renderer.CandelasToAbsoluteValue(intensity) * clr, subtractive); // smooth fade of light
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public bool CalcIsFullyCulled(Transform cameraTransform)
    {
        if (softCullingDistance > hardCullingDistance - 0.1f) return false;
        if (cameraTransform == null) return true;
        var sqrDistance = (cameraTransform.position - transform.position).sqrMagnitude;
        if (sqrDistance < softCullingDistance * softCullingDistance) return false;
        return sqrDistance > hardCullingDistance * hardCullingDistance;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}