using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CanEditMultipleObjects]
[CustomEditor(typeof(TriggerState), true)]
public class EditTriggerState : Editor
{

    protected TriggerState _target;

    void ReAttach()
    {
        _target = target as TriggerState;

        _target._conditions.Clear();
        _target._conditionsChating.Clear();

        for (int i = 0; i < _target.transform.childCount; i++)
        {
            if (_target.transform.GetChild(i).gameObject.name == "Condition" + (i+1))
            {
                _target._conditions.Add(_target.transform.GetChild(i).GetComponent<TriggerCondition>());
            }
        }
        for (int i = 0; i < _target.transform.childCount; i++)
        {
            if (_target.transform.GetChild(i).gameObject.name == "ConditionChating1")
            {
                int count = 1;
                while (true)
                {
                    if (_target.transform.childCount > (i + count - 1))
                    {
                        if (_target.transform.GetChild(i + count - 1).gameObject.name == "ConditionChating" + count)
                        {
                            _target._conditionsChating.Add(_target.transform.GetChild(i + count - 1).GetComponent<TriggerCondition>());
                        }
                        else
                            break;
                    }
                    else
                        break;
                    count++;
                }
                break;
            }
        }
    }



    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ReAttach All Condition"))
        {
            ReAttach();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //     GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        //   GUILayout.Label("Tool", EditorStyles.boldLabel);
    }

}
