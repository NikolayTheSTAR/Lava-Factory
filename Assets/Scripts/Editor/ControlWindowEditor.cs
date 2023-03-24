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

    private string GetLevelPath => $"Assets/Scenes/Level_{levelIndex}.unity";

    [MenuItem("Window/Control")]
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

                if (GUILayout.Button("Prepare"))
                {
                    var preparables = FindObjectsOfType<MonoBehaviour>().OfType<IPreparable>();

                    foreach (var p in preparables) p.Prepare();

                    Debug.Log("Prepared");
                }

                break;
        }
    }

    public enum ControlWindowState
    {
        Main
    }
}