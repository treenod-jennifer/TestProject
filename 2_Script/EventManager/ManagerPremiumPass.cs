using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPremiumPass : MonoBehaviour, IEventBase
{
    public enum PassBuyState
    {
        NONE,
        PASS_DISABLED,
        PASS_ACTIVE,
    }

    public static ManagerPremiumPass _instance { get; private set; }

    public static int userMissionCount;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance != null)
            _instance = null;
    }

    public static PassBuyState passState;

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if (ServerContents.PremiumPass == null || ServerRepos.UserPremiumPass == null)
        {
            return false;
        }

        if (GetPremiumPassData().type == 0)
        {
            return false;
        }

        var preimumPassData = GetPremiumPassData();

        if(preimumPassData == null)
        {
            return false;
        }

        return Global.LeftTime(preimumPassData.endTs) > 0;
    }

    public static int GetMissionCount()
    {
        return GetPremiumPassData().targetCount.Count;
    }

    public static CdnPremiumPass GetPremiumPassData()
    {
        return ServerContents.PremiumPass;
    }

    public static int GetPremiumPassResourceIndex()
    {
        return ServerContents.PremiumPass.resourceIndex;
    }

    public static string GetPassType(bool IsUpper = true)
    {
        string value;

        if (ManagerPremiumPass.GetPremiumPassResourceIndex() > 2)
        {
            value = $"{ManagerPremiumPass.GetPremiumPassResourceIndex()}";
        }
        else
        {
            if (IsUpper)
                value = ManagerPremiumPass.GetPremiumPassResourceIndex() == 1 ? "NPU" : "PU";
            else
                value = ManagerPremiumPass.GetPremiumPassResourceIndex() == 1 ? "npu" : "pu";
        }

        return value;
    }

    public static void OpenPopupGetReward(string messagekey)
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(messagekey), false);
        popup.SortOrderSetting();
    }

    public static List<Reward> GetPremiumPassReward(int index, bool isPremiumReward)
    {
        List<Reward> rewards = new List<Reward>();

        if(isPremiumReward)
        {
            rewards.AddRange(GetPremiumPassData().premiumReward[index]);
        }
        else
        {
            rewards.Add(GetPremiumPassData().reward[index]);
        }

        return rewards;
    }

    public static bool IsMissionActive(int index)
    {
        return ServerRepos.UserPremiumPass.targetCount >= GetPremiumPassData().targetCount[index];
    }

    public static UIItemPremiumPass.RewardState GetRewardState(int index, bool isPremiumReward)
    {
        int targetCount = ServerRepos.UserPremiumPass.targetCount;

        List<int> reward_json = isPremiumReward ? ServerRepos.UserPremiumPass.premiumRewardState : ServerRepos.UserPremiumPass.rewardState;

        if (reward_json[index] == 1)
            return UIItemPremiumPass.RewardState.GET_REWARD;
        else
        {
            if (targetCount >= GetPremiumPassData().targetCount[index])
                return UIItemPremiumPass.RewardState.OPEN_REWARD;
            else
                return UIItemPremiumPass.RewardState.LOCK;
        }
    }

    public static int IsGetRewardState()
    {
        var nRewardState = ServerRepos.UserPremiumPass.rewardState;
        var pRewardState = ServerRepos.UserPremiumPass.premiumRewardState;

        int Count = 0;

        for (int i = 0; i < ServerRepos.UserPremiumPass.missionProgress; i++)
        {
            if (nRewardState[i] == 0)
                Count++;

            if(ServerRepos.UserPremiumPass.premiumState > 1)
            {
                if (pRewardState[i] == 0)
                    Count++;
            }
        }

        return Count;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (_instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerPremiumPass>();

        if (_instance == null)
            return;

        userMissionCount = ServerRepos.UserPremiumPass.targetCount;
        passState = (ManagerPremiumPass.PassBuyState)ServerRepos.UserPremiumPass.premiumState;

        ManagerEvent.instance.RegisterEvent(_instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.PREMIUM_PASS;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnIconPhase()
    {
        if (CheckStartable())
        {
            SideLeftIcon.Maker.MakeIcon<SideLeftIcon.IconPremiumPass>(
                SidebarLeft: ManagerUI._instance.anchorTopLeft,
                init: (icon) => icon.Init(GetPremiumPassData()));
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
}
