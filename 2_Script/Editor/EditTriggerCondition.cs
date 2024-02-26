using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




[CanEditMultipleObjects]
[CustomEditor(typeof(TriggerCondition), true)]
public class EditTriggerCondition : Editor
{
 
    TriggerCondition _target;

    void OnEnable()
    {
        
     //   Reattach();
        
    }
    void Reattach()
    {
        _target = target as TriggerCondition;
        _target._actions.Clear();

        ActionBase[] _actionTrigger = _target.gameObject.GetComponents<ActionBase>();
        int nChildCount = _actionTrigger.Length;
        for (int i = 0; i < nChildCount; i++)
            _target._actions.Add(_actionTrigger[i]);

        if (_target.transform.parent != null)
        {
            TriggerState state = _target.transform.parent.gameObject.GetComponent<TriggerState>();

            for (int i = 0; i < (state._conditions.Count - 1); i++)
            {
                if (_target == state._conditions[i])
                    _target._nextCondition = state._conditions[i + 1];
            }
            /*
            for (int i = 0; i < (state._conditionsChating.Count - 1); i++)
            {
                if (_target == state._conditionsChating[i])
                    _target._nextCondition = state._conditionsChating[i + 1];
            }*/
        }
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ReAttach All ActionTriggers"))
        {
            _target = target as TriggerCondition;
            Reattach();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }
/*    public virtual void OnSceneGUI()
    {
        Handles.BeginGUI();

        Rect button_rect = new Rect(10, 10, 100, 30);
        GUI.enabled = true;
        if (GUI.Button(button_rect, "View"))
        {

        }
        button_rect.y += 40f;
        if (GUI.Button(button_rect, "Add Trigger"))
        {

        }

        Handles.EndGUI();
    }

    private void OnItemGUI()
    {

    }*/
 /*   public override void OnInspectorGUI()
    {

    }*/
}
