using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using TheSTAR.Utility;
using ColorUtility = TheSTAR.Utility.ColorUtility;

public class ControlWindowEditor : EditorWindow
{
    private int levelIndex = 1;
    private ControlWindowState state = ControlWindowState.Main;

    private const float LightValue = 1.2f;
    private float contrastValue = 1f;

    private Color
        initialColor,
        lightColor,
        contrastColor,
        faintColor;

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

                EditorGUILayout.Space(10);
                initialColor = EditorGUILayout.ColorField("Initial", initialColor);
                EditorGUILayout.Space(10);

                EditorGUILayout.ColorField("Faint", ColorUtility.Faint(initialColor));
                EditorGUILayout.ColorField("Contrast", ColorUtility.Contrast(initialColor));
                EditorGUILayout.ColorField("Gray", ColorUtility.Gray(initialColor));
                EditorGUILayout.ColorField("Light", ColorUtility.Light(initialColor, LightValue));

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