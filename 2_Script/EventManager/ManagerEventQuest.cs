using System;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public class ManagerEventQuest : MonoBehaviour, IEventBase
{
    public static ManagerEventQuest instance = null;
    public static List<QuestGameData> eventQuestList = null;

    private long endTime;
    private IconEventQuest icon;
    
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
        if (eventQuestList == null)
            UpdateGameData();
        
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerEventQuest>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return false;

        if (eventQuestList == null || eventQuestList.Count < 1)
            return false;
        
        return Global.LeftTime(eventQuestList[0].valueTime1) > 0;
    }
    
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.EVENT_QUEST;
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
            SideIcon.Maker.MakeIcon<SideIcon.IconEventQuest>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    endTime = eventQuestList[0].valueTime1;
                    icon.Init(eventQuestList);
                    this.icon = icon;
                });
        }
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        UpdateGameData();
        icon?.SetNewIcon();
            
        if (eventQuestList.Count > 0)
        {
            ManagerUI._instance.OpenPopup<UIPopupEventQuest>((popup) => popup.InitPopup());
        }
        else
        {
            string strKey = "n_s_68";
            if (Global.LeftTime(endTime) > 0)
            {
                strKey = "n_ev_19";
            }

            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(strKey), false);
        }
    }

    public static void UpdateGameData()
    {
        eventQuestList = new List<QuestGameData>();
        foreach (var item in ManagerData._instance._questGameData)
        {
            var qData = item.Value;
            if (qData.state != QuestState.Finished && qData.valueTime1 >= Global.GetTime())
                eventQuestList.Add(qData);
        }
    }
}
