using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CanEditMultipleObjects]
[CustomEditor(typeof(LobbyBehavior), true)]
public class EditLobbyBehavior : Editor
{

    protected LobbyBehavior _target;

    void ReAttach()
    {
        _target = target as LobbyBehavior;


    }


    int areaIndex = 1;
    int sceneIndex = 1;
    


    public override void OnInspectorGUI()
    {
        _target = target as LobbyBehavior;
        DrawDefaultInspector();


        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space();
    //    GUILayout.Label("Area Behavior:", GUILayout.Width(80));

     /*   int beforeWakeUpScene = wakeUpScene;
        wakeUpScene = EditorGUILayout.IntField(wakeUpScene, GUILayout.Width(100));
        if (beforeWakeUpScene != wakeUpScene)
            PlayerPrefs.SetInt("EditArea", wakeUpScene);*/

        if (GUILayout.Button(" TempBehavior  Play"))
        {
            if(_target.TempBehavior != null)
                _target.PlayBehavior(_target.TempBehavior);
            
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //     GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        //   GUILayout.Label("Tool", EditorStyles.boldLabel);
    }

}
