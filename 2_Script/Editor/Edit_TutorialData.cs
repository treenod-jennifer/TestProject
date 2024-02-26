using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialBase), true)]
public class Edit_TutorialData : Editor
{
    TutorialBase _target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ReAttach All ActionTriggers"))
        {
            Reattach();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            //UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_target.gameObject.scene);
            serializedObject.ApplyModifiedProperties();
        }
    }

    void Reattach()
    {
        _target = target as TutorialBase;
        _target.InitTutorialData();
    }
}
