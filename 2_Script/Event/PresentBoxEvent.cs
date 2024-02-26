using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresentBoxEvent : AreaBase
{
    
    public TextAsset _jpStringData = null;
    public TextAsset _exStringData = null;

    public ObjectEvent _touchTarget;

    public override bool IsEventArea()
    {
        return true;
    }

    void Awake()
    {
        for (int i = 0; i < _listSceneDatas.Count; i++)
        {
            if (_listSceneDatas[i].sceneData != null)
            {
                _listSceneDatas[i].sceneData._sceneIndex = i + 1;

                _listSceneDatas[i].sceneData._triggerWait.gameObject.SetActiveRecursively(false);
                _listSceneDatas[i].sceneData._triggerWakeUp.gameObject.SetActiveRecursively(false);
                _listSceneDatas[i].sceneData._triggerActive.gameObject.SetActiveRecursively(false);
                _listSceneDatas[i].sceneData._triggerFinish.gameObject.SetActiveRecursively(false);
            }
        }
        
    }
    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }
    public void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(96f);

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
	// Use this for initialization
	void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());

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
    }
    void onTouch()
    {
        ManagerUI._instance.OpenPopupBoxShop();
    }

    public string GetString(string in_key)
    {
        if (_stringData.ContainsKey(in_key))
            return _stringData[in_key];
        else
            return in_key + ": string empty";
    }
}
