using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class UL_GUI_Examples : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Multiline] public string description;
    public string prevScene;
    public string nextScene;

    [Header("References")]
    public Transform target;
    public AnimationKind animationKind;
    public VolumeProfile volumeProfile;

    public enum AnimationKind { AnimateX, AnimateZ }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private Vector3 _initialTargetPosition;
    private Vector3 _deltaTargetPosition;

    void Start()
    {
        foreach (var rgo in SceneManager.GetActiveScene().GetRootGameObjects()) rgo.hideFlags &= ~HideFlags.HideInHierarchy;

        if (volumeProfile && volumeProfile.TryGet<UPGEN_Lighting>(out var postEffect)) postEffect.intensity.value = 1;
        if (target) _initialTargetPosition = target.transform.position;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

#if !UNITY_EDITOR
    private float _nextUpdate;
    private int _fpsCounter;
    private int _fps;

    void Update()
    {
        if (!Application.isPlaying) return;
        _fpsCounter++;
        var time = Time.unscaledTime;
        if (time < _nextUpdate) return;
        _nextUpdate = time + 1;
        _fps = _fpsCounter;
        _fpsCounter = 0;
    }
#endif

    void LateUpdate()
    {
        if (target == null) return;

        if (Application.isPlaying)
            switch (animationKind)
            {
                case AnimationKind.AnimateX: _deltaTargetPosition.x = 3 * Mathf.Sin(Time.unscaledTime); break;
                case AnimationKind.AnimateZ: _deltaTargetPosition.z = 4 * Mathf.Sin(Time.unscaledTime * 0.5f); break;
            }

        if (_initialTargetPosition == Vector3.zero) _initialTargetPosition = target.transform.position;
        else target.transform.position = _initialTargetPosition + _deltaTargetPosition;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    void OnGUI()
    {
#if !UNITY_EDITOR
        GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, Screen.height));
        {
            if (_fps > 0) GUILayout.Label($"FPS: <b>{_fps}</b>", GUI.skin.box);
        }
        GUILayout.EndArea();
#endif
        OnGUI_Tools();
        OnGUI_Scene();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void OnGUI_Tools()
    {
        GUILayout.BeginArea(new Rect(0, 0, 200, Screen.height));
        {
            GUILayout.FlexibleSpace();

            if (volumeProfile && volumeProfile.TryGet<UPGEN_Lighting>(out var postEffect))
            {
                var v = postEffect.intensity.value;
                var nv = UL_GUI_Utils.Slider("Intensity", v, 0, 2);
                if (nv != v) postEffect.intensity.value = nv;
            }

            if (target && Application.isPlaying)
            {
                if (animationKind != AnimationKind.AnimateX) _deltaTargetPosition.x = UL_GUI_Utils.Slider("X", _deltaTargetPosition.x, -2, 2);
                _deltaTargetPosition.y = UL_GUI_Utils.Slider("Y", _deltaTargetPosition.y, -2, 2);
                if (animationKind != AnimationKind.AnimateZ) _deltaTargetPosition.z = UL_GUI_Utils.Slider("Z", _deltaTargetPosition.z, -3, 3);
                var rt = target.GetComponent<UL_RayTracedGI>();
                if (rt) rt.raysMatrixSize = (int)UL_GUI_Utils.Slider("Rays", rt.raysMatrixSize, 2, 15);
            }
        }
        GUILayout.EndArea();
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void OnGUI_Scene()
    {
        if (string.IsNullOrEmpty(description)) return;
        var activeScene = SceneManager.GetActiveScene();

        GUILayout.BeginArea(new Rect((Screen.width - 500) * 0.5f, Screen.height - 164, 500, 36));
        {
            GUILayout.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(prevScene) && SceneManager.sceneCountInBuildSettings > 1)
                    if (Application.isPlaying && GUILayout.Button("<size=24><b>◄</b></size>", GUILayout.Width(32), GUILayout.Height(34)))
                        SceneManager.LoadScene(prevScene);

                GUILayout.Label($"<size=24><b>{activeScene.name}</b></size>", GUI.skin.box);

                if (!string.IsNullOrEmpty(nextScene) && SceneManager.sceneCountInBuildSettings > 1)
                    if (Application.isPlaying && GUILayout.Button("<size=24><b>►</b></size>", GUILayout.Width(32), GUILayout.Height(34)))
                        SceneManager.LoadScene(nextScene);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();

        GUI.Box(new Rect((Screen.width - 1200) * 0.5f, Screen.height - 100, 1200, 60), "<size=20>" + description + "</size>");
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}