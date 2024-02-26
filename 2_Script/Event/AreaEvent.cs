using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEvent : AreaBase, IEventLobbyObject
{
    static public AreaEvent instance = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    [SerializeField] private string textFileName;

    [System.NonSerialized]
    public CdnEventChapter _data = null;
    public ObjectEvent _touchTarget;
    [System.NonSerialized]
    public EventObjectUI _uiEvent = null;

    public override bool IsEventArea()
    {
        return true;
    }

    void Awake()
    {
        instance = this;

        InitSceneDatas();

        var lobbyFocus = gameObject.GetComponent<LobbyEntryFocus>();
        if (lobbyFocus == null)
        {
            lobbyFocus = gameObject.AddMissingComponent<LobbyEntryFocus>();
            lobbyFocus.focusData = new FocusData()
            {
                camTransform = this.transform,
                camOffset = Vector2.zero,
                charPos = null,
                charOffset = Vector2.zero,
                zoom = 0f
            };

            lobbyFocus.focusPriority = 100000;
            lobbyFocus.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnActionUIActive(bool in_active)
    {
        if (_uiEvent != null)
            _uiEvent.gameObject.SetActive(in_active);
    }
    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);
        
        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }
	// Use this for initialization
	void Start ()
    {
        string textName = string.Format("area" + this.name);

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }

        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());
        
        if (_data != null)
        {
            CdnEventChapter cdnData = ServerContents.EventChapters;

            //이벤트 그룹을 사용하는 이벤트 타입인지 검사.
            bool isEventType_UseGroup = (cdnData.type != (int)EVENT_CHAPTER_TYPE.SCORE);
            int groupState = (isEventType_UseGroup == false) ? 1 : ServerRepos.EventChapters.groupState;
          
            //마지막 씬을 본 상태라면 ui 없애줌.
            if ((isEventType_UseGroup == false && (ServerRepos.EventChapters.groupState > cdnData.rewardFlowerCount.Count))
                || (isEventType_UseGroup == true && (groupState > _data.counts.Count)))
            {
                _uiEvent.gameObject.SetActive(false);
            }
            else
            {
                int stageCnt = 0;
                int allStageCnt = 0;

                if (cdnData.type == (int)EVENT_CHAPTER_TYPE.SCORE) //스코어 모드에서는 별 카운트 표시
                {
                    int rewardFlowerCnt = _data.rewardFlowerCount.Count;
                    allStageCnt = _data.rewardFlowerCount[rewardFlowerCnt - 1];

                    //현재 이벤트의 스테이지 리스트 가져옴.
                    List<ServerEventStage> listEventStage = new List<ServerEventStage>();
                    foreach (var item in ServerRepos.EventStages)
                    {
                        if (item.eventIdx == _data.index)
                        {
                            listEventStage.Add(item);
                        }
                    }

                    int stageCount = _data.counts[0];
                    for (int i = 0; i < stageCount; i++)
                    {
                        var stage = listEventStage.Find(x => x.stage == (i + 1));
                        stageCnt += (stage == null) ? 0 : stage.flowerLevel;
                    }
                }
                else //다른 모드에서는 클리어 한 스테이지 카운트 표시
                {
                    allStageCnt = _data.counts[groupState - 1];
                    stageCnt = ServerRepos.EventChapters.stage - 1;
                    if (groupState > 1)
                    {
                        allStageCnt -= _data.counts[groupState - 2];
                        stageCnt -= _data.counts[groupState - 2];
                    }
                }
                if (_uiEvent != null)
                    _uiEvent._uiLevel.text = stageCnt + "/" + allStageCnt;
            }
        }
	}

    void onTouch()
    {
        if (UIPopupReady._instance != null)
            return;
        
        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            if (ServerRepos.EventChapters.groupState > ServerContents.EventChapters.counts.Count)
            {
                if (UIPopupSystem._instance == null)
                {
                    string title = Global._instance.GetString("p_t_4");
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_4"), false);
                    popup.SetResourceImage("Message/happy1");
                    popup.SortOrderSetting();
                }
                return;
            }
        }

        CdnEventChapter cdnData = ServerContents.EventChapters;

        if (cdnData.active <= 0)
            return;
        if(!ManagerEventStage.IsActiveEvent())
            return;

        int stageIndex = ServerRepos.EventChapters.stage;

        if (stageIndex > cdnData.counts[cdnData.counts.Count - 1])
            stageIndex = cdnData.counts[cdnData.counts.Count - 1];

        Global.SetGameType_Event(_data.index, stageIndex);

        ManagerUI._instance.OpenPopupReadyStageEvent();

        //Debug.Log(Global.stageIndex + " / " + Global.eventIndex + " groupState  " + ServerRepos.EventChapters[_data.index].groupState);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.EVENT_STAGE;
    }

    

    public void Invalidate()
    {
    }

    public void TriggerSetting()
    {
        if (ManagerData._instance._eventChapterData != null)
        {
            var eventChapData = ManagerData._instance._eventChapterData;
            CdnEventChapter cdnData = ServerContents.EventChapters;
            for (int x = 0; x < _listSceneDatas.Count; x++)
            {
                if (cdnData.type != (int)EVENT_CHAPTER_TYPE.SCORE)
                {
                    if (eventChapData._groupState - 1 < x)
                        _listSceneDatas[x].state = TypeSceneState.Wait;
                    else if (eventChapData._groupState - 1 == x)
                        _listSceneDatas[x].state = TypeSceneState.Active;
                    else
                        _listSceneDatas[x].state = TypeSceneState.Finish;
                }
                else
                {
                    //모든 보상 목표를 다 달성했는지.
                    bool isCompleteGetReward = (eventChapData._groupState > cdnData.rewardFlowerCount.Count);

                    switch (x)
                    {
                        //시작 연출
                        case 0:
                            //보상을 전부 획득한 상태라면 첫번재 씬은 종료 상태
                            if (isCompleteGetReward == true)
                                _listSceneDatas[x].state = TypeSceneState.Finish;
                            else if (eventChapData._groupState == 0)
                                _listSceneDatas[x].state = TypeSceneState.Wait;
                            else
                                _listSceneDatas[x].state = TypeSceneState.Active;
                            break;
                        //마지막 연출
                        default:
                            if (isCompleteGetReward == true)
                                _listSceneDatas[x].state = TypeSceneState.Active;
                            else
                                _listSceneDatas[x].state = TypeSceneState.Wait;
                            break;
                    }
                }
            }
        }

    }

    static public IEnumerator LoadObject()
    {
        var eventChapterInfo = ServerContents.EventChapters;

        //번들 받아지지 않은 경우에 로비 진입 시 추가.
        if (!ManagerLobby._assetBankEvent.ContainsKey(eventChapterInfo.assetName))
        {
            yield return LoadEventStageBundle(eventChapterInfo);
        }

        GameObject obj = null;
        if (ManagerLobby._assetBankEvent.ContainsKey(eventChapterInfo.assetName))
            obj = ManagerLobby.NewObject(ManagerLobby._assetBankEvent[eventChapterInfo.assetName]);

        if (obj != null)
        {
            AreaEvent areaEvent = obj.GetComponent<AreaEvent>();
            areaEvent._data = eventChapterInfo;
            ManagerArea._instance.RegisterEventLobbyObject(areaEvent);
            {
                EventChapterData data = ManagerData._instance._eventChapterData;
                if (data != null)
                {
                    GameObject ui = Instantiate<GameObject>(ManagerLobby._instance._objEventObjectUI);
                    ui.transform.parent = areaEvent._touchTarget.transform;
                    ui.transform.localPosition = new Vector3(0.2f, 0.2f, -0.2f);
                    areaEvent._uiEvent = ui.GetComponent<EventObjectUI>();

                    if (data._groupState <= 0)
                        ui.SetActive(false);
                }
            }

            AreaBase areaBase = obj.GetComponent<AreaBase>();
            if (areaBase)
            {
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
            }
        }
        yield return null;
    }

    static public IEnumerator LoadObject_Outland()
    {
        var eventChapterInfo = ServerContents.EventChapters;

        //번들 받아지지 않은 경우에 로비 진입 시 추가.
        if (!ManagerLobby._assetBankEvent.ContainsKey(eventChapterInfo.assetName))
        {
            yield return LoadEventStageBundle(eventChapterInfo);
        }

        if (ManagerLobby._assetBankEvent.ContainsKey(eventChapterInfo.assetName))
        {

            AreaBase areaBase = ManagerLobby._assetBankEvent[eventChapterInfo.assetName].GetComponent<AreaBase>();
            if (areaBase)
            {
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                yield return ManagerCharacter._instance.LoadCharacters();
            }

        }
    }


    public static IEnumerator LoadEventStageBundle(CdnEventChapter eventChapter)
    {
        if (Global.LoadFromInternal)
            LoadEventStageBundleFromInternal(eventChapter);
        else
            yield return LoadEventStageBundleFromBundle(eventChapter);
    }

    public static void LoadEventStageBundleFromInternal(CdnEventChapter eventChapter)
    {
        string bundelName = ManagerAssetLoader.GetNewBundleName(eventChapter.assetName, "_v2");
        string path = "Assets/5_OutResource/events/" + bundelName;
        string bundelPath = path + "/" + eventChapter.assetName + ".prefab";
#if UNITY_EDITOR
        GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(bundelPath);
        ManagerLobby._assetBankEvent.Add(eventChapter.assetName, BundleObject);

        string readyBundleName = string.Format(eventChapter.assetName + "_ready");
        for (int x = 0; x < eventChapter.counts.Count; x++)
        {
            string readyPath = path + "/" + readyBundleName + (x + 1) + ".prefab";
            GameObject objReady = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(readyPath);
            ManagerLobby._assetBankEvent.Add(readyBundleName + (x + 1), objReady);
        }
#endif
    }

    public static IEnumerator LoadEventStageBundleFromBundle(CdnEventChapter eventChapter)
    {
        string bundelName = ManagerAssetLoader.GetNewBundleName(eventChapter.assetName, "_v2");

        NetworkLoading.MakeNetworkLoading(0.5f);
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundelName);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                yield return LoadEventStageBundleFromBundle(eventChapter, assetBundle);
            }
        }
        NetworkLoading.EndNetworkLoading();
    }

    public static IEnumerator LoadEventStageBundleFromBundle(CdnEventChapter eventChapter, AssetBundle assetBundle)
    {
        GameObject obj = assetBundle.LoadAsset<GameObject>(eventChapter.assetName);
        ManagerLobby._assetBankEvent.Add(eventChapter.assetName, obj);

        string readyBundleName = string.Format(eventChapter.assetName + "_ready");
        for (int x = 0; x < eventChapter.counts.Count; x++)
        {
            var bundleAsync = assetBundle.LoadAssetAsync<GameObject>(readyBundleName + (x + 1));
            yield return bundleAsync;
            if( bundleAsync.isDone )
            {
                GameObject objReady = bundleAsync.asset as GameObject;
                ManagerLobby._assetBankEvent.Add(readyBundleName + (x + 1), objReady);
            }
        }
    }
}
