using System.Collections.Generic;
using UnityEngine;

public class UIItemAdventurePassReward : MonoBehaviour
{
    [Header("Reward")]
    [SerializeField] private List<GenericReward> rewards;
    [SerializeField] private GameObject getButton;
    [SerializeField] private GameObject completeStamp;
    [SerializeField] private GameObject objRoot;
    [SerializeField] private GameObject objLock;

    private int rewardIndex;
    private bool isPremiumReward;

    public void SetReward(int index, bool isPremiumReward)
    {
        rewardIndex = index;
        this.isPremiumReward = isPremiumReward;

        var rewards = ManagerAdventurePass.GetAdventurePassReward(index, isPremiumReward);

        if (rewards.Exists((item) => item.type == 0))
        {
            objRoot.SetActive(false);
            return;
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            this.rewards[i].gameObject.SetActive(true);
            this.rewards[i].SetReward(rewards[i]);
        }

        SetRewardState(ManagerAdventurePass.GetRewardState(index, isPremiumReward));
    }

    public void SetRewardState(UIItemAdventurePass.RewardState rewardState)
    {
        switch (rewardState)
        {
            case UIItemAdventurePass.RewardState.LOCK:
            {
                getButton.SetActive(false);
                completeStamp.SetActive(false);
                objLock.SetActive(true);
            }
                break;
            case UIItemAdventurePass.RewardState.OPEN_REWARD:
            {
                getButton.SetActive(true);
                completeStamp.SetActive(false);
                objLock.SetActive(false);
            }
                break;
            case UIItemAdventurePass.RewardState.GET_REWARD:
            {
                getButton.SetActive(false);
                completeStamp.SetActive(true);
                objLock.SetActive(false);
            }
                break;
        }
    }

    private void OnClickGetButton()
    {
        int rewardType = isPremiumReward ? 2 : 1;

        ServerAPI.AdventurePassReward(rewardType, rewardIndex,
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null,
                        resp.reward);
                    SetRewardState(ManagerAdventurePass.GetRewardState(rewardIndex, isPremiumReward));

                    var rewards = ManagerAdventurePass.GetAdventurePassReward(rewardIndex, isPremiumReward);
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                    (
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.ADVENTURE_PASS_REWARD,
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_ADVENTURE_PASS_REWARD,
                        $"ADVENTURE_PASS_REWARD_{rewardIndex + 1}_{(isPremiumReward ? "PREMIUM" : "NORMAL")}",
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                    var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);

                    for (int i = 0; i < rewards.Count; i++)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            rewards[i].type,
                            rewards[i].value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PASS_REWARD,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PASS_REWARD,
                            $"G_ADVENTURE_PASS_REWARD_{resp.userAdventurePass.vsn}_{(isPremiumReward ? "PREMIUM" : "NORMAL")}"
                        );
                    }

                    Global.star = (int) GameData.User.Star;
                    Global.clover = (int) (GameData.User.AllClover);
                    Global.coin = (int) (GameData.User.AllCoin);
                    Global.jewel = (int) (GameData.User.AllJewel);
                    Global.wing = (int) (GameData.User.AllWing);
                    Global.exp = (int) (GameData.User.expBall);

                    if (ManagerUI._instance != null)
                        ManagerUI._instance.UpdateUI();
                }
            });
    }
}