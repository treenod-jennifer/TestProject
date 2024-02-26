using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerAdventurePass : MonoBehaviour, IEventBase
{
    public enum PassBuyState
    {
        NONE,           // 0 : 구매하지 않은 상태
        PASS_DISABLED,  // 1 : 구매했으나 재접속하지 않은 상태
        PASS_ACTIVE,    // 2 : 구매 후 재접속 완료한 상태
    }

    public static ManagerAdventurePass _instance { get; private set; }
    public static PassBuyState passState;
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
    
    public static bool CheckStartable()
    {
        if (!ManagerAdventure.CheckStartable())  // 탐험 모드 플레이 가능 시 오픈
            return false;

        if (ServerContents.AdventurePass == null || ServerRepos.UserAdventurePass == null)
            return false;

        return Global.GetTime() >= GetAdventurePassData().startTs && Global.LeftTime(GetAdventurePassData().endTs) > 0;
    }

    public static void Init()
    {
        if (!CheckStartable()) return;

        if (_instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerAdventurePass>();

        if (_instance == null)
            return;

        passState = (PassBuyState) ServerRepos.UserAdventurePass.premiumState;

        ManagerEvent.instance.RegisterEvent(_instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.ADVENTURE_PASS;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList,
        Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public void OnIconPhase()
    {
        if (CheckStartable())
        {
            SideLeftIcon.Maker.MakeIcon<SideLeftIcon.IconAdventurePass>(
                SidebarLeft: ManagerUI._instance.anchorTopLeft,
                init: (icon) => icon.Init(GetAdventurePassData()));
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

    public void OnLobbyStart() => userMissionCount = ServerRepos.UserAdventurePass.targetCount;

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

    #region 탐험 패스 전용 함수

    public static CdnAdventurePass GetAdventurePassData()
    {
        return ServerContents.AdventurePass;
    }

    public static List<Reward> GetAdventurePassReward(int index, bool isPremiumReward)
    {
        if (isPremiumReward)
            return GetAdventurePassData().premiumReward[index];
        else
            return new List<Reward>() { GetAdventurePassData().reward[index] };
    }

    public static bool IsMissionActive(int index)
    {
        return ServerRepos.UserAdventurePass.targetCount >= GetAdventurePassData().targetCount[index];
    }

    public static UIItemAdventurePass.RewardState GetRewardState(int index, bool isPremiumReward)
    {
        List<int> rewardState = isPremiumReward ? ServerRepos.UserAdventurePass.premiumRewardState : ServerRepos.UserAdventurePass.rewardState;

        if (rewardState[index] >= 1)
            return UIItemAdventurePass.RewardState.GET_REWARD;
        if (IsMissionActive(index))
            return UIItemAdventurePass.RewardState.OPEN_REWARD;
        return UIItemAdventurePass.RewardState.LOCK;
    }

    public static int GetMissionProgress()
    {
        int missionProgress = 0;

        foreach (var count in ServerContents.AdventurePass.targetCount)
        {
            if (count <= ServerRepos.UserAdventurePass.targetCount)
                missionProgress += 1;
            else
                break;
        }

        return missionProgress;
    }    

    public static void OpenPopupGetReward(string messagekey)
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString(messagekey), false);
        popup.SortOrderSetting();
    }

    #endregion
}