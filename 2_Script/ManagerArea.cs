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

    public SortedDictionary<int, AreaStepInfo> _areaCommonStep = new SortedDictionary<int, AreaStepInfo>();
    public SortedDictionary<int, AreaStepInfo> _areaStep = new SortedDictionary<int, AreaStepInfo>();

    [System.NonSerialized]
    public AdventureEntry _adventureEntry = null;

    [System.NonSerialized]
    Dictionary<GameEventType, IEventLobbyObject> _eventLobbyObjects = new Dictionary<GameEventType, IEventLobbyObject>();

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
        HashSet<int> areaSet = new HashSet<int>();
        for (int i = 0; i < _transform.childCount; i++)
        {
            string name = _transform.GetChild(i).name;
            if (name.Contains("a_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");

                areaSet.Add(int.Parse(matches[0].Value));
                
            }
            else if (name.Contains("ac_"))
            {
                MatchCollection matches = Regex.Matches(name, "[0-9]+");

                areaSet.Add(int.Parse(matches[0].Value));
            }
        }
        _areaCommonStep.Clear();
        _areaStep.Clear();
        _extendTrigger.Clear();
        foreach(var areaIdx in areaSet)
        {
            AreaStepInfo stepC = new AreaStepInfo();
            stepC.name = "ac_x";
            stepC.step = 0;
            _areaCommonStep.Add (areaIdx,  stepC);


            AreaStepInfo step = new AreaStepInfo();
            step.name = "a_" + (areaIdx) + "_x";
            step.step = 0;
            _areaStep.Add(areaIdx, step);
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
                int index = int.Parse(matches[0].Value);

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
                int index = int.Parse(matches[0].Value);

                if( _areaCommonStep.ContainsKey(index) )
                {
                    AreaStepInfo commonStep = _areaCommonStep[index];
                    commonStep.name = name;
                    commonStep.step = int.Parse(matches[0].Value);
                    commonStep.area = cTransform.GetComponent<Area>();
                }                
            }
        }
    }

    public void TriggerSettingEvent()
    {
        foreach (var e in this._eventLobbyObjects)
        {
            e.Value?.TriggerSetting();
        }

        if ( ManagerArea._instance._adventureEntry != null)
        {
            if (PlayerPrefs.HasKey(ManagerAdventure.OpenSceneKey) == true)
                ManagerArea._instance._adventureEntry._listSceneDatas[0].state = TypeSceneState.Active;
        }
    }
    public void TriggerSetting(Dictionary<int, MissionData> missionData)
    {
        AutoSetAreaStep();

        if (_developMode)
            return;

        foreach( var areaStep in _areaStep)
        {
            if (areaStep.Value.area != null)
                areaStep.Value.area.SetAllTriggerState(TypeSceneState.Wait);
        }
        foreach (var item in missionData)
        {
            int area = item.Value.sceneArea;
            TypeMissionState state = item.Value.state;

            int sceneIndex = item.Value.sceneIndex - 1;

            if ( _areaStep.ContainsKey(area) )
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

    

        for (int i = 0; i < LobbyBehavior._instance._behaviorScene.Count; i++)
        {
            LobbyBehavior._instance._behaviorScene[i]._missionIndex = LobbyBehavior._instance._behaviorScene[i].transform.parent.GetComponent<TriggerScene>()._missionIndex;
        }

    }
    public void TriggerStartEvent()
    {
        _adventureEntry?.TriggerStart();

        foreach( var e in this._eventLobbyObjects)
        {
            e.Value?.TriggerStart();
        }
    }
    public void TriggerStart()
    {
        foreach(var a in _areaStep)
        {
            a.Value?.area?.TriggerStart();
        }
    }
	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RegisterEventLobbyObject(IEventLobbyObject lobbyObject)
    {
        if( this._eventLobbyObjects.ContainsKey(lobbyObject.GetEventType()))
        {
            _eventLobbyObjects[lobbyObject.GetEventType()].Invalidate();

            _eventLobbyObjects.Remove(lobbyObject.GetEventType());
        }

        _eventLobbyObjects.Add(lobbyObject.GetEventType(), lobbyObject);
    }

    public IEventLobbyObject GetEventLobbyObject(GameEventType et)
    {
        if( _eventLobbyObjects.ContainsKey(et) )
        {
            return _eventLobbyObjects[et];
        }
        return null;
    }
}
