using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIPopupMailBox : UIPopupBase
{
    #region OfferWall

    [SerializeField] private GameObject mailboxOW;
    [SerializeField] private UIUrlTexture textureOWBanner;

    #endregion
    
    public static UIPopupMailBox _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;
    
    public UIPanel      scrollPanel;
    public GameObject   mailboxEmpty;
    public GameObject   mailboxLoading;
    public GameObject   mailboxScroll;
    public UILabel      mailBoxText;
    public UILabel      mailBoxWarning;
    public UILabel      mailBoxCountText;
    public UILabel      emptyText;
    public UITexture    emptyTexture;
    public UIPokoButton refreshButton;

    private List<MessageData> listMessageDatas = new List<MessageData>();

    private bool bCheckFriendMessage = false;
    private int tabMailCnt = 0;

    public bool allowEndPopup = true;

    public static TapType tapType = TapType.SystemReward;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public void SetTabMailCount(int _tabMailCnt) { tabMailCnt += _tabMailCnt; }

    public enum TapType
    {
        All,
        SystemReward,
        FriendMail,
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();


        bool skip = false;
#if UNITY_IOS
        // ios 에서는 무조건 작은초대창을 띄우지 않게 수정되었음
        skip = true;
#endif

        if (allowEndPopup && !skip)
        {
            int day = System.DateTime.Now.DayOfYear;
            int openCount = PlayerPrefs.GetInt("OpenPopupInviteSmallCount", 0);
            if (openCount < 10 && PlayerPrefs.GetInt("OpenPopupInviteSmall", 0) != day)
            {
                openCount++;
                PlayerPrefs.SetInt("OpenPopupInviteSmall", day);
                PlayerPrefs.SetInt("OpenPopupInviteSmallCount", openCount);
                ManagerUI._instance.OpenPopupInviteSmall();
            }
        }

        textureOWBanner.mainTexture = null;
    }
    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
        InitMailBox();

        tapType = TapType.SystemReward;
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

    public void SetMailBox()
    {
        MessageData.SetUserData();
        listMessageDatas = ManagerData._instance._messageData;

        mailBoxCountText.text = string.Format("{0} / 100", tabMailCnt > 100 ? "100+" : tabMailCnt.ToString());

        // 스페셜 다이아샵이 있을 경우에 광고 정렬 순서를 바꾸는 작업.
        int mailShopCount = 0;
        MessageData tempMessageData = null;
        foreach(var messageData in listMessageDatas)
        {
            if (messageData.type == RewardType.specialDiaShop)
            {
                if(InActiveSpecialDia())
                {
                    listMessageDatas.Remove(messageData);
                    break;
                }
                else
                {
                    mailShopCount = 1;
                    tempMessageData = messageData;
                }
            }
        }

        // 스페셜 다이아샵 데이터가 최상단이 아닐 때 변경해주는 작업.
        if(tempMessageData != null)
        {
            listMessageDatas.Remove(tempMessageData);
            listMessageDatas.Insert(0, tempMessageData);
        }

        ADCheck(mailShopCount);
        
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.UpdateUI();
        }

        if (listMessageDatas.Count == 0)
        {
            mailboxEmpty.SetActive(true);
            mailboxScroll.SetActive(false);
            emptyText.text = Global._instance.GetString("p_e_11");
            emptyTexture.mainTexture = Box.LoadResource<Texture2D>("UI/mailbox_empty");
        }
        else
        {
            mailboxEmpty.SetActive(false);
            mailboxScroll.SetActive(true);
        }

        CheckRefreshButton();

        if (callbackDataComplete != null && listMessageDatas.Count > 0)
        {
            callbackDataComplete();
        }
    }

    private void ADCheck(int mailShopCount)
    {
        if (Global.clover == 0 && AdManager.ADCheck(AdManager.AdType.AD_2))
        {
            MessageData messageData = new MessageData()
            {
                adType = AdManager.AdType.AD_2,
                type = (RewardType)ServerContents.AdInfos[(int)AdManager.AdType.AD_2].rewards[0].type,
                value = ServerContents.AdInfos[(int)AdManager.AdType.AD_2].rewards[0].value
            };

            listMessageDatas.Insert(mailShopCount, messageData);
        }
        else if (AdManager.ADCheck(AdManager.AdType.AD_1))
        {
            MessageData messageData = new MessageData()
            {
                adType = AdManager.AdType.AD_1,
                type = RewardType.none
            };

            listMessageDatas.Insert(mailShopCount, messageData);
        }
    }

    public MessageData GetMessageData(int index)
    {
        if (listMessageDatas.Count <= index || listMessageDatas[index] == null)
            return null;
        return listMessageDatas[index];
    }

    void InitMailBox()
    {
        mailBoxText.text = Global._instance.GetString("p_msg_2");
        mailBoxWarning.text = Global._instance.GetString("p_msg_3");

        SetOfferWall();
    }

    private void SetOfferWall()
    {
        if (ManagerOfferwall.instance == null && ManagerOfferwallTapjoy.instance == null)
        {
            mailboxOW.gameObject.SetActive(false);
            mainSprite.transform.localPosition = new Vector3(3, 0, 0);
        }
        else
        {
            textureOWBanner.LoadCDN(Global.gameImageDirectory, "IconEvent/", "ow_banner.png");
        }
    }

    void OnClickReceiveAllMessage()
    {
        List<MessageData> messages = new List<MessageData>(ManagerData._instance._messageData);

        messages.RemoveAll((item) => item.adType != AdManager.AdType.None || item.type == RewardType.specialDiaShop);

        //터치 가능 조건 검사.
        if (UIPopupMailBox._instance.bCanTouch == false || messages.Count == 0) return;

        bCheckFriendMessage = CheckFriendsMessage();
        string text = "";
        if (bCheckFriendMessage == true)
            text = Global._instance.GetString("n_m_3");
        else
            text = Global._instance.GetString("n_m_6");

        ConfirmReceiveAllMessage();
    }

    private List<MessageData> listTempMessageDatas = new List<MessageData>();

    void ConfirmReceiveAllMessage()
    {
        //터치 가능 조건 검사.
        if (UIPopupMailBox._instance.bCanTouch == false
            || ManagerData._instance._messageData.Count == 0)
            return;

        //터치막음.
        UIPopupMailBox._instance.bCanTouch = false;

        SpringPanel.Begin(scrollPanel.gameObject, Vector3.one, 8f);

        long lastItemIndex = 0;
        listTempMessageDatas = new List<MessageData>();
        foreach (var item in listMessageDatas)
        {
            if (item.adType != AdManager.AdType.None || item.type == RewardType.specialDiaShop) continue;

            MessageData tempItem = new MessageData();
            tempItem.index = item.index;
            tempItem.type = item.type;
            tempItem.value = item.value;
            tempItem.textKey = item.textKey;
            tempItem.userKey = item.userKey;
            listTempMessageDatas.Add(tempItem);

            lastItemIndex = lastItemIndex > item.index ? lastItemIndex : item.index;
        }

        ServerAPI.ReceiveAllMail(lastItemIndex, recvMessageReceive, (int)tapType);
    }

    void recvMessageReceive(UserReceiveMailResp resp)
    {
        if (resp.IsSuccess)
        {
            var value = resp;

            //Debug.Log("** Message Receive ok index :" + resp.receiveIdx);

            //친구에게 라인메세지 전송.
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                bCheckFriendMessage = SendReplyToAllFriend(resp.receivedMails);
            }

            tabMailCnt = resp.tabMailCnt;
            UIPopupMailBox._instance.SetMailBox();

            if(resp.receivedMails.Count > 0)
            {
                List<MessageData> messageDatas = new List<MessageData>();

                messageDatas = SetMessageDatas(resp);
                if(messageDatas.Count > 0)
                {
                    ManagerUI._instance.OpenPopup<UIPopupMessageAllReceive>((popup) => popup.InitData(messageDatas));
                }
            }
            else
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_m_11"), false);
            }

            Global.star = (int)GameData.User.Star;
            Global.clover = (int)(GameData.User.AllClover);
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            Global.wing = (int)(GameData.User.AllWing);
            Global.exp = (int)(GameData.User.expBall);

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            UIDiaryController._instance.UpdateProgressHousingData();

            SpringPanel.Begin(scrollPanel.gameObject, Vector3.one, 8f);

            #region 그로씨         
            foreach (var item in listTempMessageDatas)
            {
                if (resp.receivedMails != null && resp.receivedMails.Exists(x => x == item.index) == false)
                    continue;

                bool isSendGrowthy = true;

                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_OPERATOR_REWARD;
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX;

                if (item.userKey != null && item.IsSystemMessage())
                {
                    if (item.textKey == 1 || item.CheckSystemMailFlag("evt_login"))  // 로그인 출석보상
                    {
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ATTENDANCE_REWARD;
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_LOGIN;
                    }
                    else if (item.textKey == 5 || item.CheckSystemMailFlag("sysrew_invite")) // 친구초대 보상
                    {
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD;
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD;
                    }
                    else if (item.textKey == 13 || item.textKey == 15 || item.CheckSystemMailFlag("sysrew_allclear"))
                    {
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_FLOWER;
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_QUEST_REWARD;
                    }
                    else if (item.textKey == 14)
                    {
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_RANKING_REWARD;
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_RANKING_REWARD;
                    }
                    else if (item.textKey == 22 || item.CheckSystemMailFlag("sysrew_adv")) //탐험모드 보상키일때 수정
                    {
                        if (item.type == RewardType.boxSmall || item.type == RewardType.boxMiddle || item.type == RewardType.boxBig)
                        {
                            isSendGrowthy = false;
                        }
                    }
                    else if (item.textKey == 31 || item.CheckSystemMailFlag("evt_mole")) //두더지모드 보상 - 팝업에서 그로시 전부 보냄
                    {
                        isSendGrowthy = false;
                    }
                    else if (item.textKey == 45 || item.CheckSystemMailFlag("evt_adv")) //이벤트 탐험모드 보상
                    {
                        isSendGrowthy = false;
                    }
                    
                    else if (item.textKey == 54 || item.textKey == 56 || item.textKey == 57 || item.CheckSystemMailFlag("evt_worldrank"))
                    {
                        isSendGrowthy = false; // 월드랭킹 보상은 보상시점에 그로시 남김
                    }
                    else if (item.textKey == 66 || item.CheckSystemMailFlag("evt_pokoflower")) // 친구에 의해 온 에코피 보상 공유 아이템
                    {
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_POKOFLOWER_SHARE;
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_POKOFLOWER_SHARE;
                    }
                    else if (item.CheckSystemMailFlag("sysrew_cbu")) // cbu 보상 수령
                    {
                        rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_CBU_SUPPLY;
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_CBU_SUPPLY;
                    }
                    else if (item.type == RewardType.housing)
                    {
                        isSendGrowthy = false;
                    }
                    else if (item.CheckSystemMailFlag("/offerwall/reward") ||
                             item.CheckSystemMailFlag("/offerwall_tapjoy/reward")) // 오퍼월 보상 수령
                    {
                        mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_OFFERWALL_REWARD;
                    }
                    else if (
                        item.CheckSystemMailFlag("evt_eventstage") ||
                        item.CheckSystemMailFlag("sysrew_pkg") ||
                        item.CheckSystemMailFlag("sysrew_xchganimalticket") ||
                        item.CheckSystemMailFlag("sysrew_quest") ||
                        item.CheckSystemMailFlag("sysrew_epflower") ||
                        item.CheckSystemMailFlag("sysrew_chapmission") ||
                        item.CheckSystemMailFlag("sysrew_ad") ||
                        item.CheckSystemMailFlag("sysrew_wrshop") ||
                        item.CheckSystemMailFlag("evt_alphabet") ||
                        item.CheckSystemMailFlag("evt_pokoflower") ||
                        item.CheckSystemMailFlag("evt_stagerank") || 
                        item.CheckSystemMailFlag("evt_turn_relay") ||
                        item.CheckSystemMailFlag("evt_capsuleGacha") ||
                        item.CheckSystemMailFlag("evt_welcomeMission") ||
                        item.CheckSystemMailFlag("evt_welcomeBackMission") ||
                        item.CheckSystemMailFlag("evt_renewal_login_bonus_reward") ||
                        item.CheckSystemMailFlag("decocollection/reward") ||
                        item.CheckSystemMailFlag("sysrew_ecshop") ||
                        item.CheckSystemMailFlag("sysrew_treasure_hunt") ||
                        item.CheckSystemMailFlag("evt_pass") ||  // 패스 상품 : 탐험(evt_pass_adventure), 권리형(evt_pass_premium)
                        item.CheckSystemMailFlag("evt_single_round") ||  // 수문장 크루그 보상
                        item.CheckSystemMailFlag("evt_criminal_clear_reward") ||
                        item.CheckSystemMailFlag("evt_criminal_all_clear_reward") ||
                        item.CheckSystemMailFlag("lucky_roulette") ||
                        item.CheckSystemMailFlag("evt_group_ranking") ||
                        item.CheckSystemMailFlag("space_travel_event") ||
                        item.CheckSystemMailFlag("evt_atelier")
                        )
                    {
                        isSendGrowthy = false;
                    }
                    else if (item.CheckSystemMailFlag("logged"))    // 차후 어떤 이유로든지간에 로그가 남았을 때
                    {
                        isSendGrowthy = false;
                    }

                    if (item.type == RewardType.gachaTicket)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.gachaTicket,
                            item.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_USE_GACHA,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                            null
                            );

                        var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM(
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                            "Animal_" + resp.userAdvAnimal.animalId,
                            "Animal",
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA_TICKET,
                            $"Gacha_{item.value}"
                            );
                        var DocTicket = JsonConvert.SerializeObject(rewardTicket);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);

                        isSendGrowthy = false;
                    }

                }
                else if (item.userKey != null && SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
                {
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD;
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD;
                    
                    {
                        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
                        var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                                            (
                                               myProfile.stage.ToString(),
                                               ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.RECEIVE_CLOVAR,
                                               user._userKey
                                            );
                        var doc = JsonConvert.SerializeObject(inviteFriend);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
                    }

                    if (item.type == RewardType.gachaTicket)
                    {
                        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                            (int)RewardType.gachaTicket,
                            item.value,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_USE_GACHA,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                            null
                            );

                        var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM(
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                            "Animal_" + resp.userAdvAnimal.animalId,
                            "Animal",
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA_TICKET,
                            $"Gacha_{item.value}"
                            );
                        var DocTicket = JsonConvert.SerializeObject(rewardTicket);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);

                        isSendGrowthy = false;
                    }
                }

                if( isSendGrowthy)
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)item.type,
                    item.value,
                    mrsnType,
                    rsnType,
                    null
                    );
                }
                
            }
            #endregion 그로씨
        }
        OnTouch();
    }

    List<MessageData> SetMessageDatas(UserReceiveMailResp resp)
    {
        List<MessageData> messageTempDatas = new List<MessageData>();

        foreach (var item in listTempMessageDatas)
        {
            if (resp.receivedMails != null && resp.receivedMails.Exists(x => x == item.index) == false)
                continue;
            if (item.type == RewardType.none)
                continue;

            messageTempDatas.Add(item);
        }

        List<MessageData> messageDatas = new List<MessageData>();


        foreach (var item in messageTempDatas)
        {
            if (messageDatas.Exists(x => x.type == item.type) == true)
            {
                messageDatas.Find(x => x.type == item.type).value += item.value;
            }
            else
            {
                messageDatas.Add(item);
            }
        }

        return messageDatas;
    }

    void OnTouch()
    {
        //터치 가능.
        UIPopupMailBox._instance.bCanTouch = true;
    }

    bool SendReplyToAllFriend(List<long> receivedMails)
    {
        //Trident.StringList providerKey = new Trident.StringList();
        List<string> userkeyList = new List<string>();
        for (int i = 0; i < listMessageDatas.Count; i++)
        {
            MessageData mData = listMessageDatas[i];
            if (receivedMails != null && receivedMails.Exists(x => x == mData.index) == false)
            {
                continue;
            }

            //시스템 메세지인 경우 반환(또는 광고인 경우)
            if (mData.IsSystemMessage() || mData.adType != AdManager.AdType.None || mData.type == RewardType.specialDiaShop)
            {
                continue;
            }

            //친구가 보낸 메세지일 경우, 클로버 답장.
            if (SDKGameProfileManager._instance.TryGetPlayingFriend(mData.userKey, out UserFriend uData))
            {
                //친구가 보낸 메세지 중 클로버(RewardType.clover)이거나 클로버 요청(RewardType.none)일 때 라인 메세지 보내지 않도록 처리
                if (mData.type != RewardType.clover && mData.type != RewardType.none)
                {
                    continue;
                }
                
                bool bRequest = mData.type == RewardType.none && mData.value == 0;
                
                //친구가 클로버를 보낸 타입인데, 요청 타입이 아니고 쿨타임 다 안됐으면 라인메세지 안보냄.
                if (bRequest == false && mData.type == RewardType.clover && mData.value > 0 && Global.LeftTime(uData.CloverCoolTime) > 0)
                {
                    continue;
                }

                if (bRequest == false)
                {
                    if (uData.CloverCoolTime == 0 || Global.LeftTime(uData.CloverCoolTime) <= 0)
                    {
                        Global._instance.UpdateCloverCoolTime(uData._userKey);
                    }
                }

                if (uData.GetTridentProfile() != null)
                {
                    userkeyList.Add(uData._userKey);
                }
            }
        }

        var lineTemplateId = UIItemSendClover.GetCloverSendLineTemplateId();
        ManagerData.SendLineMessage(userkeyList, lineTemplateId);

        return userkeyList.Count > 0;
    }

    bool CheckFriendsMessage()
    {
        for (int i = 0; i < listMessageDatas.Count; i++)
        {
            MessageData mData = listMessageDatas[i];

            if (mData.adType != AdManager.AdType.None || mData.type == RewardType.specialDiaShop) continue;

            //시스템에서 받은 메세지가 아니고 친구에게서 온 메세지가 하나라도 있는 경우, true 반환.
            if (mData.IsFriendMessage() &&
               ((mData.type == RewardType.none && mData.value == 0) || mData.type == RewardType.clover))
            {
                return true;
                
            }
        }
        //리스트 다 검사했을 때 친구에게 온 메세지가 없으면 false 반환.
        return false;
    }

    bool refreshButtonActive = false;

    void OnClickRefresh()
    {
        if (bCanTouch == false || refreshButtonActive == false)
        {
            SetRefreshButtonState(false);
            return;
        }

        refreshButtonActive = false;

        bCanTouch = false;
        ServerAPI.Inbox(OnRefreshReceived, 0, (int)tapType);
    }
    public void OnRefreshReceived(UserInboxResp resp)
    {
        NetworkLoading.MakeNetworkLoading(1f);

        if (resp.IsSuccess)
        {
            tabMailCnt = resp.tabMailCnt;

            NetworkLoading.EndNetworkLoading();

            SetMailBox();

            scrollPanel.GetComponent<UIScrollView>().ResetPosition();
            scrollPanel.GetComponentInChildren<UIReuseGridBase>().SetContent();
        }
        else
        {
            NetworkLoading.EndNetworkLoading();
        }

        OnTouch();
    }

    void CheckRefreshButton()
    {
        var messages = listMessageDatas.FindAll((message) => message.adType == AdManager.AdType.None);

        bool active = false;
        if (tabMailCnt > 100 && messages.Count < 100)
            active = true;

        if (tabMailCnt <= 100 && messages.Count != tabMailCnt)
            active = true;

        SetRefreshButtonState(active);
    }

    public void SetRefreshButtonState(bool active)
    {
        refreshButtonActive = active;
        var sprList = refreshButton.GetComponentsInChildren<UISprite>();
        for (var i = 0; i < sprList.Length; ++i)
        {
            sprList[i].alpha = active ? 1.0f : 0.5f;
        }

    }

    public void TapChangeEvent(int _tapType)
    {
        if (tapType == (TapType)_tapType) return;

        listMessageDatas.Clear();

        tapType = (TapType)_tapType;

        ServerAPI.Inbox(OnRefreshReceived, 0, _tapType);
    }

    public bool InActiveSpecialDia()
    {
        if (ServerRepos.UserMailShop == null) return true;

        int buyCount = 0;
        foreach (var IsBuySpecialItem in ServerRepos.UserMailShop.purchaseStatus)
        {
            if (IsBuySpecialItem == 1)
                buyCount++;
        }
        return ServerRepos.UserMailShop.purchaseStatus.Count == buyCount;
    }

    private void OnClickBtnOW()
    {
        bCanTouch = false;

        if (ManagerOfferwall.instance != null)
        {
            ManagerOfferwall.instance.ShowOW(() => { bCanTouch = true; });
        }
        else if (ManagerOfferwallTapjoy.instance != null)
        {
            if(ManagerOfferwallTapjoy.IsOfferWallReady())
            {
                ManagerOfferwallTapjoy.instance.ShowOfferWall_Tapjoy(() => { bCanTouch = true; });
            }
            else
            {
                ManagerOfferwallTapjoy.instance.ReConnectOfferWall(() =>
                {
                    var popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
                    popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_48"), false);
                
                    bCanTouch = true; 
                });
            }
        }
    }
}
