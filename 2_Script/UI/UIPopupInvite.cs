using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;

public class InviteTimeComparer : IComparer<UserData>
{
    public int Compare(UserData a, UserData b)
    {
        //a, b 둘다 초대 x 인 상태 : 그대로.
        if (a.inviteTime <= 0 && b.inviteTime <= 0)
            return 0;
        //b 만 초대인 상태 : a 가 위(b는 아래로).
        else if (a.inviteTime <= 0 && b.inviteTime > 0)
            return -1;
        //a 만 초대인 상태 : a 가 아래(b는 위로).
        else if (a.inviteTime > 0 && b.inviteTime <= 0)
            return 1;
        //a, b 둘다 초대인 상태 : 그대로.
        else if (a.inviteTime > 0 && b.inviteTime > 0)
            return 0;
        else
            return 0;
    }
}

public class UIPopupInvite : UIPopupBase
{
    public static UIPopupInvite _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    public UIPanel scrollPanel;
    public UILabel emptyText;
    public UILabel invitePopupInfo;
    public UILabel[] inviteAllCount;
    public GameObject[] rewardCheck;
    public UIProgressBar inviteProgressBar;
    public UITexture boniImage;

    private float progressOffset = 0f;
    private int allCount = 50;
    private int inviteCount = 0;

    List<UserData> inviteFriendList = new List<UserData>();

    InviteTimeComparer inviteComparer = new InviteTimeComparer();

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
        boniImage.mainTexture = Resources.Load("UI/invite_icon") as Texture2D;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }
    /*
    public void InitPopUpInvite(List<ServerUserInvitedFriend> inviteFriends)
    {
        invitePopupInfo.text = Global._instance.GetString("p_iv_2");
        inviteCount = ServerRepos.TotalInviteCnt;   // 누적 초대수
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeInviteFriends();
        }
        else
        {
            inviteFriendList = ManagerData._instance._inviteFriendsData.Values.ToList();
        }

        if (inviteFriendList.Count > 0)
        {
            emptyText.gameObject.SetActive(false);
            inviteFriendList.Sort(inviteComparer);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_1");
        }

        //초대 수, 프로그레스 바 세팅.
        progressOffset = 100f / allCount;
        inviteAllCount[0].text = inviteCount + "/" + allCount;
        inviteAllCount[1].text = inviteCount + "/" + allCount;
        inviteProgressBar.value = (inviteCount * progressOffset) * 0.01f;

        if (callbackDataComplete != null && inviteFriendList.Count > 0)
        {
            callbackDataComplete();
        }

        //체크버튼 세팅.
        SettingCheck();
    }
    */
    public int GetInviteAllCount()
    {
        return inviteCount;
    }

    void SettingCheck()
    {
        int checkNum = 0;
        if (inviteCount >= 50)
            checkNum = 3;
        else if(inviteCount >= 20)
            checkNum = 2;
        else if (inviteCount >= 10)
            checkNum = 1;

        for (int i = 0; i < checkNum; i++)
        {
            rewardCheck[i].SetActive(true);
        }
    }

    void MakeInviteFriends()
    {
        inviteFriendList.Clear();
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
        //            inviteFriendList.Add(fp);
        //        }
        //    }
        //}
        /*
        // 친구
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "제아(Jea)";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            inviteFriendList.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "Evie ㅇㅇㅈ";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            inviteFriendList.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "안준호 (Jude)";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            inviteFriendList.Add(fp);
        }*/
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "Ford";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    inviteFriendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "빅 vic";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    inviteFriendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "신소정(포코퍼즐 Meriel)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    inviteFriendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "에피 (ㄴㄱㅅㅇ)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    inviteFriendList.Add(fp);
        //}
    }

    public UserData GetInviteData(int index)
    {
        if (inviteFriendList.Count <= index || inviteFriendList[index] == null)
            return null;

        return inviteFriendList[index];
    }

    public int GetInviteFriendsCount()
    {
        return (inviteFriendList.Count - 1);
    }

    public void InviteFriends()
    {
        QuestGameData.SetUserData();

        inviteCount += 1;
        inviteAllCount[0].text = inviteCount + "/" + allCount;
        inviteAllCount[1].text = inviteCount + "/" + allCount;
        MoveProgressBar(0.2f);
        StartCoroutine(GetInviteCloverPopUp());
    }

    public void MoveProgressBar(float _mainDelay)
    {
        float targetValue = inviteProgressBar.value + (progressOffset * 0.01f);
        if (targetValue > 1)
        {
            targetValue = 1;
        }
        DOTween.To(() => inviteProgressBar.value, x => inviteProgressBar.value = x, targetValue, _mainDelay).SetEase(Ease.Flash);
    }

    public IEnumerator GetInviteCloverPopUp()
    {
        yield return new WaitForSeconds(0.2f);
        //초대해서 nn 보상을 받았다!
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/ok") as Texture2D;
        popupSystem.FunctionSetting(1, "GetInviteRewardPopUP", gameObject, true);
        popupSystem.SetButtonText(0, "確認");
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_iv_6"), false, texture);
        popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
    }

    public void GetInviteRewardPopUP()
    {
        if (inviteCount < 10 || inviteCount > 50)
            return;
        /*
        if (inviteCount == 10)
        {
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);
            //10명 보상을 받았다!
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            Texture2D texture = Resources.Load("Message/happy1") as Texture2D;
            popupSystem.SetButtonText(0, "確認");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_iv_3"), false, texture);
            popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
            rewardCheck[0].SetActive(true);
            
            //보상
            //그로씨
            var useReadyItem0 = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.GIFTBOX,
                    "GIFTBOX_boxSmall",
                    "GIFTBOX_boxSmall",                    
                    1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD,
                    "InVite_10"
                );
            var doc0 = JsonConvert.SerializeObject(useReadyItem0);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc0);


        }
        else if (inviteCount == 20)
        {
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);
            //20명 보상을 받았다!
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            Texture2D texture = Resources.Load("Message/happy1") as Texture2D;
            popupSystem.SetButtonText(0, "確認");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_iv_4"), false, texture);
            popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
            rewardCheck[1].SetActive(true);

            //보상
            //그로씨
            var useReadyItem0 = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    "CLOVER_30m",
                    "CLOVER_30m",                    
                    1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD,
                    "InVite_20"
                );
            var doc0 = JsonConvert.SerializeObject(useReadyItem0);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc0);

            //바로사용
            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    "CLOVER_30m",
                    "CLOVER_30m",
                    -1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
                );
            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);

        }
        else if (inviteCount == 50)
        {
            ManagerSound.AudioPlay(AudioInGame.CONTINUE);
            //50명 보상을 받았다!
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            Texture2D texture = Resources.Load("Message/happy2") as Texture2D;
            popupSystem.SetButtonText(0, "確認");
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_iv_5"), false, texture);
            popupSystem.SortOrderSetting(this.uiPanel.useSortingOrder);
            rewardCheck[2].SetActive(true);


            //보상
            //그로씨
            var useReadyItem0 = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,                    
                    "CLOVER_30m",
                    "CLOVER_30m",
                    1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD,
                    "InVite_50"
                );
            var doc0 = JsonConvert.SerializeObject(useReadyItem0);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc0);

            //바로사용
            var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                   ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    "CLOVER_30m",
                    "CLOVER_30m",
                    -1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
                );
            var doc1 = JsonConvert.SerializeObject(useReadyItem1);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);

            var GetClo = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD,
                0,
                10,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                "InviteReward"
                );
            var docClo = JsonConvert.SerializeObject(GetClo);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docClo);
        }*/
    }
}
