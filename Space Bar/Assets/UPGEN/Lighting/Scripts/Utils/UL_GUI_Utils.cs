using UnityEngine;

public static class UL_GUI_Utils
{
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public static void Text(string text) => GUILayout.Label(text, GUI.skin.box);

    public static float Slider(string text, float value, float min, float max)
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        {
            GUILayout.Label($"<b>{text}</b>", GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            {
                GUILayout.Space(6);
                value = GUILayout.HorizontalSlider(value, min, max);
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        return value;
    }

    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}