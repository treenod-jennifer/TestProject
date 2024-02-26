using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIPopupRequestCloverSmall : UIPopupSmallBase
{
    public static UIPopupRequestCloverSmall _instance = null;

    public List<UIItemProfileSmall> itemList = new List<UIItemProfileSmall>();

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

        string titleText = Global._instance.GetString("p_t_6");
        title[0].text = titleText;
        title[1].text = titleText;
        message.text = Global._instance.GetString("p_rq_1");
        //MakeFriend();
    }

    void MakeFriend()
    {
        List<UserFriend> requestPossible = new List<UserFriend>();
        {
            //친구정보 생성
            for (int i = 0; i < 15; i++)
            {
                UserFriend fp = new UserFriend();
                var p = new ServiceSDK.UserProfileData();
                p.name = "TOMATO" + i;
                p.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
                fp.SetTridentProfile(p);
                //초대가능한 친구.
                requestPossible.Add(fp);
            }
        }
        InitPopUp(requestPossible);
    }

    protected override void SetItem()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].SettingItem(friends[i], i, OnClickBtnItem);
        }
    }

    void OnClickBtnRequest()
    {
        //선택한 친구가 없으면 작동 안 함.
        if (activeButton.activeInHierarchy == true)
            return;

        //터치 가능 조건 검사.
        if (this.bCanTouch == false)
            return;
        //터치막음.
        this.bCanTouch = false;

        int requestCount = 0;
        //현재 요청중인 수 셈.
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == true)
            {
                requestCount += 1;
            }
        }
        //클로버를 요청하시겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmRequestClover", gameObject, false);
        popupSystem.FunctionSetting(2, "CancelRequestClover", gameObject, true);
        popupSystem.FunctionSetting(3, "CancelRequestClover", gameObject, true);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
        string text = Global._instance.GetString("n_rq_2");
        text = text.Replace("[n]", requestCount.ToString());
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_6"), text, false);
    }

    void ConfirmRequestClover()
    {
        MultiRequestFUserKey req = new MultiRequestFUserKey();
        req.fUserKeyList = new List<string>();

        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                continue;
            }
            req.fUserKeyList.Add(friends[i]._userKey);
        }
        ServerAPI.MultiRequestClover(req, recvMultiRequestClover);
    }

    void CancelRequestClover()
    {  
        //터치 가능.
        this.bCanTouch = true;
    }

    void recvMultiRequestClover(RequestedCloverListResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** Multi Request Clover Count :" + resp.cloverCoolTime.Count);
            //친구에게 라인메세지 전송.
            SendRequestLineMessage();

            //클로버를 요청했어! 팝업.
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.FunctionSetting(1, "RequestSuccess", gameObject, true);
            popupSystem.FunctionSetting(3, "RequestSuccess", gameObject, true);
            popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
            string text = Global._instance.GetString("n_rq_1");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_6"), text, false);
            popupSystem.SetResourceImage("Message/ok");
        }
    }

    void RequestSuccess()
    {
        ManagerUI._instance.ClosePopUpUI();
    }

    void SendRequestLineMessage()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        long cloverTime = ServerRepos.LoginCdn.sendCloverEventVer != 0 ? (60 * 60 * 12) : (60 * 60 * 24);

        List<string> userKeyList = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                continue;
            }

            friends[i].cloverRequestCoolTime = Global.GetTime() + cloverTime;
            userKeyList.Add(friends[i]._userKey);

            var tProfile = friends[i].GetTridentProfile();
            var cloverReqFriend = new ServiceSDK.GrowthyCustomLog_Social
            (
                myProfile.stage.ToString(),
                ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.REQUEST_CLOVAR,
                (tProfile != null && !string.IsNullOrEmpty(tProfile.providerKey)) ? tProfile.providerKey : friends[i]._userKey,
                tProfile != null ? "" : "GAMEFRIEND"
            );
            var doc = Newtonsoft.Json.JsonConvert.SerializeObject(cloverReqFriend);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
        }

        var lineTemplateId = UIPopupRequestClover.GetCloverRequestLineTemplateId();
        ManagerData.SendLineMessage(userKeyList, lineTemplateId);
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }
}
