using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemWelcomeMission : MonoBehaviour
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

    public void UpdateData(WelcomeMissionRewardData mData, int missionIndex, int type, int value, bool isPassBuy)
    {
        welcomeData = mData;
        this.missionIndex = missionIndex;
        this.isPassBuy = isPassBuy;

        if (ManagerWelcomeEvent.IsMissionClear(type, value))
        {
            missionState = MissionState.Complete;
        }
        else
        {
            missionState = MissionState.Lock;
        }

        var missionText = Global._instance.GetString($"p_wm_m_{type}");
        mLabelMissionText.text = missionText.Replace("[n]", $"{value}");

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
        ServerAPI.GetWelcomeMissionReward(missionIndex, 0, (resp)=> {resvWelcomeMissionReward(0, resp);});
    }

    public void OnClickGetVipPassReward()
    {
        if (isPassBuy == false) return;

        ServerAPI.GetWelcomeMissionReward(missionIndex, 1, (resp) => { resvWelcomeMissionReward(1, resp); });
    }

    void resvWelcomeMissionReward(int rewardType, GetWelcomeMissionRewardResp resp)
    {
        if(resp.IsSuccess)
        {
            var reward = rewardType == 0 ? ServerContents.WelcomeMission.completeReward[this.missionIndex- 1] 
                : ServerContents.WelcomeMission.completePassReward[this.missionIndex - 1];

            var rwTypeString = rewardType == 0 ? "A" : "B";
            //G_WELCOME_MISSION_REWARD
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    reward.type,
                    reward.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_WELCOME_MISSION_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_WELCOME_MISSION_REWARD,
                    $"WM_{ServerRepos.UserWelcomeMission.vsn}_{missionIndex}_{rwTypeString}"
                );

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE_MISSION,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WELCOME_MISSION,
                $"WM_{ServerRepos.UserWelcomeMission.vsn}_{missionIndex}_{rwTypeString}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

            bool allCleared = true;
            for(int i =0; i < resp.userWelcomeMission.gainReward.Length; ++i)
            {
                if(resp.userWelcomeMission.gainReward[i] == 0)
                {
                    allCleared = false;
                    break;
                }
            }

            for (int i = 0; i < resp.userWelcomeMission.gainPassReward.Length; ++i)
            {
                if (resp.userWelcomeMission.gainPassReward[i] == 0)
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
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WELCOME_MISSION,
                    $"WM_{ServerRepos.UserWelcomeMission.vsn}",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                    );
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", JsonConvert.SerializeObject(allClearAch));
            }

            welcomeData.nGetReward = resp.userWelcomeMission.gainReward[this.missionIndex - 1] > 0;
            welcomeData.vGetReward = resp.userWelcomeMission.gainPassReward[this.missionIndex - 1] > 0;

            SetReward();

            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.clearReward);

            Global.clover = (int)(GameData.User.AllClover);
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();

        }
    }
}