using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable, VolumeComponentMenu("Post-processing/UPGEN Lighting")]
public sealed class UPGEN_Lighting : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private const string SHADER = "Hidden/Shader/UPGEN_Lighting";
    private Material _material;

    public override void Setup() => _material = new Material(Shader.Find(SHADER));
    public override void Cleanup() => CoreUtils.Destroy(_material);

    public bool IsActive() => _material && intensity.value > 0 && UL_Renderer.HasLightsToRender;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Tooltip("Controls the intensity of all additional lighting")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 5);

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterOpaqueAndSky;

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (_material == null) return;
        _material.SetTexture("_InputTexture", source);
        _material.SetFloat("_Intensity", intensity.value);
        UL_Renderer.SetupForCamera(camera.camera, _material);
        HDUtils.DrawFullScreen(cmd, _material, destination);
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}