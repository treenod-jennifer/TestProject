using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemWelcomeBackMission : MonoBehaviour
{
    enum MissionState
    {
        Complete,
        Lock,
    }

    [Header("MissionLink")]
    [SerializeField] private GameObject mObjectCheck;
    [SerializeField] private GameObject mObjectLock;
    [SerializeField] private UISprite[] mObjectSellect;
    [SerializeField] private UILabel mLabelMissionText;

    [Header("NomalRewardLink")]
    [SerializeField] private GenericReward nReward;
    [SerializeField] private BoxCollider nColRewardBtn;
    [SerializeField] private UISprite nSprRewardBtn;
    [SerializeField] private BoxCollider nColInfoBtn;

    [Header("VipRewardLink")]
    [SerializeField] private GenericReward vReward;
    [SerializeField] private BoxCollider vColRewardBtn;
    [SerializeField] private UISprite vSprRewardBtn;
    [SerializeField] private BoxCollider vColInfoBtn;

    private WelcomeMissionRewardData welcomeData = new WelcomeMissionRewardData();

    private MissionState mState;
    private int missionIndex;
    private bool isPassBuy = false;

    private MissionState missionState
    {
        get
        {
            return mState;
        }
        set
        {
            mState = value;

            switch (value)
            {
                case MissionState.Complete:
                    {
                        mObjectSellect[0].enabled = false;
                        mObjectSellect[1].enabled = false;

                        mObjectCheck.SetActive(true);
                        mObjectLock.SetActive(false);
                    }
                    break;
                case MissionState.Lock:
                    {
                        mObjectSellect[0].enabled = false;
                        mObjectSellect[1].enabled = false;

                        mObjectCheck.SetActive(false);
                        mObjectLock.SetActive(true);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void UpdateData(WelcomeMissionRewardData mData, int missionIndex, bool isMissionClear, bool isPassBuy)
    {
        welcomeData = mData;
        this.missionIndex = missionIndex;
        this.isPassBuy = isPassBuy;

        if (isMissionClear)
        {
            missionState = MissionState.Complete;
        }
        else
        {
            missionState = MissionState.Lock;
        }

        var storedAch = ManagerWelcomeBackEvent.GetUserWelcomeBackMission().achievementStored;
        string missionText = Global._instance.GetString($"p_rp_m_{mData.missionType}");

        for (int i = 0; i < storedAch.Length; i++)
        {
            if(mData.missionType == storedAch[i].type)
            {
                var current = ServerRepos.GetAchievements((ServerRepos.AchievementType)mData.missionType);

                var useClover = current - storedAch[i].value;

                if (useClover > mData.missionValue) useClover = mData.missionValue;

                missionText = missionText.Replace("[n]", $"{mData.missionValue}");
                missionText = missionText.Replace("[x]", $"{useClover}");
            }
        }

        mLabelMissionText.text = missionText;

        SetReward();
    }

    void SetReward()
    {
        Color32 nColor = new Color32(58, 94, 145, 255);
        Color32 btnColor = new Color32(34, 100, 14, 255);

        nReward.SetReward(welcomeData.nReward);
        nReward.EnableCheck(welcomeData.nGetReward);
        nReward.SetEffectTextColor(welcomeData.nGetReward == true ? nColor : btnColor);

        vReward.SetReward(welcomeData.vReward);
        vReward.EnableCheck(welcomeData.vGetReward);
        vReward.SetEffectTextColor(welcomeData.vGetReward == true ? nColor : btnColor);

        //버튼 기능
        if (missionState == MissionState.Complete)
        {
            nColRewardBtn.enabled = welcomeData.nGetReward == false;
            nSprRewardBtn.enabled = welcomeData.nGetReward == false;
            nColInfoBtn.enabled = welcomeData.nGetReward == true;

            vColRewardBtn.enabled = welcomeData.vGetReward == false;
            vSprRewardBtn.enabled = welcomeData.vGetReward == false;
            vColInfoBtn.enabled = welcomeData.vGetReward == true;
        }
        else
        {
            nColRewardBtn.enabled = false;
            nSprRewardBtn.enabled = false;
            nColInfoBtn.enabled = true;

            vColRewardBtn.enabled = false;
            vSprRewardBtn.enabled = false;
            vColInfoBtn.enabled = true;
        }
    }

    public void OnClickGetNomalReward()
    {
        ServerAPI.GetWelcomeBackMissionReward((int)ManagerWelcomeBackEvent._instance.GetContentsType(), missionIndex, 0, (resp) => { resvWelcomeBackMissionReward(0, resp); });
    }

    public void OnClickGetVipPassReward()
    {
        if (isPassBuy == false) return;

        ServerAPI.GetWelcomeBackMissionReward((int)ManagerWelcomeBackEvent._instance.GetContentsType(), missionIndex, 1, (resp) => { resvWelcomeBackMissionReward(1, resp); });
    }

    void resvWelcomeBackMissionReward(int rewardType, GetWelcomeBackMissionRewardResp resp)
    {
        if (resp.IsSuccess)
        {
            var reward = rewardType == 0 ? ManagerWelcomeBackEvent.GetWelcomeBackMission().completeReward[this.missionIndex - 1]
                : ManagerWelcomeBackEvent.GetWelcomeBackMission().completePassReward[this.missionIndex - 1];

            var rwTypeString = rewardType == 0 ? "A" : "B";
            var contentsTypeString = ManagerWelcomeBackEvent._instance.GetContentsType() == ManagerWelcomeBackEvent.ContentsType.WelcomeBack ? "WBM" : "LMM";

            ServerUserWelcomeBackMission retUserData = null;
            for (int i = 0; i < resp.userWelcomeBackMission.Length; ++i)
            {
                if (resp.userWelcomeBackMission[i].contentsType == (int)ManagerWelcomeBackEvent._instance.GetContentsType())
                {
                    retUserData = resp.userWelcomeBackMission[i];
                    break;
                }
            }
            if(retUserData != null)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type,
                    reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_WELCOME_BACK_MISSION_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_WELCOME_BACK_MISSION_REWARD,
                    $"{contentsTypeString}_{retUserData.vsn}_{missionIndex}_{rwTypeString}"
                );

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE_MISSION,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WELCOME_BACK_MISSION,
                    $"{contentsTypeString}_{retUserData.vsn}_{missionIndex}_{rwTypeString}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    num1: retUserData.cbuCount,
                    str1: (retUserData.loginCount - 1).ToString()
                );
                
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

                bool allCleared = true;
                for (int i = 0; i < retUserData.gainReward.Length; ++i)
                {
                    if (retUserData.gainReward[i] == 0)
                    {
                        allCleared = false;
                        break;
                    }
                }

                for (int i = 0; i < retUserData.gainPassReward.Length; ++i)
                {
                    if (retUserData.gainPassReward[i] == 0)
                    {
                        allCleared = false;
                        break;
                    }
                }
                if (allCleared)
                {
                    var allClearAch = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE_MISSION,
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WELCOME_BACK_MISSION,
                        $"{contentsTypeString}_{retUserData.vsn}",
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR,
                        num1: retUserData.cbuCount,
                        str1: (retUserData.loginCount - 1).ToString()
                        );
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", JsonConvert.SerializeObject(allClearAch));
                }

                welcomeData.nGetReward = retUserData.gainReward[this.missionIndex - 1] > 0;
                welcomeData.vGetReward = retUserData.gainPassReward[this.missionIndex - 1] > 0;
            }

            SetReward();

            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.clearReward);

            Global.clover = (int)(GameData.User.AllClover);
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();

        }
    }
}
