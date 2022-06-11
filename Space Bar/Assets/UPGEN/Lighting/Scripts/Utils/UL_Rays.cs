using System.Collections.Generic;
using UnityEngine;

public static class UL_Rays
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static readonly Ray[] EMPTY_RAYS = new Ray[0];

    public static Ray[] GenerateRays(int count)
    {
        var rays = new Ray[count];
        for (var i = count - 1; i >= 0; i--) rays[i] = new Ray();
        return rays;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Ray
    {
        public bool hit;
        public Vector3 interpolatedPosition;
        public Vector3 position;
        public Color interpolatedColor;
        public Color color;

        private const float HIT_OFFSET = 1f;
        private const float HIT_OFFSET_MAX = 2f * HIT_OFFSET;

        public void Point(Vector3 pt, Vector3 dir)
        {
            hit = true;
            position = pt + dir;
            color = Color.white;
        }

        public void Trace(Vector3 pt, Vector3 dir, float range, Color lightColor, int layersToHit)
        {
            var hadHit = hit;
            hit = false;
            if (!Physics.Raycast(pt, dir, out var rayHit, range * 0.9f, layersToHit)) return;

            var hitPt = rayHit.point;
            var hitNorm = rayHit.normal;
            var hitPtWithOffset = hitPt + hitNorm * 0.001f;
            var hitDistance = rayHit.distance;

            if (hitDistance < 0.1f) return; // hit is too close

            // reflected ray, perpendicular to surface
            position = Physics.Raycast(hitPtWithOffset, hitNorm, out var offsetHit, HIT_OFFSET_MAX, layersToHit) ? hitPt + 0.5f * offsetHit.distance * hitNorm : hitPt + HIT_OFFSET * hitNorm;
            if (Physics.CheckSphere(position, 0.2f, layersToHit)) // do not allow rays close to surface
            {
                // reflected ray, in opposite direction to light
                position = Physics.Raycast(hitPtWithOffset, -dir, out offsetHit, HIT_OFFSET_MAX, layersToHit) ? hitPt - 0.5f * offsetHit.distance * dir : hitPt - HIT_OFFSET * dir;
                if (Physics.CheckSphere(position, 0.1f, layersToHit)) return; // do not allow rays close to surface
            }

            var distanceFalloff = hitDistance / range;
            distanceFalloff *= distanceFalloff;
            distanceFalloff = 1 - distanceFalloff;
            color = GetRayHitColor(rayHit.transform.GetComponent<Renderer>());
            color.r *= color.r * color.r * lightColor.r * distanceFalloff;
            color.g *= color.g * color.g * lightColor.g * distanceFalloff;
            color.b *= color.b * color.b * lightColor.b * distanceFalloff;
            hit = true;

            if (!hadHit) // force interpolated values if found new hit
            {
                interpolatedColor = color;
                interpolatedPosition = position;
            }

#if UNITY_EDITOR
            if (UL_Manager.instance && UL_Manager.instance.showDebugRays)
                Debug.DrawLine(hitPt, position, color * 2, 0.21f);
#endif
        }

        private static readonly int _propertyColorId = Shader.PropertyToID("_Color");
        private static readonly int _propertyMainTexId = Shader.PropertyToID("_MainTex");
        private static readonly Dictionary<Renderer, Color> _cachedRendererColor = new Dictionary<Renderer, Color>();
        private static RenderTexture _renderTexture;
        private static Texture2D _tempTexture;

        private static Color GetRayHitColor(Renderer r)
        {
            if (r == null) return Color.white;
            if (_cachedRendererColor.TryGetValue(r, out var color)) return color;

            // Renderer has no material
            var m = r.sharedMaterial;
            if (m == null)
            {
                _cachedRendererColor.Add(r, Color.white);
                return Color.white;
            }

            // Get main color property from material
            color = m.HasProperty(_propertyColorId) ? m.GetColor(_propertyColorId) : Color.white;
            if (!m.HasProperty(_propertyMainTexId))
            {
                _cachedRendererColor.Add(r, color);
                return color;
            }

            // Get main texture from material
            var tex = m.GetTexture(_propertyMainTexId);
            if (tex == null)
            {
                _cachedRendererColor.Add(r, color);
                return color;
            }

            // Create helper textures
            if (_renderTexture == null) _renderTexture = new RenderTexture(1, 1, 0);
            if (_tempTexture == null) _tempTexture = new Texture2D(1, 1);

            // Extract color information from texture
            Graphics.Blit(tex, _renderTexture);
            var previous = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            _tempTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            _tempTexture.Apply();
            RenderTexture.active = previous;

            // Multiply texture color on main material color
            color *= _tempTexture.GetPixel(0, 0);
            _cachedRendererColor.Add(r, color);
            return color;
        }

        public static void ClearCache() => _cachedRendererColor.Clear();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}