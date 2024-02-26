using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneStateInfo
{
    public TriggerScene sceneData = null;
    public TypeSceneState state = TypeSceneState.Wait;
}

public class AreaBase : MonoBehaviour
{
    public List<SceneStateInfo> _listSceneDatas = new List<SceneStateInfo>();

    [System.NonSerialized]
    public Dictionary<string, string> _stringData = new Dictionary<string, string>();

    public List<TypeCharacterType> _characters = new List<TypeCharacterType>();
    public List<TypeCharacterType> _live2dChars = new List<TypeCharacterType>();

    public virtual bool IsEventArea() { return false; }
}

public class Area : AreaBase
{
    public TextAsset _naviData = null;
    public TextAsset _jpStringData = null;
    public TextAsset _exStringData = null;


    public Transform _defaultCharacterPosition = null;
    public Vector2 _defaultCharacterOffset;
    public Transform _defaultCameraPosition = null;
    public Vector2 _defaultCameraOffset;
    public float _defaultZoom = 0f;

    public List<TriggerScene> _extendTrigger = new List<TriggerScene>();

    void Awake()
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
            {
                _listSceneDatas[i].sceneData._sceneIndex = i+1;
                if (_listSceneDatas[i].state == TypeSceneState.Wait)
                {
                    _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerWakeUp.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActiveRecursively(false);
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Active)
                {
                    _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerWakeUp.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActiveRecursively(false);
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Finish)
                {
                    _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerWakeUp.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActiveRecursively(false);
                    _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActiveRecursively(false);
                }
            }
        }

        for (int i = 0; i < _extendTrigger.Count; i++)
        {
            if (_extendTrigger[i] != null)
            {
                _extendTrigger[i]._triggerWait.gameObject.SetActiveRecursively(false);
                _extendTrigger[i]._triggerWakeUp.gameObject.SetActiveRecursively(false);
                _extendTrigger[i]._triggerActive.gameObject.SetActiveRecursively(false);
                _extendTrigger[i]._triggerFinish.gameObject.SetActiveRecursively(false);
            }
        }
    }
    void Start()
    {
        if (_naviData != null && LobbyBehavior._instance != null)
            AstarPath.active.astarData.DeserializeGraphs(_naviData.bytes);

        /*
        // 에리아 스트링  
        {
            char[] separator = new char[] { ',' };
            TextAsset asset = null;
            asset = Resources.Load<TextAsset>(gameObject.name);
            if (asset != null)
            {
                string str = asset.text;
                string[] strChatList = str.Split('\n');
                for (int i = 0; i < strChatList.Length; i++)
                {   
                    string[] strChat = strChatList[i].Split(separator,2);
                    
                    if (strChat.Length == 2)
                    {
                        if (strChat[0].Length > 1 && strChat[1].Length > 1)
                        {
                            string ss = strChat[1].Replace("\"", "");
                            ss = ss.Replace("\\n", "\n");
                            string key = strChat[0].Replace("\"", "");
                            //Debug.Log(key+" "+ss);
                            _stringData.Add(key, ss);
                        }
                    }
                }

               // Debug.Log(_stringData["a1_s1_c1"] + "  " + _stringData["a1_s1_c2"]);
            }
        }*/

        {
            char[] separator = new char[] { ',' };
            TextAsset asset = null;
            if (Global._systemLanguage == CurrLang.eJap)
                asset = _jpStringData;
            else
                asset = _exStringData;

            if (asset != null)
            {
                string str = asset.text;
                string[] strSeperator = new string[] { "\n=" };
                string[] strChatList = str.Split(strSeperator, System.StringSplitOptions.None);
                for (int i = 0; i < strChatList.Length; i++)
                {
                    string[] strChat = strChatList[i].Split(separator, 2);
                    if (strChat.Length == 2)
                    {
                        if (strChat[0].Length > 1 && strChat[1].Length > 1)
                        {
                            string key = strChat[0];
                            _stringData.Add(key, strChat[1]);
                        }
                    }
                }
            }
        }
    /*    string language = "";
        if (Global._systemLanguage == CurrLang.eJap)
            language = "_jp";
        else if (Global._systemLanguage == CurrLang.eKor)
            language = "_ko";
        else if (Global._systemLanguage == CurrLang.eEng)
            language = "_jp";

        // 에리아 스트링  
        {
            char[] separator = new char[] { ',' };
            TextAsset asset = null;
            asset = Resources.Load<TextAsset>(gameObject.name + language);
            if (asset != null)
            {  
                string str = asset.text;
                string[] strSeperator = new string[] { "\n=" };
                string[] strChatList = str.Split(strSeperator, System.StringSplitOptions.None);
                for (int i = 0; i < strChatList.Length; i++)
                {
                    string[] strChat = strChatList[i].Split(separator, 2);
                    if (strChat.Length == 2)
                    {
                        if (strChat[0].Length > 1 && strChat[1].Length > 1)
                        {
                            string key = strChat[0];
                            _stringData.Add(key, strChat[1]);
                        }
                    }
                }
            }
        }*/
    }
    public string GetString(string in_key)
    {
        if (_stringData.ContainsKey(in_key))
        {
            //Debug.Log(gameObject.name + "   " + in_key + "  파일에서 이런 이런 키는 없음");
            return _stringData[in_key];
        }
        else
            return in_key + ": string empty";
    }
    public void SetAllTriggerState(TypeSceneState in_state)
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
                _listSceneDatas[i].state = in_state;
        }
    }
    public void TriggerStart()
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
            {

                if (_listSceneDatas[i].state == TypeSceneState.Wait)
                {
                    _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerWait.StartCondition();
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Active)
                {
                    _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerActive.StartCondition();
                }
                else if (_listSceneDatas[i].state == TypeSceneState.Finish)
                {
                    _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActive(true);
                    _listSceneDatas[i].sceneData._triggerFinish.StartCondition();
                }
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        if (_defaultCharacterPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_defaultCharacterPosition.position + new Vector3(_defaultCharacterOffset.x, _defaultCharacterOffset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }

        if (_defaultCameraPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_defaultCameraPosition.position + new Vector3(_defaultCameraOffset.x, _defaultCameraOffset.y), 0.5f);
        }
    }
    //public 
}
