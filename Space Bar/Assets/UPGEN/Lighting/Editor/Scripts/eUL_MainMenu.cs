using UnityEditor;
using UnityEngine;

public static class eUL_MainMenu
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static GameObject CreateGameObject(string name)
    {
        var go = new GameObject(name);
        var selectedGO = Selection.activeObject as GameObject;
        if (selectedGO != null) go.transform.SetParent(selectedGO.transform, false);
        Selection.activeObject = go;
        return go;
    }

    [MenuItem("GameObject/UPGEN Lighting/Fast Light", false, 10)]
    public static void CreateFastLight() => CreateGameObject("Fast Light").AddComponent<UL_FastLight>();

    [MenuItem("GameObject/UPGEN Lighting/Manager", false, 10)]
    public static void CreateManager() => CreateGameObject("UPGEN Lighting Manager").AddComponent<UL_Manager>();

    [MenuItem("GameObject/UPGEN Lighting/Clear Cache", false, 10)]
    public static void ClearCache() => UL_Rays.Ray.ClearCache();

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
