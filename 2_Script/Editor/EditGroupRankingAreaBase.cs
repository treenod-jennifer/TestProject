using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GroupRankingAreaBase), true)]
public class EditGroupRankingAreaBase : Editor
{
    private static int wakeUpScene = 0;

    private GroupRankingAreaBase _target;

    protected void OnEnable() => wakeUpScene = PlayerPrefs.GetInt("EditGroupRankingAreaBase", 0);

    private void ReAttach()
    {
        _target = target as GroupRankingAreaBase;
        _target._listSceneDatas.Clear();

        var haveGroupPosition = false;
        var haveGroupEffect   = false;
        var haveGroupObject   = false;

        for (var i = 0; i < _target.transform.childCount; i++)
        {
            var obj = _target.transform.GetChild(i).GetComponent<TriggerScene>();

            if (obj != null)
            {
                var info = new SceneStateInfo
                {
                    sceneData = obj
                };
                _target._listSceneDatas.Add(info);
            }

            if (_target.transform.GetChild(i).name == "GroupPosition")
            {
                haveGroupPosition = true;
                _target.transform.GetChild(i).gameObject.SetActive(false);
            }

            if (_target.transform.GetChild(i).name == "GroupObject")
            {
                haveGroupObject = true;
            }

            if (_target.transform.GetChild(i).name == "GroupEffect")
            {
                haveGroupEffect = true;

                for (var h = 0; h < _target.transform.GetChild(i).childCount; h++)
                {
                    _target.transform.GetChild(i).GetChild(h).gameObject.SetActive(false);
                }
            }
        }

        if (!haveGroupPosition)
        {
            var obj = new GameObject("GroupPosition");
            obj.transform.parent        = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale    = Vector3.one;
            obj.transform.rotation      = Quaternion.identity;
        }

        if (!haveGroupEffect)
        {
            var obj = new GameObject("GroupEffect");
            obj.transform.parent        = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale    = Vector3.one;
            obj.transform.rotation      = Quaternion.identity;
        }

        if (!haveGroupObject)
        {
            var obj = new GameObject("GroupObject");
            obj.transform.parent        = _target.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale    = Vector3.one;
            obj.transform.rotation      = Quaternion.identity;
        }
    }

    public override void OnInspectorGUI()
    {
        _target = target as GroupRankingAreaBase;

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
        GUILayout.Label("Scene Index:", GUILayout.Width(80));

        var beforeWakeUpScene = wakeUpScene;
        wakeUpScene = EditorGUILayout.IntField(wakeUpScene, GUILayout.Width(100));
        if (beforeWakeUpScene != wakeUpScene)
        {
            PlayerPrefs.SetInt("EditGroupRankingAreaBase", wakeUpScene);
        }

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
    }
}