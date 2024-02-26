using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;
using System;

public class UIPopupInviteSmall : UIPopupSmallBase
{
    public static UIPopupInviteSmall _instance = null;

    public List<UIItemProfileSmall> itemList = new List<UIItemProfileSmall>();

    public List<GenericReward> rewardIcons;
    
    public UILabel      inviteCount;

    public List<GameObject> eventTags;

    public UILabel inviteEventText;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        //MakeFriend();
        UpdateInviteCount();
        string titleText = Global._instance.GetString("p_iv_1");
        title[0].text = titleText;
        title[1].text = titleText;
        message.text = Global._instance.GetString("p_ivs_1");

        for (int i = 0; i < eventTags.Count; ++i)
        {
            eventTags[i].SetActive(ServerRepos.LoginCdn.PerInviteRewardEvent == 1);
        }
        inviteEventText.text = Global._instance.GetString("p_ivs_2");
    }

    public override void OnClickBtnItem(int index)
    {
        base.OnClickBtnItem(index);
        UpdateInviteCount();
    }

    void MakeFriend()
    {
        List<UserFriend> invitePossible = new List<UserFriend>();
        {
            //친구정보 생성
            for (int i = 0; i < 15; i++)
            {
                UserFriend fp = new UserFriend();

                var p= new ServiceSDK.UserProfileData();
                p.name = "TOMATO" + i;
                p.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
                fp.SetTridentProfile(p);
                //초대가능한 친구.
                invitePossible.Add(fp);
            }
        }

        /*
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "정현천";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m0573a03972517cc149081cff808b0ce9347e3c54fe62/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            fp._profile.scoreData.SetInitScoreData(fp._profile);
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "Alice (Miju Lee)";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m049a36f772512f643e3393b600ec3139ec831dcf912c/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            fp._profile.scoreData.SetInitScoreData(fp._profile);
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "김재영";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m059fdf8672513cdafb2422af9fd7bf01a0a759ef4bce/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            fp._profile.scoreData.SetInitScoreData(fp._profile);
            fp._profile.scoreData.scoreValue = 1;
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "김경화 carrie";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05eb2c6f7251dbac9031b4f0af2d4e25be215435ff6c/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "박톰";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m056d63ba7251f5fbcc60fea90bfe786405bd2df0bb83/small";
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "Yuri Jeong";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05f1d4e57251ab0b0359aaec27e532761933e5ef9f02/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            invitePossible.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "SunJu Kim Doreen";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05ebef5d7251be88b93728327755bc7a2e614d1499e1/small";
            fp._profile.scoreData = new ServiceSDK.Score();
            invitePossible.Add(fp);
        }*/

        InitPopUp(invitePossible);
    }

    protected override void SetItem()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].SettingItem(friends[i], i, OnClickBtnItem);
        }
    }

    void UpdateInviteCount()
    {
        int inviteCnt = 0;
        //현재 초대 중인 수 셈.
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == true)
            {
                inviteCnt += 1;
            }
        }
        inviteCount.text = string.Format("{0}/{1}", inviteCnt, 6);

        for(int i = 0; i < rewardIcons.Count; ++i)
        {
            if( i < ServerRepos.LoginCdn.PerInviteRewards.Count )
            {
                this.rewardIcons[i].SetReward(
                new Reward()
                {
                    type = ServerRepos.LoginCdn.PerInviteRewards[i].type,
                    value = ServerRepos.LoginCdn.PerInviteRewards[i].value * inviteCnt
                });
            }
            else
            {
                rewardIcons[i].gameObject.SetActive(false);
            }
            
        }
    }

    void OnClickBtnInvite()
    {
        //초대로 선택한 친구가 없으면 작동 안 함.
        if (activeButton.activeInHierarchy == true)
            return;

        //터치 가능 조건 검사.
        if (this.bCanTouch == false)
            return;
        //터치막음.
        this.bCanTouch = false;

        int inviteCount = 0;
        //현재 초대 중인 수 셈.
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == true)
            {
                inviteCount += 1;
            }
        }
        //초대하시겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmInvite", gameObject, false);
        popupSystem.FunctionSetting(2, "OnTouch", gameObject, true);
        popupSystem.FunctionSetting(3, "OnTouch", gameObject, true);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        string text = Global._instance.GetString("n_iv_7");
        text = text.Replace("[n]", inviteCount.ToString());
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
    }

    void ConfirmInvite()
    {
        MultiRequestFUserKey req = new MultiRequestFUserKey();
        req.fUserKeyList = new List<string>();

        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                continue;
            }
            if( friends[i].GetTridentProfile() != null )
            {
                req.fUserKeyList.Add(friends[i].GetTridentProfile().providerKey);
            }
            
        }
        ServerAPI.MultiInviteFriend(req, recvMultiInvite);
    }

    void OnTouch()
    {
        //터치 가능.
        this.bCanTouch = true;
    }

    void recvMultiInvite(MultiInviteFriendResp resp)
    {
        if (resp.IsSuccess)
        {
            //그로씨 갯수확인
            int getCoin = 0;
            int cloverCount = 0;

            for (int i = 0; i < 6; i++)
            {
                if (check[i].activeInHierarchy == true)
                {
                    getCoin += 50;
                    cloverCount++;

                    var myProfile = SDKGameProfileManager._instance.GetMyProfile();

                    var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                    (
                       myProfile.stage.ToString(),
                       ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SEND_INVITE,
                       friends[i].GetTridentProfile().providerKey
                    );
                    var doc = JsonConvert.SerializeObject(inviteFriend);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
                }
            }

            var playEndrecvMTC = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD,
                0,
                cloverCount,
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
                getCoin,
                (int)(GameData.User.coin),
                (int)(GameData.User.fcoin)
                );

            var docA = JsonConvert.SerializeObject(playEndrecvMT);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docA);

            for (int i = 0; i < 6; i++)
            {
                if (check[i].activeInHierarchy == false)
                {
                    continue;
                }
                friends[i].canInvite = false;
            }

            //친구를 초대했어! 팝업.
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            popupSystem.FunctionSetting(1, "InviteSuccess", gameObject, true);
            popupSystem.FunctionSetting(3, "InviteSuccess", gameObject, true);
            popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
            string text = Global._instance.GetString("n_iv_6");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
            popupSystem.SetResourceImage("Message/ok");

            //보상 받은거 UI 업데이트.
            UpdateUI();

            //다이어리 쪽 퀘스트 데이터 갱신.
            UIDiaryController._instance.UpdateQuestData(true);
        }
    }

    void InviteSuccess()
    {
        ManagerUI._instance.ClosePopUpUI();
    }

    void UpdateUI()
    {
        Global.coin = (int)(GameData.User.AllCoin);
        Global.clover = (int)(GameData.User.AllClover);
        ManagerUI._instance.UpdateUI();
    }
    
    protected override void OnClickBtnCheckAll()
    {
        base.OnClickBtnCheckAll();
        UpdateInviteCount();
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }
}
