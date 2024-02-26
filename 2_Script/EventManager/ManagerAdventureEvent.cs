using System;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public class ManagerAdventureEvent : MonoBehaviour, IEventBase
{
    public static ManagerAdventureEvent instance;

    private IconAdventureEvent icon;
    
    void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        icon?.SetNewIcon(IsPlayCurrentVersion() == false);
        UIButtonAdventureEvent.OpenEvent();
    }

    private static bool CheckStartable()
    {
        return ManagerAdventure.EventData.GetActiveEvent_AdventureEvent();
    }

    public static void Init()
    {
        if (CheckStartable() == false)
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerAdventureEvent>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.ADVENTURE_EVENT;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback) { }

    public void OnLobbyStart() { }

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
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
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

    public IEnumerator OnRewardPhase() 
    {
        yield break;
    }

    public void OnIconPhase() 
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconAdventureEvent>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) =>
            {
                icon.Init(ServerContents.EventAdv);
                this.icon = icon;
            });
        }
    }
    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserEventAdventure != null && ServerRepos.UserEventAdventureStages.FindAll(x => x.event_index == ServerContents.EventAdv.event_idx).Count == 0)
            return false;
        else
            return true;
    }
}
