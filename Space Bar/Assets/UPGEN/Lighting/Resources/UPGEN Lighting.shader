Shader "Hidden/Shader/UPGEN_Lighting"
{
	HLSLINCLUDE

	#define MAX_LIGHTS_COUNT 96

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"

	struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings

    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {

        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

	TEXTURE2D_X(_InputTexture);

	uniform float _Intensity;
	uniform float4x4 _WorldFromView;
	uniform float4x4 _ViewFromScreen;

	uniform int _LightsCount = 0;
	uniform float4 _LightsPositions[MAX_LIGHTS_COUNT]; // XYZ - position, W - range
	uniform float4 _LightsColors[MAX_LIGHTS_COUNT]; // RGB - color, A - not used

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
		float depth = LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS).r;
		if (depth == 0) return float4(outColor, 1); // do not apply lighting to skybox

		float4 gbuff0 = LOAD_TEXTURE2D_X(_GBufferTexture0, positionSS); // albedo + model AO (Mask map G)
		float4 gbuff1 = LOAD_TEXTURE2D_X(_GBufferTexture1, positionSS); // normals + roughness (Mask map A)
		float3 normal = UnpackNormalOctQuadEncode(Unpack888ToFloat2(gbuff1.rgb) * 2 - 1);
		//float4 gbuff2 = LOAD_TEXTURE2D_X(_GBufferTexture2, positionSS); // specular/metallic (Mask map R) + coat mask
		//float4 gbuff3 = LOAD_TEXTURE2D_X(_GBufferTexture3, positionSS); // bake lighting and/or emissive + something
		//float3 emission = gbuff3.rgb - gbuff0.rgb;
		//float4 gbuff4 = LOAD_TEXTURE2D_X(_GBufferTexture4, positionSS); // light layer or shadow mask (aka _LightLayersTexture)
		//float4 gbuff5 = LOAD_TEXTURE2D_X(_GBufferTexture5, positionSS); // shadow mask
		float ssao = 1 - LOAD_TEXTURE2D_X(_AmbientOcclusionTexture, positionSS).r;
		float ao = ssao * gbuff0.a;

		// bit of matrix math to take the screen space coord (u,v,depth) and transform to world space
		float4 viewPos = mul(_ViewFromScreen, float4(input.texcoord * 2 - 1, depth, 1)); // inverse projection by clip position
		viewPos /= viewPos.w; // perspective division
		float3 wpos = mul(_WorldFromView, viewPos).xyz;
		//return float4(cos(wpos * 100), 1); // debug

		int c = _LightsCount;
		float3 light; float3 dir; float dist; float4 lp;

		while (c >= 11) // loop unrolling
		{
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
		}

		while (c >= 3) // loop unrolling
		{
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
		}

		while (c > 0) // real iteration
		{
			c--;
			lp = _LightsPositions[c];
			dir = lp.xyz - wpos;
			dist = length(dir);
			if (dist < lp.w)
			{
				float i = 1 - dist / lp.w;
				i *= i;
				i *= max(0, dot(normal.xyz, dir / dist));
				light += _LightsColors[c].rgb * i;
			}
		}

		return float4(outColor.rgb + gbuff0.rgb * light * ao * _Intensity, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
				#pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }

    Fallback Off
}
