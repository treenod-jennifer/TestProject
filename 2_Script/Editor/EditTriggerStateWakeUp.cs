using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TriggerStateWakeUp), true)]
public class EditTriggerStateWakeUp : EditTriggerState
{
    private TriggerStateWakeUp _target;
    
    public override void OnInspectorGUI()
    {
        _target = target as TriggerStateWakeUp;
        
        base.OnInspectorGUI();
        
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("SetCameraPotion", EditorStyles.boldLabel);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.red;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Set Camera Position"))
        {
            SetCameraPosition();
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.yellow;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Clear Camera Position"))
        {
            ClearCameraPosition();
        }
    }

    private void SetCameraPosition()
    {
        Debug.Log("SetCameraPosition");
        
        for (int idx = 0; idx < _target._conditions.Count; idx++)
        {
            for (int i = 0; i < _target._conditions[idx]._actions.Count; i++)
            {
                //마지막 ActionCameraMove에는 Position 세팅 X
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>().Length; j++)
                {
                    if (idx == _target._conditions.Count - 1) return;
                    
                    if(idx == 0)
                        _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>()[j]._startPosition = _target.posCamera;

                    _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>()[j]._endPosition = _target.posCamera;
                }
                
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionEffect>().Length; j++)
                {
                    foreach (var animation in _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionEffect>()[j]._animation)
                    {
                        animation._rootTransform = _target.posEffect;
                    }
                }
                
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCharacterSpawn>().Length; j++)
                {
                    //NPC Mission일 경우 첫번째는 무조건 포코타 위치
                    //Pokota Mission일 경우 ActionCharacterSpawn은 하나만 존재하기에 포코타 위치만 설정
                    if (j == 0)
                        _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCharacterSpawn>()[j]._position = _target.posPokotaSpawn;
                    else
                        _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCharacterSpawn>()[j]._position = _target.posNpcSpawn;
                }
            }
        }
    }

    private void ClearCameraPosition()
    {
        Debug.Log("ClearCameraPosition");
        
        for (int idx = 0; idx < _target._conditions.Count; idx++)
        {
            for (int i = 0; i < _target._conditions[idx]._actions.Count; i++)
            {
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>().Length; j++)
                {

                    _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>()[j]._startPosition = null;
                    
                    _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCameraMove>()[j]._endPosition = null;
                }
                
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionEffect>().Length; j++)
                {
                    foreach (var animation in _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionEffect>()[j]._animation)
                    {
                        animation._rootTransform = null;
                    }
                }
                
                for (int j = 0; j < _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCharacterSpawn>().Length; j++)
                {
                    _target._conditions[idx]._actions[i].GetComponentsInChildren<ActionCharacterSpawn>()[j]._position = null;
                }
            }
        }
    }
}
