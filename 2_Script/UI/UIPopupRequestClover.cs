using Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPopupRequestClover : UIPopupBase
{
    public static UIPopupRequestClover _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    public UIPanel scrollPanel;
    public UILabel emptyText;
    public GameObject cloverRequestTimeEvent;
    bool sendBtnState = false;
    public UISprite sendBtnSprite;
    public UILabel sendBtnLabel;
    public UILabel selectedCountLabel;
    public UILabel overviewInfoLabel;

    public UIReuseGrid_RequestClover grid = null;

    public class ReqData
    {
        public UserFriend userData;
        public bool check = false;
    }

    //List<UserData> friendList = new List<UserData>();
    List<ReqData> friendList = new List<ReqData>();

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
        scrollPanel.depth = uiPanel.depth + 1;
        if (UIPopupReady._instance != null)
        {
            uiPanel.useSortingOrder = true;
            scrollPanel.useSortingOrder = true;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            scrollPanel.useSortingOrder = true;
            scrollPanel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUpRequestClover(int requestCloverCnt)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeFriends();
        }
        else
        {
            var orgFriendList = SDKGameProfileManager._instance.GetAllPlayingFriendList();
            orgFriendList.Sort(new GameStarComparer());

            for(int i = 0; i < orgFriendList.Count; ++i)
            {
                var f = orgFriendList[i] as UserFriend;
                this.friendList.Add(new ReqData() { userData = f });
            }
        }

        if (friendList.Count > 0)
        {
            emptyText.gameObject.SetActive(false);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_2");
        }
        InitCloverRequestEvent();

        if (callbackDataComplete != null && friendList.Count > 0)
        {
            callbackDataComplete();
        }

        OnCheckChanged();
    }

    private void InitCloverRequestEvent()
    {
        if (ServerRepos.LoginCdn.sendCloverEventVer != 0)
        {
            cloverRequestTimeEvent.SetActive(true);
        }
        else
        {
            cloverRequestTimeEvent.SetActive(false);
        }
    }

    void AddTestFriend(UserFriend fp)
    {
        friendList.Add(new ReqData() { userData = fp });
    }

    void MakeFriends()
    {
        friendList.Clear();
        //{
        //    int noCount = 1;
        //    //친구정보 생성
        //    for (int i = 1; i < 60; i++)
        //    {
        //        if (i % 2 == 0)
        //            continue;
        //        for (int d = 0; d < 3; d++)
        //        {
        //            UserData fp = new UserData();
        //            fp._profile = new ServiceSDK.UserProfileData();
        //            fp._profile.name = "OREO" + i;
        //            fp._profile.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
        //            friendList.Add(fp);
        //        }
        //    }
        //}

        // 친구
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.name = "제아(Jea)";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            AddTestFriend(fp);
        }
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.name = "Evie ㅇㅇㅈ";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            AddTestFriend(fp);
        }
        {
            UserFriend fp = new UserFriend();
            var p = new ServiceSDK.UserProfileData();
            p.name = "안준호 (Jude)";
            p.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            fp.SetTridentProfile(p);
            AddTestFriend(fp);
        }
        //{
        //    UserData fp = new UserData();
        //    var p = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "Ford";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "빅 vic";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "신소정(포코퍼즐 Meriel)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "에피 (ㄴㄱㅅㅇ)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
    }
    
    public ReqData GetFriendData(int index)
    {
        if (friendList.Count <= index || friendList[index] == null)
            return null;

        return friendList[index];
    }

    public int GetUserFriendsCount()
    {
        return (friendList.Count - 1);
    }

    void OnClickBtnRequest()
    {
        if (CheckExists() == false)
        {
            return;
        }

        this.bCanTouch = false;

        //클로버를 요청하시겠습니까? 팝업.

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmRequestClover", gameObject, false);
        popupSystem.FunctionSetting(2, "CancelRequestClover", gameObject, true);
        popupSystem.FunctionSetting(3, "CancelRequestClover", gameObject, true);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        string text = Global._instance.GetString("n_rq_2");
        text = text.Replace("[n]", CheckCount().ToString());
        
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_6"), text, false);
        popupSystem.SetResourceImage("Message/clover");
        if (UIPopupRequestClover._instance != null)
        {
            popupSystem.SortOrderSetting(UIPopupRequestClover._instance.uiPanel.useSortingOrder);
        }
    }

    bool CheckExists()
    {
        bool canSend = false;
        for (int i = 0; i < friendList.Count; ++i)
        {
            if (friendList[i].check)
            {
                canSend = true;
                break;
            }
        }
        return canSend;
    }

    public int CheckCount()
    {
        int count = 0;
        for (int i = 0; i < friendList.Count; ++i)
        {
            if (friendList[i].check)
            {
                ++count;
            }
        }
        return count;
    }

    public void OnCheckChanged()
    {
        SetSendButton(CheckExists());
        this.selectedCountLabel.text = string.Format("{0} / 10", CheckCount());
    }

    void SetSendButton(bool state)
    {
        if( state == false)
        {
            this.sendBtnSprite.color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
            this.sendBtnLabel.color = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
            sendBtnState = false;
        }
        else
        {
            this.sendBtnSprite.color = new Color(1f, 1f, 1f, 1f);
            this.sendBtnLabel.color = new Color(1f, 1f, 1f, 1f);
            sendBtnState = true;
        }
    }

    void ConfirmRequestClover()
    {
        MultiRequestFUserKey req = new MultiRequestFUserKey();
        req.fUserKeyList = new List<string>();


        for (int i = 0; i < friendList.Count; ++i)
        {
            if (friendList[i].check)
            {
                req.fUserKeyList.Add(friendList[i].userData._userKey);
            }
        }
        
        ServerAPI.MultiRequestClover(req, recvMultiRequestClover);
    }

    void CancelRequestClover()
    {
        //터치 가능.
        UIPopupRequestClover._instance.bCanTouch = true;
    }

    void RequestSuccess()
    {
        UIPopupRequestClover._instance.bCanTouch = true;
    }

    void recvMultiRequestClover(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            //친구에게 라인메세지 전송.
            SendRequestLineMessage();
            OnCheckChanged();

            this.grid.OnCloverSendComplete();

            //클로버를 요청했어! 팝업.
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.FunctionSetting(1, "RequestSuccess", gameObject, true);
            popupSystem.FunctionSetting(3, "RequestSuccess", gameObject, true);
            popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
            string text = Global._instance.GetString("n_rq_4");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_6"), text, false);
            popupSystem.SetResourceImage("Message/ok");
        }
        else
        {
            UIPopupRequestClover._instance.bCanTouch = true;
        }
    }

    void SendRequestLineMessage()
    {
        long cloverTime = ServerRepos.LoginCdn.sendCloverEventVer != 0 ? (60 * 60 * 12) : (60 * 60 * 24);

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        List<string> userKeyList = new List<string>();
        for (int i = 0; i < friendList.Count; ++i)
        {
            if (friendList[i].check)
            {
                friendList[i].check = false;
                friendList[i].userData.cloverRequestCoolTime = Global.GetTime() + cloverTime;

                var tProfile = friendList[i].userData.GetTridentProfile();
                if (tProfile != null)
                {
                    userKeyList.Add(friendList[i].userData._userKey);
                }

                var cloverReqFriend = new ServiceSDK.GrowthyCustomLog_Social
                (
                    myProfile.stage.ToString(),
                    ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.REQUEST_CLOVAR,
                    (tProfile != null && !string.IsNullOrEmpty(tProfile.providerKey)) ? tProfile.providerKey : friendList[i].userData._userKey,
                    tProfile != null ? "" : "GAMEFRIEND"
                );
                var doc = Newtonsoft.Json.JsonConvert.SerializeObject(cloverReqFriend);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
            }
        }

        var lineTemplateId = GetCloverRequestLineTemplateId();
        ManagerData.SendLineMessage(userKeyList, lineTemplateId);
    }

    internal static string GetCloverRequestLineTemplateId()
    {
        var lineTemplateId = "lgpkv_request_clover_jp_2";
        switch (LanguageUtility.SystemCountryCode)
        {
            case "tw":
                {
                    lineTemplateId = "lgpkv_request_clover_tw_2";
                    break;
                }
            case "jp":
                {
                    lineTemplateId = "lgpkv_request_clover_jp_2";
                    break;
                }
            default:
                {
                    lineTemplateId = "lgpkv_request_clover_jp_2";
                    break;
                }
        }
        return lineTemplateId;
    }
}
