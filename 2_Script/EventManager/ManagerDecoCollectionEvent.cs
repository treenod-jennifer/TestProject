using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerDecoCollectionEvent : MonoBehaviour, IEventBase
{
    public static ManagerDecoCollectionEvent instance = null;
    public static int DecoCollectionEventIndex { get { return ServerRepos.UserDecoCollectionEvent.eventIndex; } }
    public static int DecoCollectCount { get { return ServerRepos.UserDecoCollectionEvent.collectCount; } }
    public static int DecoMaxCount { get { return ServerContents.DecoCollectionEvent.maxCount; } }
    public static bool IsGetReward { get { return ServerRepos.UserDecoCollectionEvent.isGetReward; } }
    public static long DecoCollectionEventEndTs { get { return ServerContents.DecoCollectionEvent.endTs; } }
    public static Reward DecoCollectionReward { get { return ServerContents.DecoCollectionEvent.completeReward; } }

    void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    
    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerDecoCollectionEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        var decoCollectionEvent = ServerContents.DecoCollectionEvent;
        
        if (decoCollectionEvent == null)
            return false;
        else
            return Global.LeftTime(decoCollectionEvent.endTs) > 0;
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.DECO_COLLECTION;
    }
    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnLobbyStart()
    {

    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconDecoCollectionEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ServerContents.DecoCollectionEvent));
        }
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        ManagerUI._instance.OpenPopup<UIPopupDecoCollectionEvent>((popup) => popup.InitData());
    }
}
