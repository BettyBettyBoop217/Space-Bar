using UnityEngine;

[ExecuteInEditMode]
public class UL_ShowCulling : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public Transform cameraTransform;
    public Light targetLight;
    public UL_FastLight targetFastLight;
    public GameObject activeSprite;
    public GameObject culledSprite;

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (targetLight)
        {
            var lightEnabled = targetLight.enabled;
            if (activeSprite) activeSprite.SetActive(lightEnabled);
            if (culledSprite) culledSprite.SetActive(!lightEnabled);
        }
        else if (targetFastLight)
        {
            var culled = targetFastLight.CalcIsFullyCulled(cameraTransform);
            if (activeSprite) activeSprite.SetActive(!culled);
            if (culledSprite) culledSprite.SetActive(culled);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}