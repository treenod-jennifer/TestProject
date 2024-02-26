using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CanEditMultipleObjects]
[CustomEditor(typeof(TriggerScene), true)]
public class EditTriggerScene : Editor
{
    protected TriggerScene _target;

    void ReAttach()
    {
        _target = target as TriggerScene;

        if (_target._triggerWait == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "TriggerSceneWait")
                {
                    if (_target.transform.GetChild(i).GetComponent<TriggerStateWait>() == null)
                        _target.transform.GetChild(i).gameObject.AddComponent<TriggerStateWait>();
                    else
                        _target._triggerWait = _target.transform.GetChild(i).GetComponent<TriggerStateWait>();
                    break;
                }
            }

            if (_target._triggerWait == null)
            {
                GameObject model = new GameObject("TriggerSceneWait");
                model.AddComponent<TriggerStateWait>();

                _target._triggerWait = model.GetComponent<TriggerStateWait>();
                _target.transform.parent = _target.transform;
            }
        }

        if (_target._triggerWakeUp == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "TriggerSceneWakeUp")
                {
                    if (_target.transform.GetChild(i).GetComponent<TriggerStateWakeUp>() == null)
                        _target.transform.GetChild(i).gameObject.AddComponent<TriggerStateWakeUp>();
                    else
                        _target._triggerWakeUp = _target.transform.GetChild(i).GetComponent<TriggerStateWakeUp>();
                    break;
                }
            }

            if (_target._triggerWakeUp == null)
            {
                GameObject model = new GameObject("TriggerSceneWakeUp");
                model.AddComponent<TriggerStateWakeUp>();

                _target._triggerWakeUp = model.GetComponent<TriggerStateWakeUp>();
                model.transform.parent = _target.transform;
            }
        }

        if (_target._triggerActive == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "TriggerSceneActive")
                {
                    if (_target.transform.GetChild(i).GetComponent<TriggerStateActive>() == null)
                        _target.transform.GetChild(i).gameObject.AddComponent<TriggerStateActive>();
                    else
                        _target._triggerActive = _target.transform.GetChild(i).GetComponent<TriggerStateActive>();
                    break;
                }
            }

            if (_target._triggerActive == null)
            {
                GameObject model = new GameObject("TriggerSceneActive");
                model.AddComponent<TriggerStateActive>();

                _target._triggerActive = model.GetComponent<TriggerStateActive>();
                model.transform.parent = _target.transform;
            }
        }

        if (_target._triggerFinish == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "TriggerSceneFinish")
                {
                    if (_target.transform.GetChild(i).GetComponent<TriggerStateFinish>() == null)
                        _target.transform.GetChild(i).gameObject.AddComponent<TriggerStateFinish>();
                    else
                        _target._triggerFinish = _target.transform.GetChild(i).GetComponent<TriggerStateFinish>();
                    break;
                }
            }

            if (_target._triggerFinish == null)
            {
                GameObject model = new GameObject("TriggerSceneFinish");
                model.AddComponent<TriggerStateFinish>();

                _target._triggerFinish = model.GetComponent<TriggerStateFinish>();
                model.transform.parent = _target.transform;
            }
        }

      /*  if (_target._transformModel == null)
        {
            for (int i = 0; i < _target.transform.GetChildCount(); i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "rootModel")
                {
                    _target._transformModel = _target.transform.GetChild(i);
                    break;
                }
            }
            if (_target._transformModel == null)
            {
                GameObject model = new GameObject("rootModel");
                _target._transformModel = model.transform;
                _target._transformModel.parent = _target.transform;
            }
        }
        _target._transformModel.rotation = Quaternion.Euler(50f, -45f, 0f);*/

    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ReAttach All TriggerState && Make TriggerState"))
        {
            ReAttach();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

   //     GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
     //   GUILayout.Label("Tool", EditorStyles.boldLabel);
    }
}
