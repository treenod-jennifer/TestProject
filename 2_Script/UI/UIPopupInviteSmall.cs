using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopupInviteSmall : UIPopupSmallBase
{
    public static UIPopupInviteSmall _instance = null;

    //public List<UIItemInviteSmall> itemList = new List<UIItemInviteSmall>();

    public UISprite[]   rewardIcon;
    public UILabel[]    cloverCount;
    public UILabel[]    coinCount;
    public UILabel      inviteCount;

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
    }

    public override void OnClickBtnItem(int index)
    {
        base.OnClickBtnItem(index);
        UpdateInviteCount();
    }

    void MakeFriend()
    {
        /*
        List<UserData> invitePossible = new List<UserData>();
        {
            //친구정보 생성
            for (int i = 0; i < 15; i++)
            {   
                UserData fp = new UserData();
                fp._profile = new ServiceSDK.UserProfileData();
                fp._profile.name = "TOMATO" + i;
                fp._profile.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
                fp._profile.scoreData = new ServiceSDK.Score();
                fp._profile.scoreData.SetInitScoreData(fp._profile);
                //초대가능한 친구.
                invitePossible.Add(fp);
            }
        }
        */
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

        //InitPopUp(invitePossible);
    }

    protected override void SetItem()
    {/*
        for (int i = 0; i < itemList.Count; i++)
        {
           // itemList[i].SettingItem(friends[i]._profile.pictureUrl, friends[i]._profile.name);
        }*/
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
        string cloverText = inviteCnt.ToString();
        string coinText = (inviteCnt * 50).ToString();
        cloverCount[0].text = cloverText;
        cloverCount[1].text = cloverText;
        coinCount[0].text = coinText;
        coinCount[1].text = coinText;
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

    }

    void OnTouch()
    {
        //터치 가능.
        this.bCanTouch = true;
    }
    

    void InviteSuccess()
    {
        ManagerUI._instance.ClosePopUpUI();
    }

    void UpdateUI()
    {

        ManagerUI._instance.UpdateUI();
    }  

    void SendInvitetLineMessage()
    {   /*
        Trident.StringList providerKey = new Trident.StringList();
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                continue;
            }
            friends[i].inviteTime = Global.GetTime() + (60 * 60 * 24);
            providerKey.Add(friends[i]._profile.providerKey);
        }
        */
        #region 예전 라인 메세지 전송코드
        
        // 게임서버와 통신 성공후 친구에게 라인 메세지 전달
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
           /* ServiceSDK.ServiceSDKManager lineMessageManager = ServiceSDK.ServiceSDKManager.instance;
            string message = string.Format("[LINE ポコパンタウン] {0}さんからLINE ポコパンタウンの招待が届いたよ！", ManagerData._instance.userData._profile.name);
            lineMessageManager.SendMessage(Trident.GraphEventType.GraphEventPresent, providerKey, lineMessageManager.GetMessageContent("lgpkv_invite_jp_1", ManagerData._instance.userData._profile.name, message), (bool isSucess) => {});*/
        }
        #endregion

        //string message = string.Format("[LINE ポコパンタウン] {0}さんからLINE ポコパンタウンの招待が届いたよ！", ManagerData._instance.userData._profile.name);
        //ManagerData.SendLineMessage(userkeyList, "lgpkv_invite_jp_1", message);
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
