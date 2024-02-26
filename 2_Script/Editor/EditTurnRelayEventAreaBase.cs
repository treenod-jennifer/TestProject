using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(TurnRelayEventAreaBase), true)]
public class EditTurnRelayEventAreaBase: Editor
{
    protected TurnRelayEventAreaBase _target;

    protected void OnEnable()
    {
        wakeUpScene = PlayerPrefs.GetInt("EditTurnRelayEventAreaBase", 0);
    }
    void ReAttach()
    {
        _target = target as TurnRelayEventAreaBase;
        _target._listSceneDatas.Clear();

        bool haveGroupPosition = false;
        bool haveGroupEffect = false;
        bool haveGroupObject = false;


        for (int i = 0; i < _target.transform.childCount; i++)
        {
            TriggerScene obj = _target.transform.GetChild(i).GetComponent<TriggerScene>();

            

            if (obj != null)
            {
                SceneStateInfo info = new SceneStateInfo();
                info.sceneData = obj;
                _target._listSceneDatas.Add(info);
            }

            if (_target.transform.GetChild(i).name == "GroupPosition")
            {
                haveGroupPosition = true;
                _target.transform.GetChild(i).gameObject.SetActive(false);
            }
            if (_target.transform.GetChild(i).name == "GroupObject")
                haveGroupObject = true;
            if (_target.transform.GetChild(i).name == "GroupEffect")
            {
                haveGroupEffect = true;

                for (int h = 0; h < _target.transform.GetChild(i).childCount; h++)
                {
                    _target.transform.GetChild(i).GetChild(h).gameObject.SetActive(false);
                }
            }
        }


        if (!haveGroupPosition)
        {
            GameObject obj = new GameObject("GroupPosition");
            obj.transform.parent = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
        }

        if (!haveGroupEffect)
        {
            GameObject obj = new GameObject("GroupEffect");
            obj.transform.parent = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
        }

        if (!haveGroupObject)
        {
            GameObject obj = new GameObject("GroupObject");
            obj.transform.parent = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.identity;
        }
    }

    static int wakeUpScene = 0;
    public override void OnInspectorGUI()
    {
        _target = target as TurnRelayEventAreaBase;

        DrawDefaultInspector();
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Tool", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ReAttach All Scene && Make Group"))
        {
            ReAttach();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        GUI.backgroundColor = Color.yellow;
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("Scene Index:",  GUILayout.Width(80));

        int beforeWakeUpScene = wakeUpScene;
        wakeUpScene = EditorGUILayout.IntField(wakeUpScene, GUILayout.Width(100));
        if (beforeWakeUpScene != wakeUpScene)
            PlayerPrefs.SetInt("EditTurnRelayEventAreaBase", wakeUpScene);

        if (GUILayout.Button("Play        Wait -> WakeUp"))
        {
            ManagerLobby._instance.PlayScene_FromEditor(_target, wakeUpScene);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan Using Characters "))
        {
            _target.ScanCharacterUsageInChildrens();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_target.gameObject.scene);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
