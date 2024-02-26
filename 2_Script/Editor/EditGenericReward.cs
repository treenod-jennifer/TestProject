using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GenericReward), true)]
public class EditGenericReward : Editor
{
    GenericReward _target = null;

    int t = 1;
    int v = 1;
    float scl = 1.0f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnInspectorGUI()
    {
        _target = target as GenericReward;

        DrawDefaultInspector();
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();


        GUI.backgroundColor = Color.yellow;
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("Type:", GUILayout.Width(40));
        t = EditorGUILayout.IntField(t, GUILayout.Width(80));

        GUILayout.Label("Value:", GUILayout.Width(40));
        v = EditorGUILayout.IntField(v, GUILayout.Width(80));

        GUILayout.Label("Scale:", GUILayout.Width(40));
        scl = EditorGUILayout.FloatField(scl, GUILayout.Width(80));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Set"))
        {
            _target.scale = scl;
            _target.SetReward(new Reward() { type = t, value = v} );
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
}
