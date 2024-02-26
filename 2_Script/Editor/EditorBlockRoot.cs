using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlockRoot))]
public class EditorBlockRoot : Editor
{
    private BlockRoot _target = null;
    private void OnEnable()
    {
        Tools.current = Tool.View;
        Tools.viewTool = ViewTool.FPS;
        _target = target as BlockRoot;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Make", GUILayout.Width(200)))
        {
            if (_target != null)
            {
                _target.MakeMesh();
            }
        }
    }



}