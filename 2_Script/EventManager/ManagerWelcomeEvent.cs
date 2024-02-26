using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeMissionRewardData
{
    public int missionType;
    public int missionValue;
    public bool nGetReward;
    public bool vGetReward;
    public Reward nReward;
    public Reward vReward;

    public WelcomeMissionRewardData() { }

    public WelcomeMissionRewardData(int missionType, int missionValue, bool nGetReward, bool vGetReward, Reward nReward, Reward vReward)
    {
        this.missionType = missionType;
        this.missionValue = missionValue;
        this.nGetReward = nGetReward;
        this.vGetReward = vGetReward;
        this.nReward = nReward;
        this.vReward = vReward;
    }
}

public class ManagerWelcomeEvent : MonoBehaviour, IEventBase
{
    public static ManagerWelcomeEvent _instance { get; private set; }

    public static bool isBuyVipPass;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance != null)
            _instance = null;
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if(ServerContents.WelcomeMission == null || ServerRepos.UserWelcomeMission == null)
        {
            return false;
        }

        return Global.LeftTime(welcomeEndTs()) > 0;
    }

    public static long welcomeEndTs()
    {
        return ServerRepos.UserWelcomeMission.endTs;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (_instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerWelcomeEvent>();

        if (_instance == null)
            return;

        isBuyVipPass = ServerRepos.UserWelcomeMission.startPass > 0;

        ManagerEvent.instance.RegisterEvent(_instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.WELCOME_EVENT;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnIconPhase()
    {
        if(CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.NRUPass, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerWelcomeEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.WelcomeMission));
            });
        }
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield return null;
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    public void OnLobbyStart()
    {
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public void OnReboot()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public bool OnTutorialCheck()
    {
        return false;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public static bool IsBuyVipPass()
    {
        return ServerRepos.UserWelcomeMission.startPass > 0;
    }

    public static Reward VipPassGetReward()
    {
        if(ServerContents.Packages.ContainsKey(ServerContents.WelcomeMission.startPassPackageIdx) == false)
        {
            Reward reward = new Reward();
            reward.type = 3;
            reward.value = 10;

            return reward;
        }

        return ServerContents.Packages[ServerContents.WelcomeMission.startPassPackageIdx].items[0];
    }

    public static List<WelcomeMissionRewardData> GetWelcomeMissionData()
    {
        List<WelcomeMissionRewardData> listMissionData = new List<WelcomeMissionRewardData>();

        var missionContent = ServerContents.WelcomeMission.missionContent;
        var nMissionReward = ServerContents.WelcomeMission.completeReward;
        var vMissionReward = ServerContents.WelcomeMission.completePassReward;
        var nGetReward = ServerRepos.UserWelcomeMission.gainReward;
        var vGetReward = ServerRepos.UserWelcomeMission.gainPassReward;

        for (int i = 0; i < nMissionReward.Count; i++)
        {
            WelcomeMissionRewardData missionData = new WelcomeMissionRewardData(
                missionContent[i].type,
                missionContent[i].value,
                nGetReward[i] > 0,
                vGetReward[i] > 0,
                nMissionReward[i],
                vMissionReward[i]);

            listMissionData.Add(missionData);
        }

        return listMissionData;
    }

    public static int GetCurrentMission()
    {
        return (int)ServerRepos.User.day;
    }

    public static bool IsMissionClear(int type, int value)
    {
        switch (type)
        {
            case 1: //DayMission
                if(ServerRepos.User.day > value)
                    return true;
                break;
            case 2: //LoginMission
                if(ServerRepos.UserInfo.loginCnt >= value)
                    return true;
                break;
            case 3: //StageMission
                if(ServerRepos.User.stage > value)
                    return true;
                break;
            default:
                return false;
        }

        return false;
    }
}
