using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemInvite : MonoBehaviour
{
    public UISprite inviteButton;
    public UILabel friendName;
    public UILabel[] inviteText;
    [SerializeField]
    UISprite recommendTag;

    [SerializeField] private UIItemProfile profileItem;

    UserFriend item;

    public List<GenericReward> rewardIcons;

    public void UpdateData(UserFriend CellData)
    {
        item = CellData as UserFriend;
        if (item == null || gameObject.activeInHierarchy == false)
            return;

        var tProfile = item.GetTridentProfile();
        if( tProfile == null )
            return;

        //프로필 아이템 추가
        profileItem.SetProfile(item);

        friendName.text = string.Format("{0}", Global.ClipString(tProfile.name, 10));

        if (tProfile.recommendOrderNo != 0)
        {
            recommendTag.gameObject.SetActive(true);
        }
        else
        {
            recommendTag.gameObject.SetActive(false);
        }

        if (item.canInvite == false)
        {
            inviteButton.color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
            inviteText[0].color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
        }
        else
        {
            inviteButton.color = new Color(1f, 1f, 1f, 1f);
            inviteText[0].color = new Color(1f, 1f, 1f, 1f);
        }

        for (int i = 0; i < rewardIcons.Count; ++i)
        {
            if (i < ServerRepos.LoginCdn.PerInviteRewards.Count)
            {
                if (rewardIcons[i].gameObject.activeInHierarchy == false)
                    rewardIcons[i].gameObject.SetActive(true);

                rewardIcons[i].SetReward(ServerRepos.LoginCdn.PerInviteRewards[i]);
            }
            else
            {
                rewardIcons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickBtnInvite()
    {
        if (UIPopupInvite._instance.bCanTouch == false
          || item.canInvite == false)
            return;

        //1000명 초대시.
        if (ServerRepos.TotalInviteCnt >= 1000)
        {
            OpenCantInvitePopup(true);
            return;
        }
        //일일 초대 수 50명 이상.
        if (ServerRepos.InviteDayCnt >= 50)
        {
            OpenCantInvitePopup(false);
            return;
        }

        //정말 초대하시겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmInviteFrined", gameObject, false);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        string text = Global._instance.GetString("n_iv_1");
        text = text.Replace("[u]", friendName.text);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
        popupSystem.SetResourceImage("Message/clover");
    }

    void OpenCantInvitePopup(bool bAll)
    {
        //bAll : true - 1000명 넘음. / false : 하루 50명 넘음.
        string text = "";
        if (bAll == false)
        {
            text = Global._instance.GetString("n_iv_8");
        }
        else
        {
            text = Global._instance.GetString("n_iv_9");
        }

        //초대할 수 있는 인원이 넘었어. 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
    }

    void ConfirmInviteFrined()
    {
        if (UIPopupInvite._instance.bCanTouch == false
        || item.canInvite == false)
            return;

        UIPopupInvite._instance.InviteFriend(this);
    }

    public string GetProviderKey()
    {
        return item.GetTridentProfile().providerKey;
    }

    public void SetInvitedUI()
    {
        item.canInvite = false;
        inviteButton.color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
        inviteText[0].color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
    }

    public void SendGrowthy()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        var tProfile = item.GetTridentProfile();

        //그로씨
        var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                            (
                               myProfile.stage.ToString(),
                               ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SEND_INVITE,
                               (tProfile?.providerKey ?? "")
                            );
        var doc = JsonConvert.SerializeObject(inviteFriend);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);

        var playEndrecvMTC = new ServiceSDK.GrowthyCustomLog_Money
            (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD,
            0,
            1,
            0,//(int)(GameData.User.clover),
            (int)(GameData.User.AllClover)//(int)(GameData.User.fclover)
            );
        var docC = JsonConvert.SerializeObject(playEndrecvMTC);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docC);

        var playEndrecvMT = new ServiceSDK.GrowthyCustomLog_Money
            (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD,
            0,
            50,
            (int)(GameData.User.coin),
            (int)(GameData.User.fcoin)
            );

        var docA = JsonConvert.SerializeObject(playEndrecvMT);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docA);
    }
}
