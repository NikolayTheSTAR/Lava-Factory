using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

public class ControlWindowEditor : EditorWindow
{
    private int levelIndex = 1;
    private ControlWindowState state = ControlWindowState.Main;

    private const float LightValue = 1.2f;
    private float contrastValue = 1f;

    private Color
        initialColor,
        grayColor,
        lightColor,
        contrastColor,
        faintColor;

    private readonly Color maxFaintColor = new Color(0.5f, 0.5f, 0.5f, 1);

    private string GetLevelPath => $"Assets/Scenes/Level_{levelIndex}.unity";

    [MenuItem("Window/Control %g")]
    public static void ShowWindow()
    {
        GetWindow<ControlWindowEditor>("Control");
    }

    private void OnGUI()
    {
        switch (state)
        {
            case ControlWindowState.Main:
                levelIndex = EditorGUILayout.IntField("LevelIndex", levelIndex);

                if (GUILayout.Button("Open Level"))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(GetLevelPath);
                }

                GUILayout.Space(20);

                if (GUILayout.Button("Prepare"))
                {
                    var preparables = FindObjectsOfType<MonoBehaviour>().OfType<IPreparable>();

                    foreach (var p in preparables) p.Prepare();

                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                    Debug.Log("Prepared");
                }

                if (GUILayout.Button("Test")) state = ControlWindowState.Test;

                break;

            case ControlWindowState.Test:
                if (GUILayout.Button("Back")) state = ControlWindowState.Main;
                if (GUILayout.Button("Color Test")) state = ControlWindowState.ColorTest;
                break;

            case ControlWindowState.ColorTest:
                if (GUILayout.Button("Back")) state = ControlWindowState.Test;

                initialColor = EditorGUILayout.ColorField("Initial", initialColor);

                // faint

                faintColor = initialColor - (initialColor - maxFaintColor) / 2;
                EditorGUILayout.ColorField("Faint", faintColor);

                // contrast

                contrastColor = initialColor + (initialColor - maxFaintColor) / 2;
                EditorGUILayout.ColorField("Contrast", contrastColor);

                // gray

                float value = (initialColor.r + initialColor.g + initialColor.b)/3;
                grayColor = new Color(value, value, value, initialColor.a);
                EditorGUILayout.ColorField("Gray", grayColor);

                // light

                lightColor = new Color(initialColor.r * LightValue, initialColor.g * LightValue, initialColor.b * LightValue, initialColor.a);
                EditorGUILayout.ColorField("Light", lightColor);

                break;
        }
    }

    public enum ControlWindowState
    {
        Main,
        Test,
        ColorTest
    }
}