using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopupRequestCloverSmall : UIPopupSmallBase
{
    public static UIPopupRequestCloverSmall _instance = null;

 
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
        List<UserData> requestPossible = new List<UserData>();
        {/*
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
                requestPossible.Add(fp);
            }*/
        }
        InitPopUp(requestPossible);
    }

    protected override void SetItem()
    {

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

    }

    void CancelRequestClover()
    {  
        //터치 가능.
        this.bCanTouch = true;
    }


    void RequestSuccess()
    {
        ManagerUI._instance.ClosePopUpUI();
    }

    void SendRequestLineMessage()
    {
        List<string> userkeyList = new List<string>();
        //Trident.StringList providerKey = new Trident.StringList();
        for (int i = 0; i < 6; i++)
        {
            if (check[i].activeInHierarchy == false)
            {
                continue;
            }
            friends[i].cloverRequestCoolTime = Global.GetTime() + (60 * 60 * 24);
           // userkeyList.Add(friends[i]._profile.userKey);
        }

        #region 예전 라인 메세지 전송코드
        /*
        // 게임서버와 통신 성공후 친구에게 라인 메세지 전달
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ServiceSDK.ServiceSDKManager lineMessageManager = ServiceSDK.ServiceSDKManager.instance;
            string message = string.Format("[LINE ポコパンタウン] {0}さんからクローバーが届いたよ！", ManagerData._instance.userData._profile.name);
            lineMessageManager.SendMessage(Trident.GraphEventType.GraphEventPresent, providerKey, lineMessageManager.GetMessageContent("lgpkv_send_clover_jp_1", ManagerData._instance.userData._profile.name, message), (bool isSucess) =>
            {
            });
        }*/
        #endregion

        //string message = string.Format("[LINE ポコパンタウン] {0}さんのクローバーがなくなっちゃったよ…今すぐクローバーを送ってあげよう！", ManagerData._instance.userData._profile.name);
       // ManagerData.SendLineMessage(userkeyList, "lgpkv_request_clover_jp_1", message);
    }

    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }
}
