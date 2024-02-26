using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginADBonusData
{
    public int day;
    public bool complete;
    public bool action;
    public List<Reward> rewards;
    public Protocol.AppliedRewardSet rewardSet;

    public LoginADBonusData(int day, bool complete, bool action, List<Reward> rewards, Protocol.AppliedRewardSet rewardSet)
    {
        this.day = day;
        this.complete = complete;
        this.action = action;
        this.rewards = rewards;
        this.rewardSet = rewardSet;
    }
}

public class ManagerLoginADBonus : MonoBehaviour, IEventBase
{
    public static ManagerLoginADBonus Instance { get; private set; }


    private List<int> listLoginADType = new List<int>();

    void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static bool CheckStartable(int type)
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if (ServerContents.LoginAdBonus == null || ServerContents.LoginAdBonus.Count == 0)
        {
            return false;
        }

        if(ServerRepos.UserLoginAdBonus == null || ServerRepos.UserLoginAdBonus.Count == 0)
        {
            return false;
        }

        if(ManagerLoginADBonus.Instance.IsUserLoginAdComplete(type))
        {
            return false;
        }

        return Global.LeftTime(ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).endTs) > 0;
    }
    
    bool IsUserLoginAdComplete(int type)
    {
        int index = ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).adRewardStatus.Length - 1;

        return ManagerLoginADBonus.Instance.GetUserLoginADBonus(type).adRewardStatus[index] == 1;
    }

    public static void Init()
    {
        if (Instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerLoginADBonus>();

        if (Instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(Instance);

        ManagerLoginADBonus.Instance.SetLoginADBonusType();
    }

    void SetLoginADBonusType()
    {
        if (ServerRepos.UserLoginAdBonus == null) return;

        foreach (var item in ServerRepos.UserLoginAdBonus)
        {
            listLoginADType.Add(item.type);
        }
    }

    public CdnLoginAdBonus GetLoginADBonus(int type)
    {
        return ServerContents.LoginAdBonus.Find((item) => item.type == type);
    }

    public ServerUserLoginAdBonus GetUserLoginADBonus(int type)
    {
        return ServerRepos.UserLoginAdBonus.Find((item) => item.type == type);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.LOGIN_AD_BONUS;
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

    public IEnumerator  OnRewardPhase()
    {
        if (ManagerLobby._firstLogin && Global._onlineMode)
        {
            // 이벤트가 존재하는지 먼저 체크
            if (ServerContents.LoginAdBonus != null && ServerContents.LoginAdBonus.Count > 0)
            { 
                NetworkLoading.MakeNetworkLoading(0.5f);
                bool loginWaitFlag = true;
                bool loginDataExist = false;

                Dictionary<int, Protocol.AppliedRewardSet> DicrewardSet = null;
                Protocol.AppliedRewardSet rewardSet = null;

                ServerAPI.LoginAdBonus((x) =>
                {
                    loginWaitFlag = false;
                    loginDataExist = x.loginAdBonus != null;
                    DicrewardSet = x.loginAdBonusReward;

                    if (DicrewardSet != null && loginDataExist)
                    {
                        foreach (var directApplied in x.loginAdBonusReward)
                        {
                            foreach (var reward in directApplied.Value.directApplied.Values)
                            {
                                //그로씨
                                {
                                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                        reward.type,
                                        reward.valueDelta,
                                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADTYPE_LOGIN_REWARD,
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADTYPE_LOGIN_REWARD,
                                        $"LOGIN_AD_REWARD_{x.loginAdBonus[0].type}_{x.loginAdBonus[0].loginCnt}"
                                    );
                                }
                            }
                        }
                    }
                });
                while (loginWaitFlag)
                {
                    yield return null;
                }

                NetworkLoading.EndNetworkLoading();

                if (loginDataExist && DicrewardSet != null)
                {
                    for (int i = 0; i < listLoginADType.Count; i++)
                    {
                        int type = listLoginADType[i];

                        if(DicrewardSet.TryGetValue(type, out rewardSet))
                        {
                            yield return ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupLoginADBonus>((popup) => popup.InitData(type, rewardSet));
                            yield return new WaitUntil(() => UIPopupLoginADBonus._instance == null);
                        }
                    }
                }
                
            }
        }
    }

    public void OnIconPhase()
    {
        for (int i = 0; i < listLoginADType.Count; i++)
        {
            int type = listLoginADType[i];

            if (CheckStartable(type))
            {
                SideIcon.Maker.MakeIcon<SideIcon.IconLoginADBonus>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(ManagerLoginADBonus.Instance.GetLoginADBonus(type)));
            }
        }
    }

    public void OnReboot()
    {
        if (Instance != null)
            Destroy(Instance);
    }

    public void OnEventIconClick(object data = null)
    {
        int type = data is int ? (int)data : 0;
        if (CheckStartable(type))
        {
            ManagerUI._instance.OpenPopup<UIPopupLoginADBonus>((popup) => popup.InitData(type));
        }
        else
        {
            UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_41"), false);
            systemPopup.SortOrderSetting();
        }
    }
}