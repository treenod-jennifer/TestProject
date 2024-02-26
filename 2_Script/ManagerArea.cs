using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


// 하나의 챕터가 완료되면 
// = 이번 챕터의 after 리소스 로딩(필수) + 다음 챕터 active 로딩(필수) + 다음다음 before로딩(큰 공간의 경우나 특정 경우에 따라서는 복수의 챕터를 읽고 표시해야하는 경우나 이미 이전 챕터에서 읽고 표시하고 있는 경우도 있음)

// 주의 시나리오 
// > 보니집 미션 완료후.다른 공간의 몇 챕터 이후.. 보니집 뒤에 챕터가  늘어나면(예 5챕터라고 하면) 기존 보니집 요소(오브젝트 배치,땅타일,네비메쉬)가 수정되어야 함,(맵 확장에 따른 모서리 교체)
// 따라서 5챕터가 로딩될때 기존 보니집  after+common도 새로 받아져야함(가능한 배치파일이 프리팹만 교체), 주의사항 5챕터이전을 플레이하는 유저는 기존 after+common이여야함,,



[System.Serializable]
public class AreaStepInfo
{
    public string name = "";
    public int step = 0;
    public Area area = null;
}



public class ManagerArea : MonoBehaviour
{

    public static ManagerArea _instance = null;
    public bool _developMode = false;

    [System.NonSerialized]
    public Transform _transform;
   // [System.NonSerialized]
  //  public Dictionary<int, Area> _areaStep = new Dictionary<int, Area>();
   // [System.NonSerialized]
   // public Dictionary<int, Area> _areaData = new Dictionary<int,Area>();

    public List<AreaStepInfo> _areaCommonStep = new List<AreaStepInfo>();
    public List<AreaStepInfo> _areaStep = new List<AreaStepInfo>();
     
    [System.NonSerialized]
    public Dictionary<string, AreaEvent> _areaEventStep = new Dictionary<string, AreaEvent>();
    public PresentBoxEvent _presentBoxEvent = null;
    [System.NonSerialized]
    public Dictionary<int, ObjectEvent> _eventWaitObject = new Dictionary<int, ObjectEvent>();



    public Dictionary<string, TriggerScene> _extendTrigger = new Dictionary<string, TriggerScene>();

    public ObjectVisibleTagManager objVisibleTagMgr = new ObjectVisibleTagManager();

    void Awake()
    {
        _instance = this;
        _transform = transform;
    }
    void AutoSetAreaStep()
    {
        int maxArea = 0;
        for (int i = 0; i < _transform.childCount; i++)
        {
            string name = _transform.GetChild(i).name;
            if (name.Contains("a_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");

                if (int.Parse(matches[0].Value) > maxArea)
                    maxArea = int.Parse(matches[0].Value);
            }
            else if (name.Contains("ac_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");

                if (int.Parse(matches[0].Value) > maxArea)
                    maxArea = int.Parse(matches[0].Value);
            }
        }
        _areaCommonStep.Clear();
        _areaStep.Clear();
        _extendTrigger.Clear();
        for (int i = 0; i < maxArea; i++)
        {
            AreaStepInfo stepC = new AreaStepInfo();
            stepC.name = "ac_x";
            stepC.step = 0;
            _areaCommonStep.Add(stepC);


            AreaStepInfo step = new AreaStepInfo();
            step.name = "a_" + (i + 1) + "_x";
            step.step = 0;
            _areaStep.Add(step);
        }

        for (int i = 0; i < _transform.childCount; i++)
        {
            Transform cTransform = _transform.GetChild(i);
            if (!cTransform.gameObject.active)
                continue;

            string name = cTransform.name;
            if (name.Contains("a_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");
                int index = int.Parse(matches[0].Value) - 1;

                AreaStepInfo stepInfo = _areaStep[index];
                stepInfo.name = name;
                stepInfo.step = int.Parse(matches[1].Value);

                Area area = cTransform.GetComponent<Area>();
                stepInfo.area = area;

                if (area._extendTrigger != null)
                    for (int x = 0; x < area._extendTrigger.Count; x++)
                    {
                        string tName = area._extendTrigger[x].name;
                        _extendTrigger.Add(tName,area._extendTrigger[x]);
                    }
            }
            else if (name.Contains("ac_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");
                int index = int.Parse(matches[0].Value) - 1;

                AreaStepInfo commonStep = _areaCommonStep[index];
                commonStep.name = name;
                commonStep.step = int.Parse(matches[0].Value);
                commonStep.area = cTransform.GetComponent<Area>();
            }
        }
    }
    public string GetAreaString(int in_index, string in_key)
    {
        if (_areaStep.Count < in_index)
            return "null";

        return _areaStep[in_index - 1].area.GetString(in_key);
    }
    public void TriggerSettingEvent()
    {/*
        foreach (var item in ManagerData._instance._eventChapterData)
        {
            if (item.Value._active == 0)
                continue;
            
            CdnEventChapter cdnData = ServerContents.EventChapters[item.Value.index];
            AreaEvent areaData = null;

            if (_areaEventStep.TryGetValue(cdnData.assetName, out areaData))
            {
                for (int x = 0; x < areaData._listSceneDatas.Count; x++)
                {
                    if (item.Value._groupState - 1 < x)
                        areaData._listSceneDatas[x].state = TypeSceneState.Wait;
                    else if (item.Value._groupState - 1 == x)
                        areaData._listSceneDatas[x].state = TypeSceneState.Active;
                    else
                        areaData._listSceneDatas[x].state = TypeSceneState.Finish;
                }

            }
        }

        if (ManagerArea._instance._presentBoxEvent != null && ServerRepos.LoginCdn.SaleNpc > 0 )
        {
            if (ServerRepos.User.missionCnt >= ServerRepos.LoginCdn.SaleNpc )
            {
                int shopResVer = ServerRepos.LoginCdn.shopResourceId;
                if ( PlayerPrefs.HasKey("boxEventOpenShow") && PlayerPrefs.GetInt("boxEventOpenShow") == shopResVer)
                    _presentBoxEvent._listSceneDatas[0].state = TypeSceneState.Active;
            }
        }

        
        for (int i = 0; i < ServerRepos.EventChapters.Count; i++)
        {
            ServerEventChapter userData = ServerRepos.EventChapters[i];
            CdnEventStage cdnData = ServerContents.EventStages[userData.eventIndex];
            AreaEvent areaData = null;

            if (_areaEventStep.TryGetValue(cdnData.assetName,out areaData))
            {
                for (int x = 0; x < areaData._listSceneDatas.Count; x++)
                {



                    if (userData.groupState - 1 < x)
                        areaData._listSceneDatas[x].state = TypeSceneState.Wait;
                   else if (userData.groupState - 1 == x)
                        areaData._listSceneDatas[x].state = TypeSceneState.Active;
                    else
                        areaData._listSceneDatas[x].state = TypeSceneState.Finish;


                   Debug.Log("!!!!!!!!!!!!!!!!!!!  " + userData.groupState + " "  + areaData._listSceneDatas[x].state);

                }
                
            }
        }*/
    }
    public void TriggerSetting()
    {
        AutoSetAreaStep();

        if (_developMode)
            return;

        for (int i = 0; i < _areaStep.Count; i++)
        {
            if (_areaStep[i].area != null)
                _areaStep[i].area.SetAllTriggerState(TypeSceneState.Wait);
        }
        foreach (var item in ManagerData._instance._missionData)
        {
            int area = item.Value.sceneArea - 1;
            TypeMissionState state = item.Value.state;

            int sceneIndex = item.Value.sceneIndex - 1;

            if (_areaStep.Count > area)
            {
                if (_areaStep[area].area != null)
                {
                    if (_areaStep[area].area._listSceneDatas != null)
                    {
                     //   if (_areaStep[area].area._listSceneDatas.Count > i)
                        {
                            if (sceneIndex >= _areaStep[area].area._listSceneDatas.Count)
                                continue;

                            //Debug.Log("dddddddddddddddd  " + item.Key + "___" + state);

                            if (state == TypeMissionState.Active)
                            {
                                _areaStep[area].area._listSceneDatas[sceneIndex].state = TypeSceneState.Active;

                                if (item.Value.stepClear > 1)
                                {
                                    _areaStep[area].area._listSceneDatas[sceneIndex + item.Value.clearCount].state = TypeSceneState.Active;

                                    for (int c = 0; c < item.Value.clearCount; c++)
                                        _areaStep[area].area._listSceneDatas[sceneIndex + c].state = TypeSceneState.Finish;
                                }
                                else if (item.Value.waitTime > 0)
                                {
                                    // 미션 활성화
                                    if (item.Value.clearTime == 0)
                                    {
                                        _areaStep[area].area._listSceneDatas[sceneIndex + 1].state = TypeSceneState.Wait;
                                    }
                                    else // 미션 완료 시간 시다리기
                                    {
                                        _areaStep[area].area._listSceneDatas[sceneIndex].state = TypeSceneState.Finish;
                                        _areaStep[area].area._listSceneDatas[sceneIndex + 1].state = TypeSceneState.Active;
                                    }
                                }
                                
                            }
                            else if (state == TypeMissionState.Clear)
                            {
                                _areaStep[area].area._listSceneDatas[sceneIndex].state = TypeSceneState.Finish;
                                for (int c = 1; c < item.Value.stepClear; c++)
                                    _areaStep[area].area._listSceneDatas[sceneIndex + c].state = TypeSceneState.Finish;
                                
                                if (item.Value.waitTime > 0)
                                    _areaStep[area].area._listSceneDatas[sceneIndex + 1].state = TypeSceneState.Finish;
                               
                            }

                            if (_areaStep[area].area._listSceneDatas[sceneIndex].sceneData != null)
                                _areaStep[area].area._listSceneDatas[sceneIndex].sceneData._missionIndex = item.Key;
                        }
                    }
                }
            }
        }

    /*    for (int i = 0; i < ManagerData._instance.missionData.Count; i++)
        {
            int area = ManagerData._instance.missionData[i].sceneArea - 1;
            TypeMissionState state = ManagerData._instance.missionData[i].state;

            int sceneIndex = ManagerData._instance.missionData[i].sceneIndex - 1;

            if (_areaStep.Count > area)
            {
                if (_areaStep[area].area != null)
                {
                    if (_areaStep[area].area._listSceneDatas != null)
                    {
                        if (_areaStep[area].area._listSceneDatas.Count > i)
                        {
                            if (sceneIndex >= _areaStep[area].area._listSceneDatas.Count)
                                continue;

                            if (state == TypeMissionState.Active)
                            {
                                _areaStep[area].area._listSceneDatas[sceneIndex].state = TypeSceneState.Active;

                                if (ManagerData._instance.missionData[i].stepClear > 1)
                                {  
                                    _areaStep[area].area._listSceneDatas[sceneIndex + ManagerData._instance.missionData[i].clearCount].state = TypeSceneState.Active;

                                    for (int c = 0; c < ManagerData._instance.missionData[i].clearCount; c++)
                                        _areaStep[area].area._listSceneDatas[sceneIndex + c].state = TypeSceneState.Finish;
                                }

                            }
                            else if (state == TypeMissionState.Clear)
                            {
                                _areaStep[area].area._listSceneDatas[sceneIndex].state = TypeSceneState.Finish;
                                for (int c = 1; c < ManagerData._instance.missionData[i].stepClear; c++)
                                    _areaStep[area].area._listSceneDatas[sceneIndex + c].state = TypeSceneState.Finish;
                            }

                            if (_areaStep[area].area._listSceneDatas[sceneIndex].sceneData != null)
                                _areaStep[area].area._listSceneDatas[sceneIndex].sceneData._missionIndex = i + 1;
                        }
                    }
                }
            }
        }*/

        for (int i = 0; i < LobbyBehavior._instance._behaviorScene.Count; i++)
        {
            
            LobbyBehavior._instance._behaviorScene[i]._missionIndex = LobbyBehavior._instance._behaviorScene[i].transform.parent.GetComponent<TriggerScene>()._missionIndex;


     //       Debug.Log(LobbyBehavior._instance._behaviorScene[i].transform.parent.GetComponent<TriggerScene>()._missionIndex);
   //         Debug.Log(LobbyBehavior._instance._behaviorScene[i]._missionIndex + " " + LobbyBehavior._instance._behaviorScene[i].name + "  " + LobbyBehavior._instance._behaviorScene[i].transform.parent.name);
        //    Debug.Log(LobbyBehavior._instance._behaviorScene[i]._missionIndex);
      //      if (LobbyBehavior._instance._behaviorScene[i]._missionIndex > 0 && LobbyBehavior._instance._behaviorScene[i]._touchObject.Count == 0)
          //      LobbyBehavior._instance._behaviorScene.Add(LobbyBehavior._instance._behaviorScene[i]);
        }

    }
    public void TriggerStartEvent()
    {
        foreach (var item in _areaEventStep)
        {
            item.Value.TriggerStart();
        }

        if (_presentBoxEvent != null)
            _presentBoxEvent.TriggerStart();
    }
    public void TriggerStart()
    {
        for (int i = 0; i < _areaStep.Count; i++)
        {
            if (_areaStep[i].area != null)
                _areaStep[i].area.TriggerStart();
        }
    }
	// Use this for initialization
	void Start ()
    {

        /*
        // 글로발 스트링  
        {
            char[] separator = new char[] { ',' };
            TextAsset asset = null;
            asset = Resources.Load<TextAsset>("global");
            if (asset != null)
            {
                string str = asset.text;
                string[] strChatList = str.Split('\n');
                for (int i = 0; i < strChatList.Length; i++)
                {

                    string[] strChat = strChatList[i].Split(separator, 2);




                    if (strChat.Length == 2)
                    {
                        if (strChat[0].Length > 1 && strChat[1].Length > 1)
                        {
                            string ss = strChat[1].Replace("\"", "");
                            ss = ss.Replace("\\n", "\n");

                            string key = strChat[0].Replace("\"", "");

                            //Debug.Log(key+""+ss);

                            _stringData.Add(key, ss);
                        }
                    }
                }
            }
        }*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
