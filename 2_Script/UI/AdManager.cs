using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Treenod.Ads.AppLovin;
using System.Collections;
using System.Runtime.InteropServices; // 제거 X: 제거시 빌드에서 The type or namespace name `DllImport' 발생

public static class AdManager
{
    public enum AdType
    {
        /// <summary>광고 없음</summary>
        None = 0,

        /// <summary>우편함 광고 (무작위 아이템 획득)</summary>
        AD_1 = 1,

        /// <summary>우편함 광고 (클로버 획득)</summary>
        AD_2 = 2,

        /// <summary>선물상자 대기시간 감소</summary>
        AD_3 = 3,

        /// <summary>미션진행 대기시간 감소</summary>
        AD_4 = 4,

        /// <summary>턴 릴레이 AP 충전</summary>
        AD_5 = 5,

        /// <summary>클로버 상점 (클로버 획득)</summary>
        AD_6 = 6,

        /// <summary>날개 상점 (날개 획득)</summary>
        AD_7 = 7,

        /// <summary>재료 박스(데코 재료 획득)</summary>
        AD_8 = 8,

        /// <summary>스테이지 준비 팝업 게임시작 시, 턴 추가(PU)</summary>
        AD_9 = 9,

        /// <summary>스테이지 준비 팝업 게임시작 시, 턴 추가(NPU)</summary>
        AD_10 = 10,

        /// <summary>스테이지 준비 팝업 게임시작 시, 턴 추가(대기시간 후 광고 재시청 가능한 타입)(PU)</summary>
        AD_11 = 11,

        /// <summary>스테이지 준비 팝업 게임시작 시, 턴 추가(대기시간 후 광고 재시청 가능한 타입)(NPU)</summary>
        AD_12 = 12,

        /// <summary>광고보기형 상품</summary>
        AD_13 = 13,

        /// <summary>로그인 AD 보상 이벤트</summary>
        AD_14 = 14,
        
        /// <summary>다이아 상점 (무작위 아이템 획득)</summary>
        AD_15 = 15,
        
        /// <summary>빙고 이벤트 스페셜 보너스 아이템 획득</summary>
        AD_16 = 16,

        /// <summary>연속모드 신규 광고지면 (PU)</summary>
        AD_17 = 17,
        
        /// <summary>연속모드 신규 광고지면 (NPU)</summary>
        AD_18 = 18,
        
        /// <summary>월드 랭킹</summary>
        AD_19 = 19,
        
        /// <summary>로그인 이벤트 누적 보상 광고지면</summary>
        AD_20 = 20,
        
        /// <summary>NPU 전용 컨티뉴 광고지면</summary>
        AD_21 = 21,
        
        /// <summary>금붕어 잡기 재시작 광고지면</summary>
        AD_22 = 22,
    }

    private static Dictionary<AdType, string> placements = new Dictionary<AdType, string>()
    {
        {AdType.AD_1, "PostRandomBox" },
        {AdType.AD_2, "PostClover" },
        {AdType.AD_3, "PresentboxTimeReduce" },
        {AdType.AD_4, "TimemissionReduce" },
        {AdType.AD_5, "TurnrelayPlaycountCharge" },
        {AdType.AD_6, "ShopClover" },
        {AdType.AD_7, "ShopWing" },
        {AdType.AD_8, "LobbyRandomBox" },
        {AdType.AD_9, "Ready_PU" },
        {AdType.AD_10, "Ready_NPU" },
        {AdType.AD_11, "ReadyRewatch_PU" },
        {AdType.AD_12, "ReadyRewatch_NPU" },
        {AdType.AD_13, "PackageRandomBox" },
        {AdType.AD_14, "LoginBonusReward" },
        {AdType.AD_15, "ShopDiamond" },
        {AdType.AD_16, "BingoEventSpecialBonus" },
        {AdType.AD_17, "FailReset_PU" },
        {AdType.AD_18, "FailReset_NPU" },
        {AdType.AD_19, "WorldRankExchangeStation" },
        {AdType.AD_20, "LoginBonusRenewal" },
        {AdType.AD_21, "NPUContinue" },
        {AdType.AD_22, "TurnRelayRestart" },
    };

    public static string GetPlacementName(AdType adType)
    {
        if (placements.TryGetValue(adType, out string placement))
        {
            return placement;
        }
        else
        {
            return string.Empty;
        }
    }


    private static IAdSdk adSdk = null;

    /// <summary>
    /// 초기화 유무. 초기화에 실패하면 모든 기능 이 동작하지 않는다.
    /// </summary>
    private static bool isInit = false;

    /// <summary>
    /// 광고가 재생중인지 여부
    /// </summary>
    private static bool isADPlaying = false;

    /// <summary>
    /// 리워드 획득 여부
    /// </summary>
    private static bool isADRewarded = false;

    private static bool isBGMPlay = true;
    
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);
#endif
    
    public static void Initialize(string userKey)
    {
        //SDK 변경에 따라 sdk 변경
        adSdk = new AppLovinAdapter();
        adSdk.Initialize(userKey, OnInitialize);
        
        adSdk.SubscribeOnRewardedAdImpression(OnStartAd);
        adSdk.SubscribeOnRewardedVideoClose(OnCloseAd);
    }
    
    public static void OnReboot()
    {
        if (adSdk != null)
        {
            adSdk.CancelSubscriptionOnRewardedAdImpression(OnStartAd);
            adSdk.CancelSubscriptionOnRewardedVideoClose(OnCloseAd);
        }
    }
    
    #region 콜백 메소드
    //광고 초기화
    private static void OnInitialize()
    {
        RequestVideo();
#if UNITY_IOS && !UNITY_EDITOR
        FBAdSettingsBridgeSetAdvertiserTrackingEnabled(true);
#endif
        isInit = true;
    }
    
    //광고 시작할 때
    private static void OnStartAd(AdInfo _adInfo)
    {
        //초기화 안됨
        if (adSdk == null || isInit == false)
            return;

        if (Global._instance != null && Global._instance.showAdNetworkInfo)
        {
            NGUIDebug.Log("[AppLovinMax] " + _adInfo.networkName);
        }
        
        ADLog("광고 시작");
        isADPlaying = true;
        isADRewarded = false;
    }

    //광고 닫힐 때
    private static void OnCloseAd()
    {
        if (!isADPlaying)
            return;

        isADPlaying = false;

        ADLog("광고 종료");
    }

    //광고 정상 시청 완료 //리워드 획득
    private static void OnGetRewarded()
    {
        ADLog("광고 보상 획득");

        isADRewarded = true;
    }

    //광고 비정상 종료
    private static void OnFailedRewarded(string errorMessage)
    {
        ADLog("광고 비정상 종료");
        string errorText = $"[ErrorCode : No Error Message]\n"+Global._instance.GetString("n_s_64");

        if(errorMessage != null)
            errorText = $"[ErrorCode : " + errorMessage + "]\n" + Global._instance.GetString("n_s_64");

        ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
        {
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), errorText, false);
        });
    }
    #endregion

    //광고 영상 로드
    private static void RequestVideo()
    {
        adSdk.LoadRewardedAd(null);
    }

    //광고 송출
    private static void ShowAd(string placementName, Action<bool> closeAd, bool noAdPopupClose = true)
    {
        //초기화 안됨
        if (adSdk == null || isInit == false)
        {
            OpenPopupNoADs(noAdPopupClose, closeAd);
            return;
        }

        if(adSdk.IsRewardedAdReady())
        {
            //광고 영상 출력
            adSdk.ShowRewardedAd(placementName, OnGetRewarded, OnClosedAd);
        }
        else
        {
            //광고 영상 로드 안됨
            OpenPopupNoADs(noAdPopupClose, closeAd);

            //재로드
            RequestVideo();
        }

        //광고 종료
        void OnClosedAd(bool isRewarded, string errorMessage)
        {
            if (isRewarded == false
                || isADRewarded == false)
            {
                //비정상 종료
                closeAd(false);

                if(isRewarded == false)
                    //광고 비정상 종료 콜백 호출
                    OnFailedRewarded(errorMessage);
            }
            else
            {
                closeAd(isRewarded);
            }

            //광고 보상 리셋
            isADRewarded = false;

            //재로드
            RequestVideo();
        }
    }

    #region 각 타입별 광고
    /// <summary>
    /// 리워드 타입의 보상을 받을 수 있는 광고
    /// </summary>
    public static void ShowAD_RequestAdReward(AdType adType, Action<bool, Reward> complete = null)
    {
        if(adType != AdType.AD_1 && 
            adType != AdType.AD_2 &&
            adType != AdType.AD_6 &&
            adType != AdType.AD_7 &&
            adType != AdType.AD_8 &&
            adType != AdType.AD_13 &&
            adType != AdType.AD_15)
        {
            complete(false, null);
            return;
        }

        //광고 시작
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int)adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, true);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false, null);
                return;
            }

            {   //광고 정상 시청 후 리워드 획득 가능한 상태
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //광고 보상 획득 통신
            ManagerData._instance.StartCoroutine(CoGetADReward());
        }

        IEnumerator CoGetADReward()
        {
            //유저의 토큰값이 완전히 갱신된 이후, 리워드 획득 통신이 진행되도록 검사.
            //(광고는 창이 닫힐 때 백그라운드로 전환되기 때문에, 토큰값이 갱신된 후 서버 통신을 진행해야 함)
            yield return new WaitUntil(() => ManagerData._instance.isRefreshedToken == true);

            //리워드 획득
            ServerAPI.RequestAdReward
            (
                (int)adType,
                (a) =>
                {
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        a.reward.type,
                        a.reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AD_VIEW,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_AD_VIEW,
                        $"TYPE_{((int)adType).ToString()}_REWARD"
                        );

                    if (ServerContents.AdInfos[(int)adType].dailyLimit <= a.adInfo.usedCount)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                            $"TYPE_{((int)adType).ToString()}_DAILY_MAX",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                            );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }

                    //퀘스트 데이터 갱신
                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);
                    
                    ManagerUI._instance.SyncTopUIAssets();
                    OpenPopupGetReward(a.reward);

                    complete?.Invoke(true, a.reward);
                }
            );
        }
    }

    public static void ShowAD_ReduceGiftboxTime(int boxId, Action<bool> complete = null)
    {
        AdType adType = AdType.AD_3;

        //광고 시작
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int)adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {   //광고 정상 시청 후 리워드 획득 가능한 상태
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerUserGiftBox giftBoxData = UIPopupTimeGiftBox.instance.GiftBoxData;

            ServerAPI.ReduceGiftboxTime
            (
                boxId,
                (a) =>
                {
                    ObjectGiftbox.OpenBoxLog(true, giftBoxData.type);
                    var giftBox = ObjectGiftbox._giftboxList.Find((box) => box._data.index == boxId);
                    giftBox.OpenGiftBox();
                    UIPopupTimeGiftBox.instance?.ClosePopUp();

                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);

                    complete?.Invoke(true);
                },
                (b) =>
                {
                    ObjectGiftbox.OpenBoxLog(true, giftBoxData.type);
                    var giftBox = ObjectGiftbox._giftboxList.Find((box) => box._data.index == b.giftBox.index);
                    giftBox.ResetTime(b.giftBox.openTimer);
                    UIPopupTimeGiftBox.instance.InitPopUp(b.giftBox);

                    OpenPopupGetReward(ServerContents.AdInfos[(int)AdType.AD_3].useInterval);

                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);

                    complete?.Invoke(true);
                }
            );
        }
    }

    public static void ShowAD_ReduceMissionTime(int mission, Action<bool> complete = null)
    {
        AdType adType = AdType.AD_4;

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int)adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {   //광고 정상 시청 후 리워드 획득 가능한 상태
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerAPI.ReduceMissionTime
            (
                mission,
                (a) =>
                {
                    var missionItem = UIPopupDiary._instance.GetMissionItem(a.mission.idx);

                    long leftTime = Global.LeftTime(missionItem.mData.clearTime);
                    leftTime = leftTime < ServerContents.AdInfos[(int)AdType.AD_4].useInterval ? leftTime : ServerContents.AdInfos[(int)AdType.AD_4].useInterval;

                    OpenPopupGetReward(leftTime);

                    missionItem.mData.clearTime = a.mission.clearTime;

                    if (Global.LeftTime(missionItem.mData.clearTime) <= 0)
                    {
                        missionItem.MissionTimeEnd();
                    }
                    else
                    {
                        missionItem.InitBtnMission(missionItem.mData);
                    }

                    //퀘스트 데이터 갱신
                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);
                    
                    complete?.Invoke(true);
                }
            );
        }
    }

    public static void ShowAD_TurnRelayRechargeAPByAd(Action<bool> complete = null)
    {
        AdType adType = AdType.AD_5;

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int)adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {   //광고 정상 시청 후 리워드 획득 가능한 상태
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerAPI.TurnRelayRechargeAPByAd
            (
                (a) =>
                {
                    if (ServerContents.AdInfos[(int)adType].dailyLimit <= a.adInfo.usedCount)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                            $"TYPE_{((int)adType).ToString()}_DAILY_MAX",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                            );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }

                    ManagerTurnRelay.instance?.SyncFromServerUserData();
                    UIPopupTurnRelay_StageReady._instance?.RefreshButton();
                    UIPopupTurnRelay_APTime._instance?.ClosePopUp();

                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);

                    complete?.Invoke(true);
                }
            );
        }
    }

    public static void ShowAD_ReqAdReadyItem(AdType adType, Action<bool> complete = null)
    {
        if (adType != AdType.AD_9 && adType != AdType.AD_10 && adType != AdType.AD_11 && adType != AdType.AD_12)
        {
            complete(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int)adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {   // 일단 광고가 있었고, 보상을 받을 수 있는 상황
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerAPI.ReqAdReadyItem
            (
                (int)adType,
                (a) =>
                {
                    if ((ServerContents.AdInfos[(int)adType].rewatchSet == 0 ? ServerContents.AdInfos[(int)adType].dailyLimit
                    : ServerContents.AdInfos[(int)adType].dailyLimit * ServerContents.AdInfos[(int)adType].rewatchSet) <= a.userAdInfo.usedCount)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                            $"TYPE_{((int)adType).ToString()}_DAILY_MAX",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                            );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }

                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);

                    complete?.Invoke(true);
                }
            );
        }
    }

    public static void ShowAD_ReqAdLoginBonus(AdType adType, int type, int day, Action<bool> complete = null)
    {
        if (adType != AdType.AD_14)
        {
            complete(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm: $"TYPE_{((int) adType).ToString()}_START",
                arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                num1: 0,
                str1: $"TYPE_{type}_DAY_{day}"
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {
                // 일단 광고가 있었고, 보상을 받을 수 있는 상황
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int) adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    str1: $"TYPE_{type}_DAY_{day}"
                );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerAPI.ReqAdLoginBonus
            (
                type,
                (a) =>
                {
                    if (ServerContents.AdInfos[(int) adType].dailyLimit <= a.adInfo.usedCount)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                            tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                            anm: $"TYPE_{((int) adType).ToString()}_DAILY_MAX",
                            arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                            str1: $"TYPE_{type}_DAY_{day}"
                        );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }

                    //퀘스트 데이터 갱신
                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);
                    
                    ManagerUI._instance.SyncTopUIAssets();
                    OpenPopupGetReward(a.reward);

                    complete?.Invoke(true);
                }
            );
        }
    }
    
    public static void ShowAD_ReqAdBingoEvent(AdType adType, int slotIndex, Action<bool> complete = null)
    {
        if (adType != AdType.AD_16)
        {
            complete(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                tag : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm : $"TYPE_{((int)adType).ToString()}_START",
                arlt : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                num1 : 0
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            {   // 일단 광고가 있었고, 보상을 받을 수 있는 상황
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                    $"TYPE_{((int)adType).ToString()}_REWARD",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }

            //리워드 획득
            ServerAPI.BingoEventGetAdBonus
            (
                slotIndex,
                (a) =>
                {
                    ManagerBingoEvent.instance.SetCompleteEvent();
                    
                    if (ManagerBingoEvent.instance.CheckComplete() == false)
                    {
                        ManagerBingoEvent.instance.SyncFromServerUserData();   
                    }

                    {   //빙고이벤트 스페셜 보상 광고 시청은, 즉시 지급 보상에 대해서만 처리되어 우편함 보상은 그로시 로그를 남기지 않음.
                        if (a.clearReward.directApplied != null)
                        {
                            foreach (var item in a.clearReward.directApplied)
                            {
                                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                                (
                                    rewardType: item.Value.type,
                                    rewardCount: item.Value.valueDelta,
                                    moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BINGO_BONUS_REWARD,
                                    itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BINGO_BONUS_REWARD,
                                    QuestName: null
                                );
                            }
                        }
                    }

                    ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), () =>
                    {
                        UIPopupBingoEvent_Board._instance.SetBoard();
                    }, a.clearReward);
                
                    ManagerBingoEvent.instance.isSlotOpen = true;

                    if (ManagerUI._instance != null)
                        ManagerUI._instance.SyncTopUIAssets();

                    //퀘스트 데이터 갱신
                    QuestGameData.SetUserData();
                    UIDiaryController._instance.UpdateQuestData(true);
                    
                    complete?.Invoke(true);
                }
            );
        }
    }

    public static void ShowAD_ReqAdWorldRankExchangeStation(Action<bool, Reward> complete = null)
    {
        AdType adType = AdType.AD_19;

        //광고 시작
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_START",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, true);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false, null);
                return;
            }

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_REWARD",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

            //광고 보상 획득 통신
            ManagerData._instance.StartCoroutine(CoGetADReward());
        }

        IEnumerator CoGetADReward()
        {
            yield return new WaitUntil(() => ManagerData._instance.isRefreshedToken == true);

            ServerAPI.RequestAdWorldRankExchangeStation
            (
                (a) =>
                {
                    if (ServerContents.AdInfos[(int) adType].dailyLimit <= a.userAdInfo.usedCount)
                    {
                        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                            $"TYPE_{((int) adType).ToString()}_DAILY_MAX",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
                        var doc = JsonConvert.SerializeObject(achieve);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    }

                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        a.reward.type,
                        a.reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_AD_VIEW,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_AD_VIEW,
                        $"TYPE_{((int) adType).ToString()}_REWARD"
                    );

                    ManagerUI._instance.SyncTopUIAssets();
                    OpenPopupGetReward(a.reward);

                    complete?.Invoke(true, a.reward);
                }
            );
        }
    }

    public static void ShowAD_FailReset(AdType adType, Action<bool> complete = null)
    {
        if (adType != AdType.AD_17 && adType != AdType.AD_18)
        {
            complete(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm: $"TYPE_{((int) adType).ToString()}_START",
                arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                UIPopupReady.backupStageIndex);
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            // 일단 광고가 있었고, 보상을 받을 수 있는 상황
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_REWARD",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                UIPopupReady.backupStageIndex);
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            
            complete?.Invoke(true);
        }
    }
    
    public static void ShowAD_ReqAdLoginBonusRenewal(AdType adType, int rewardIndex, Action<bool> complete = null)
    {
        int cumulativeRewardIndex = rewardIndex;
        
        if (adType != AdType.AD_20)
        {
            complete?.Invoke(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                tag : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm : $"TYPE_{((int)adType).ToString()}_START",
                arlt : ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                num1 : cumulativeRewardIndex
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            } 
            
            // 일단 광고가 있었고, 보상을 받을 수 있는 상황
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_REWARD",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                cumulativeRewardIndex
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            
            complete?.Invoke(true);
        }
    }
    
    public static void ShowAD_ReqNPUContinue(AdType adType, Action<bool> complete = null)
    {
        if (adType != AdType.AD_21)
        {
            complete?.Invoke(false);
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm: $"TYPE_{((int) adType).ToString()}_START",
                arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                ServerContents.NpuAdContinueInfo.ver,
                Global.stageIndex.ToString());
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                return;
            }

            // 일단 광고가 있었고, 보상을 받을 수 있는 상황
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_REWARD",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS, 
                ServerContents.NpuAdContinueInfo.ver,
                Global.stageIndex.ToString());
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            
            complete?.Invoke(true);
        }
    }
    
    public static void ShowAD_ReqTurnRelaySaveWave(AdType adType, Action<bool> complete = null)
    {
        if (adType != AdType.AD_22)
        {
            complete?.Invoke(false);
            complete = null;
            return;
        }

        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(tag: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                cat: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                anm: $"TYPE_{((int) adType).ToString()}_START",
                arlt: ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                str1: ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString());
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        ShowAd(GetPlacementName(adType), CheckReward, false);

        void CheckReward(bool isRewarded)
        {
            if (!isRewarded)
            {
                complete?.Invoke(false);
                complete = null;
                return;
            }

            // 일단 광고가 있었고, 보상을 받을 수 있는 상황
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement(
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.AD_VIEW,
                $"TYPE_{((int) adType).ToString()}_REWARD",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS, 
                str1: ManagerTurnRelay.turnRelayIngame.CurrentWave.ToString());
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            
            complete?.Invoke(true);
            complete = null;
        }
    }
    #endregion

    #region 팝업

    private static void OpenPopupGetReward(Reward rewards)
    {
        ManagerUI._instance.OpenPopup<UIPopupGetReward>((popup) => { popup.InitReward(rewards); });
    }

    private static void OpenPopupGetReward(long time)
    {
        ManagerUI._instance.OpenPopup<UIPopupGetReward>((popup) => { popup.InitReduceTime(time); });
    }

    private static void OpenPopupNoADs(bool isPopupClose, Action<bool> adEndCallback)
    {
        ManagerUI._instance?.OpenPopup<UIPopupSystem>
        (
            (popup) =>
            {
                popup.InitSystemPopUp
                (
                    name: Global._instance.GetString("p_t_4"),
                    text: Global._instance.GetString("n_s_48"),
                    useButtons: false
                );

                if (isPopupClose == true)
                {
                    popup._callbackClose += () => ManagerUI._instance.ClosePopUpUI();
                }
                else
                {   //팝업이 자동으로 꺼지지 않는다면 실패 콜백을 여기서 호출
                    popup._callbackClose += () => adEndCallback?.Invoke(false);
                }
            }
        );
    }

    #endregion

    //각 타입별 광고 시청 가능 여부
    public static bool ADCheck(AdType adType)
    {
        //광고가 꺼진 경우
        if (ServerContents.AdInfos == null) return false;

        //광고 정보가 없는 경우
        if (!ServerContents.AdInfos.ContainsKey((int)adType)) return false;

        //해당 타입의 광고가 꺼져있는 경우
        //if (ServerContents.AdInfos[adType].) return false;

        //광고를 한번도 보지 않은 경우
        if (ServerRepos.UserAdInfos == null) return true;

        int index = ServerRepos.UserAdInfos.FindIndex(i => i.adType == (int)adType);

        if (index == -1)
        {
            //광고를 본적이 있지만 해당 타입의 광고는 본적이 없는 경우
            return true;
        }
        else
        {
            int usedCount = ServerRepos.UserAdInfos[index].usedCount;
            int dailyLimit = ServerContents.AdInfos[(int)adType].dailyLimit;
            int rewatchSet = ServerContents.AdInfos[(int)adType].rewatchSet;
            long lastUsedTime = ServerRepos.UserAdInfos[index].lastUsedTime;
            long useInterval = ServerContents.AdInfos[(int)adType].useInterval;

            //하루 리미트 / 인터벌 시간이 없는 경우 무조건 보여주도록 처리
            if (dailyLimit == 0 && useInterval == 0) return true;

            // 11, 12 타입이 아닌 경우 기본적으로 rewatchSet에 0이 들어가지만, 다른 값이 들어갔을 때도 정상적으로 광고를 출력하기 위해 조건문에 타입 체크 추가
            if (rewatchSet == 0 ||
                (adType != AdType.AD_11 && adType != AdType.AD_12))
            {
                //하루 리미트 횟수를 초과한 경우
                if (usedCount >= dailyLimit) return false;

                //마지막 광고 시청 후 대기시간 만큼 지나지 않은 경우
                if (Global.LeftTime(lastUsedTime) * -1 <= useInterval) return false;
            }
            else   // 하루 리미트 이후에도 재시청 가능한 광고 타입
            {
                if (usedCount >= dailyLimit * rewatchSet) return false;

                // 한 세트의 마지막 광고 시청 후 대기시간 만큼 지나지 않은 경우 (usedCount가 0일 경우 제외)
                if (usedCount > 0 && usedCount % dailyLimit == 0 && Global.LeftTime(lastUsedTime) * -1 <= useInterval) return false;
            }

            return true;
        }
    }

    public static void AppLovinSDK_MediationDebugger()
    {
        MaxSdk.ShowMediationDebugger();
    }

    private static void ADLog(string message)
    {
        Debug.Log($"[AD_TEST_LOG]{message}");
    }
}
