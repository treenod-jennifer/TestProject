using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using SideIcon;
using UnityEngine;

public class ManagerEventStage : MonoBehaviour, IEventBase
{
    static public ManagerEventStage instance = null;

    static public bool _eventStageClear = false;
    static public bool _eventStageFail = false;
    static public bool _eventStagePlayed = false;

    private IconStageEvent icon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Awake()
    {
        instance = this;
    }

    public static void Init()
    {
        if(CheckStartable() == false)
            return;
        if (instance != null) return;

        Global._instance.gameObject.AddMissingComponent<ManagerEventStage>();
        if (instance == null) return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool IsActiveEvent()
    {
        return CheckStartable();

    }

    private static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;
        
        return ServerContents.EventChapters != null && ServerContents.EventChapters.index != 0 && ServerContents.EventChapters.active == 1;
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.EVENT_STAGE;
    }

    void IEventBase.OnIconPhase()
    {
        if (IsActiveEvent())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconStageEvent>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) =>
            {
                icon.Init(ServerContents.EventChapters);
                this.icon = icon;
            });
        }
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public bool OnLobbySceneCheck()
    {
       return false;
    }

    void IEventBase.OnLobbyStart()
    {
        return;
    }

    void IEventBase.OnReboot()
    {
        _eventStageClear = false;
        _eventStageFail = false;
        UIPopupReady.eventStageClear = false;
        UIPopupReady.eventGroupClear = false;

        if(instance != null)
            Destroy(instance);

        return;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        _eventStageClear = false;
        _eventStageFail = false;
        
        yield break;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;

    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        EventChapterData.SetUserData();
        UIPopupReady.eventStageClear = false;
        UIPopupReady.eventStageFail = false;
        UIPopupReady.dicEventStageFlower.Clear();
        yield break;
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback )
    {
        if (ServerContents.EventChapterActivated())
        {
            var eventChapter = ServerContents.EventChapters;

            string eventBundleName = ManagerAssetLoader.GetNewBundleName(eventChapter.assetName, "_v2");

            ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
            {
                uri = eventBundleName,
                successCallback = null,
                failCallback = (r) =>
                {
                    Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                    failCallback(r, $"{eventBundleName}\n");
                    //ErrorController.ShowResourceDownloadFailed("Download Resource");
                }
            };
            loadList.Add(bundleReq);
        }

    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.EventStages != null && ServerRepos.EventStages.Count == 0)
            return false;
        else
            return true;
    }

    public void OnEventIconClick(object obj = null)
    {
        if (UIPopupReady._instance != null)
        {
            return;
        }

        if (ServerContents.EventChapters.type == (int)EVENT_CHAPTER_TYPE.FAIL_RESET)
        {
            if (ServerRepos.EventChapters.groupState > ServerContents.EventChapters.counts.Count)
            {
                if (UIPopupSystem._instance == null)
                {
                    UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                    string        title = Global._instance.GetString("p_t_4");
                    popup.InitSystemPopUp(title, Global._instance.GetString("n_ev_4"), false);
                    popup.SetResourceImage("Message/happy1");
                    popup.SortOrderSetting();
                }

                icon?.SetNewIcon(false);
                return;
            }
        }

        icon?.SetNewIcon(IsPlayCurrentVersion() == false);

        CdnEventChapter cdnData = ServerContents.EventChapters;

        int stageIndex = ServerRepos.EventChapters.stage;

        if (stageIndex > cdnData.counts[cdnData.counts.Count - 1])
        {
            if (cdnData.index == Global.eventIndex)
            {
                stageIndex = Global.stageIndex;
            }
            else
            {
                stageIndex = cdnData.counts[cdnData.counts.Count - 1];
            }
        }

        Global.SetGameType_Event(cdnData.index, stageIndex);

        ManagerUI._instance.OpenPopupReadyStageEvent();
    }
    
    /// <summary>
    /// 스테이지형 이벤트 팝업 강제 노출
    /// </summary>
    public IEnumerator CoOpenForceDisplayEventPopup()
    {
        if (CheckStartable() == false)
        {
            yield break;
        }
        
        if (UIPopupReady._instance != null)
        {
            yield break;
        }
        
        var cdnData = ServerContents.EventChapters;

        var stageIndex = ServerRepos.EventChapters.stage;

        if (stageIndex > cdnData.counts[cdnData.counts.Count - 1])
        {
            stageIndex = cdnData.index == Global.eventIndex ? Global.stageIndex : cdnData.counts[cdnData.counts.Count - 1];
        }

        Global.SetGameType_Event(cdnData.index, stageIndex);

        
        yield return ManagerUI._instance.CoOpenPopupReadyStageEvent();
    }
}
