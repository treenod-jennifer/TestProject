using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(Area), true)]
public class EditArea : Editor
{
    protected Area _target;
    protected void OnEnable()
    {
        wakeUpScene = PlayerPrefs.GetInt("EditArea",0);
    }
    void ReAttach()
    {
        _target = target as Area;
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
                
                ChatGameData chatGameData = obj.GetComponentInChildren<ChatGameData>();
                if (chatGameData != null)
                {
                    foreach (var cData in chatGameData._listChatData)
                    {
                        if (cData.chatBack_0.Equals("0") || cData.chatBack_1.Equals("0") || cData.chatBackCenter.Equals("0"))
                            Debug.LogError("[" + obj.gameObject.name + "] " +cData.tempTitle+ " : chatBack 확인 필요");
                    }
                }
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
        _target = target as Area;

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
            PlayerPrefs.SetInt("EditArea", wakeUpScene);

        if (GUILayout.Button("Play        Wait -> WakeUp"))
        {
            ManagerLobby._instance.PlayScene_FromEditor(_target, wakeUpScene);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Scan Using Characters "))
        {
            _target.ScanCharacterUsageInChildrens();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // 씬을 생성하는 커스텀 Inspector
        OnInspectorSceneCreate();
    }

    #region SceneCreate

    private int sceneIndex = 0;
    private int createCount = 0;
    private SceneType secenType;

    private GameObject objSceneData;
    
    void OnInspectorSceneCreate()
    {
        Area area = (Area)_target;
        
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("SecneCreate", EditorStyles.boldLabel);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        GUI.backgroundColor = Color.grey;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label("Scene Index:",  GUILayout.Width(80));

        sceneIndex = EditorGUILayout.IntField(sceneIndex, GUILayout.Width(40));
        
        GUILayout.Label("Create Count:",  GUILayout.Width(80));
        
        createCount = EditorGUILayout.IntField(createCount, GUILayout.Width(40));
        
        GUILayout.Label("Scene Type:",  GUILayout.Width(80));

        secenType = (SceneType)EditorGUILayout.EnumPopup(secenType, GUILayout.Width(140));
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUI.backgroundColor = Color.red;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Scene"))
        {
            List<SceneStateInfo> listSceneStateInfo = new List<SceneStateInfo>();
            
            for (int i = 0; i < createCount; i++)
            {
                objSceneData = Instantiate(area.listSceneObject[(int)secenType], area.transform);

                objSceneData.name = $"Scene{sceneIndex + i}_{secenType}";

                SceneStateInfo sceneStateInfo = new SceneStateInfo();
                
                sceneStateInfo.sceneData = objSceneData.GetComponent<TriggerScene>();
                sceneStateInfo.state = TypeSceneState.Wait;
                
                listSceneStateInfo.Add(sceneStateInfo);
            }
            
            area._listSceneDatas.AddRange(listSceneStateInfo);
        }

    }

    public enum SceneType
    {
        DAY_START,
        POKOTA_MISSION,
        NPC_MISSION,
        DAY_END
    }

    #endregion
}
