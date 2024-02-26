using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeBackMissionRewardData
{
    public int missionType;
    public int missionValue;
    public bool nGetReward;
    public bool vGetReward;
    public Reward nReward;
    public Reward vReward;

    public WelcomeBackMissionRewardData() { }

    public WelcomeBackMissionRewardData(int missionType, int missionValue, bool nGetReward, bool vGetReward, Reward nReward, Reward vReward)
    {
        this.missionType = missionType;
        this.missionValue = missionValue;
        this.nGetReward = nGetReward;
        this.vGetReward = vGetReward;
        this.nReward = nReward;
        this.vReward = vReward;
    }
}

public class ManagerWelcomeBackEvent : MonoBehaviour, IEventBase
{
    public static ManagerWelcomeBackEvent _instance { get; private set; }

    public enum ContentsType
    {
        InvalidContents = 0,
        WelcomeBack = 1,
        LoginMore = 2,
    }

    ContentsType contentsType;

    public bool buyPassNeedReboot;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance != null)
            _instance = null;
    }

    static CdnWelcomeBackMission GetWelcomeBackMission(ContentsType t)
    {
        for (int i = 0; i < ServerContents.WelcomeBackMission.Length; ++i)
        {
            if ((ContentsType)ServerContents.WelcomeBackMission[i].contentsType == t)
            {
                return ServerContents.WelcomeBackMission[i];
            }
        }
        return null;
    }

    public static CdnWelcomeBackMission GetWelcomeBackMission()
    {
        if (_instance == null)
            return null;
        return GetWelcomeBackMission(_instance.contentsType);
    }

    static ServerUserWelcomeBackMission GetUserWelcomeBackMission(ContentsType t)
    {
        for (int i = 0; i < ServerRepos.UserWelcomeBackMission.Length; ++i)
        {
            if ((ContentsType)ServerRepos.UserWelcomeBackMission[i].contentsType == t)
            {
                return ServerRepos.UserWelcomeBackMission[i];
            }
        }
        return null;
    }

    public static ServerUserWelcomeBackMission GetUserWelcomeBackMission()
    {
        if (_instance == null)
            return null;
        return GetUserWelcomeBackMission(_instance.contentsType);
    }

    static bool CheckStartable()
    {
        if(GameData.User.day < 2)
        {
            return false;
        }

        if(ServerContents.WelcomeBackMission == null || ServerRepos.UserWelcomeBackMission == null)
        {
            return false;
        }

        if (CalcCurrentContentsType() == ContentsType.InvalidContents)
            return false;

        return true;
    }

    static ContentsType CalcCurrentContentsType()
    {
        if (Global.LeftTime(GetUserWelcomeBackMission(ContentsType.WelcomeBack).endTs) > 0) //WelcomeBackMission 기간
        {
            return ContentsType.WelcomeBack;
        }
        else
        {
            if (GetUserWelcomeBackMission(ContentsType.LoginMore).startTs == 0)
            {
                return ContentsType.InvalidContents;
            }

            if (GetUserWelcomeBackMission(ContentsType.WelcomeBack).buyPass == 1) //RestartPass 구입.
            {
                if (Global.LeftTime(GetUserWelcomeBackMission(ContentsType.LoginMore).endTs) > 0)//LoginMore 기간
                {
                    return ContentsType.LoginMore;
                }
            }
        }
        return ContentsType.InvalidContents;
    }

    public static bool IsActiveEvent()
    {
        if (_instance == null)
            return false;

        if (ServerContents.WelcomeBackMission == null || ServerRepos.UserWelcomeBackMission == null)
        {
            return false;
        }
        var currentMission = GetUserWelcomeBackMission(_instance.contentsType);

        if (currentMission == null)
            return false;

        if (Global.LeftTime(currentMission.endTs) > 0)
            return true;

        return false;
    }
    
    public static void Init()
    {
        if (!CheckStartable()) return;

        if (_instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerWelcomeBackEvent>();

        if (_instance == null)
            return;

        _instance.contentsType = CalcCurrentContentsType();

        ManagerEvent.instance.RegisterEvent(_instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.WELCOME_BACK_EVENT;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnIconPhase()
    {
        if (IsActiveEvent())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.CBUPass_PremiumPass, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerWelcomeBackEvent>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(GetUserWelcomeBackMission()));
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

    public ManagerWelcomeBackEvent.ContentsType GetContentsType()
    {
        return contentsType;
    }

    //public static int GetWelcomeBackMissionIndex()
    //{
    //    return (int)contentsType - 1;
    //}

    //public static ServerUserWelcomeBackMission GetWelcomeBackUserMission()
    //{
    //    ServerUserWelcomeBackMission serverUserWelcomeBackMission = new ServerUserWelcomeBackMission();

    //    if(CheckStartable())
    //    {
    //        serverUserWelcomeBackMission = ServerRepos.UserWelcomeBackMission[GetWelcomeBackMissionIndex()];
    //    }

    //    return serverUserWelcomeBackMission;
    //}

    public static string GetResourceKey()
    {
        if (_instance == null)
            return "";

        string Key = string.Empty;

        if (_instance.GetContentsType() == ContentsType.WelcomeBack)
            Key = "rp";
        else
            Key = "pp";

        return Key;
    }

    public static List<WelcomeMissionRewardData> GetWelcomeMissionData()
    {
        List<WelcomeMissionRewardData> listMissionData = new List<WelcomeMissionRewardData>();
        var missionInfo = GetWelcomeBackMission();
        var missionUserData = GetUserWelcomeBackMission();

        var missionContent = missionInfo.missionContent;
        var nMissionReward = missionInfo.completeReward;
        var vMissionReward = missionInfo.completePassReward;
        var nGetReward = missionUserData.gainReward;
        var vGetReward = missionUserData.gainPassReward;

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

    public static bool IsMissionClear(int type, int value)
    {
        bool isMissionClear = false;

        switch (type)
        {
            case 1: //DayLoginCount
                if (GetUserWelcomeBackMission().loginCount >= value)
                    isMissionClear = true;
                else
                    isMissionClear = false;
                break;
            case 2: //NomarStageNewClear
                if (ServerRepos.User.stage - GetUserWelcomeBackMission().lastStage >= value)
                    isMissionClear = true;
                else
                    isMissionClear = false;
                break;
            default:
                var storedAch = GetUserWelcomeBackMission().achievementStored;
                for (int i = 0; i < storedAch.Length; ++i)
                {
                    if (type == storedAch[i].type)
                    {
                        var current = ServerRepos.GetAchievements((ServerRepos.AchievementType)type);
                        if ((int)(current - storedAch[i].value) >= value)
                            isMissionClear = true;
                        else 
                            isMissionClear = false;
                        break;
                    }
                }
                break;
        }

        return isMissionClear;
    }
}
