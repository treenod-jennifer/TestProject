using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

public class UIPopUpPremiumPass : UIPopupBase
{
    public static UIPopUpPremiumPass _instance = null;

    [Header("TopUI")]
    [SerializeField] private UIUrlTexture texMainTitle;
    [SerializeField] private UILabel labelSubTitle;
    [SerializeField] private UILabel labelEndTs;
    [SerializeField] private UIUrlTexture texMissionIcon;
    [SerializeField] private UILabel labelMissionCount;
    [SerializeField] private TextMeshGlobalString tmsTitle;

    [Header("Pass")]
    [SerializeField] private UILabel[] labelPassInfo;
    [SerializeField] private UILabel[] labelPassPrice;
    [SerializeField] protected GameObject objPremiumPassBuyBtn;
    [SerializeField] protected GameObject objPremiumPassCheck;

    [Header("ScrollView")]
    [SerializeField] private GameObject _objPremiumPassItem;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private UIPanel blockPanel;

    [Header("LanPageInfo")]
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    [SerializeField] private UIItemLanpageButton lanpageButton;


    private List<UIItemPremiumPass> premiumPassItem = new List<UIItemPremiumPass>();
    private PackageBuyRoutine buyRoutine = null;

    private int Type = 0;

    protected bool PremiumPassBuy
    {
        set
        {
            if (value)
            {
                objPremiumPassBuyBtn.SetActive(false);
                objPremiumPassCheck.SetActive(true);
                SetPremiumActive(false);
            }
            else
            {
                objPremiumPassBuyBtn.SetActive(true);
                objPremiumPassCheck.SetActive(false);
                SetPremiumActive(true);
            }
        }
    }

    private string TermsOfUse
    {
        get
        {
            string value = Global._instance.GetString("p_dia_4");

            if (string.IsNullOrEmpty(value))
                objTermsOfUse.SetActive(false);
            else
                objTermsOfUse.SetActive(true);

            return value;
        }
    }

    private string Precautions
    {
        get
        {
            string value = Global._instance.GetString("p_dia_3");

            if (string.IsNullOrEmpty(value))
                objPrecautions.SetActive(false);
            else
                objPrecautions.SetActive(true);

            return value;
        }
    }

    private int PremiumPassIndex
    {
        get { return ManagerPremiumPass.GetPremiumPassData().packageIndex; }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    protected override void OnDestroy()
    {
        if (buyRoutine != null)
            buyRoutine.PostPurchaseCompleteEvent -= RebootSetting;

        base.OnDestroy();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void InitData(int type)
    {
        Type = type;

        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        //상단 UI 세팅
        SetTopUI();

        //ScrollView 세팅
        SetScrollView();

        //패스 구매 설정
        if (ManagerPremiumPass.passState == ManagerPremiumPass.PassBuyState.PASS_DISABLED)
        {
            objPremiumPassBuyBtn.SetActive(false);
            objPremiumPassCheck.SetActive(true);
            SetPremiumActive(true);

            labelPassInfo.SetText(Global._instance.GetString($"pr_b_1"));
        }
        else
        {
            PremiumPassBuy = ServerRepos.UserPremiumPass.premiumState > 1;

            labelPassPrice.SetText(ServiceSDKManager.instance.GetPassPrice_String(PremiumPassIndex));
        }

        lanpageButton.On($"LGPKV_pass_{ManagerPremiumPass.GetPassType(false)}", Global._instance.GetString("prpass_4"));

        //패키지 등록
        if (ServerContents.Packages.ContainsKey(PremiumPassIndex))
        {
            var packageData = ServerContents.Packages[PremiumPassIndex];

            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.init(this, packageData);
        }
    }

    private void SetScrollView()
    {
        UIGrid uiGrid = scrollView.GetComponentInChildren<UIGrid>();
        
        for (int i = 0; i < ManagerPremiumPass.GetMissionCount(); i++)
        {
            UIItemPremiumPass _premiumPassItem 
                = NGUITools.AddChild(uiGrid.transform, _objPremiumPassItem).GetComponent<UIItemPremiumPass>();
            _premiumPassItem.InitData(i);
            
            premiumPassItem.Add(_premiumPassItem);
        }

        scrollView.GetComponent<UIScrollView>().ResetPosition();
        scrollView.GetComponentInChildren<UIGrid>().Reposition();
        UIScrollView tempScrollView = scrollView.GetComponent<UIScrollView>();
        tempScrollView.ResetPosition();
        uiGrid.Reposition();
        tempScrollView.UpdateScrollbars();

        //스크롤범위 최대, 최소 값 맞춰서 스크롤 될 위치 인덱스 구함
        int positionIndex = (ServerRepos.UserPremiumPass.missionProgress - 3 < 0
            ? 0
            : ServerRepos.UserPremiumPass.missionProgress - 3);
        
        tempScrollView.verticalScrollBar.value =
            ((float) uiGrid.cellHeight * positionIndex) /
            ((float) uiGrid.cellHeight * ManagerPremiumPass.GetMissionCount() -
             scrollView.height);
    }

    private void SetTopUI()
    {
        texMainTitle.SettingTextureScale(800, 364);
        texMainTitle.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"pass_{ManagerPremiumPass.GetPremiumPassResourceIndex()}");
        
        tmsTitle.SetStringKey($"prpass_t_{Type}");

        labelSubTitle.text = Global._instance.GetString($"prpass_st_{Type}");
        
        texMissionIcon.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/", $"icon_collection_{ManagerPremiumPass.GetPremiumPassResourceIndex()}");
        texMissionIcon.MakePixelPerfect();

        labelMissionCount.text = $"{ServerRepos.UserPremiumPass.targetCount}";

        //EndTs
        SetTime_Play();

        labelPassInfo.SetText(Global._instance.GetString($"pr_b_{ServerRepos.UserPremiumPass.premiumState}"));
    }

    private void SetTime_Play()
    {
        EndTsTimer.Run(target: labelEndTs, endTs: ManagerPremiumPass.GetPremiumPassData().endTs, 
            timeOutAction: () => { labelEndTs.text = "00:00:00"; });
    }

    public override void OpenPopUp(int depth)
    {
        scrollView.depth = depth + 1;
        blockPanel.depth = depth + 2;

        base.OpenPopUp(depth);
    }

    public void OnClickMissionInfo()
    {
        ManagerUI._instance.OpenPopup<UIPopupPremiumPassMission>();
    }

    public void OnClickBuyPremiumPass()
    {
        if (LanguageUtility.IsShowBuyInfo)
        {
            ManagerUI._instance.OpenMinorCheckPopup(ServiceSDKManager.instance.GetPassPrice_Double(PremiumPassIndex), ServiceSDKManager.instance.GetPassCurrency(PremiumPassIndex), OpenPurchaseConfirmPopup);
        }
        else
        {
            BuyPremiumPass();
        }
    }

    private void OpenPurchaseConfirmPopup()
    {
        //상품판매 법률 개정 표기
        int priceType = (int)RewardType.none; //1차 재화
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, BuyPremiumPass, true);
        string text = Global._instance.GetString("n_b_22");
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false,price_type: (PackagePriceType)priceType,price_value: labelPassPrice[0].text);
        popupSystem.ShowBuyInfo("buyinfo_prpass_1");
    }

    void BuyPremiumPass()
    {
        if(ManagerPremiumPass.CheckStartable() == false)
        {
            OpenPopupNotBuyPremiumPass();
            return;
        }

#if UNITY_EDITOR
        RebootSetting();
#else
        if(buyRoutine.PostPurchaseCompleteEvent == null)
            buyRoutine.PostPurchaseCompleteEvent += RebootSetting;
        buyRoutine.OnClickButtonImmediately();
#endif
    }

    public void OpenPopupNotBuyPremiumPass()
    {
        UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_17"), false);
        systemPopup.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
        systemPopup.SortOrderSetting();
        systemPopup._callbackEnd += () => OnApplicationPauseReboot();
    }

    public void OnClickGetAllReward()
    {
        var nRewardState = ServerRepos.UserPremiumPass.rewardState;
        var pRewardState = ServerRepos.UserPremiumPass.premiumRewardState;

        bool isAllRewardGet = true;

        var nReward = ManagerPremiumPass.GetPremiumPassData().reward;
        var pReward = ManagerPremiumPass.GetPremiumPassData().premiumReward;

        for (int i = 0; i < nReward.Count; i++)
        {
            if(UIItemPremiumPass.RewardState.OPEN_REWARD == ManagerPremiumPass.GetRewardState(i, false))
            {
                isAllRewardGet = false;
                break;
            }

            if(ServerRepos.UserPremiumPass.premiumState > 1)
            {
                if (UIItemPremiumPass.RewardState.OPEN_REWARD == ManagerPremiumPass.GetRewardState(i, true))
                {
                    isAllRewardGet = false;
                    break;
                }
            }
        }

        if (isAllRewardGet)
        {
            ManagerPremiumPass.OpenPopupGetReward("n_s_71");
            return;
        }

        ServerAPI.PremiumPassReward(0, 0, 
            (resp) => 
            { 
                if(resp.IsSuccess)
                {
                    List<Reward> nRewards = new List<Reward>();
                    List<Reward> pRewards = new List<Reward>();

                    List<int> nMissionIndex = new List<int>();
                    List<int> pMissionIndex = new List<int>();

                    for (int i = 0; i < nRewardState.Count; i++)
                    {
                        if(nRewardState[i] != resp.userPremiumPass.rewardState[i])
                        {
                            nRewards.Add(ManagerPremiumPass.GetPremiumPassData().reward[i]);
                            nMissionIndex.Add(i + 1);
                        }

                        if(pRewardState[i] != resp.userPremiumPass.premiumRewardState[i])
                        {
                            for (int j = 0; j < ManagerPremiumPass.GetPremiumPassData().premiumReward[i].Count; j++)
                            {
                                pRewards.Add(ManagerPremiumPass.GetPremiumPassData().premiumReward[i][j]);
                            }
                            pMissionIndex.Add(i + 1);
                        }
                    }

                    if (nRewards.Count > 0 || pRewards.Count > 0)
                    {//받을 수 있는 보상이 있을 경우.(시간제 아이템의 경우 모두 받기에서 제외 됨.)
                        ManagerPremiumPass.OpenPopupGetReward("n_s_70");
                    }
                    else
                    {//받을 수 있는 보상이 없을 경우.(시간제 아이템의 경우 모두 받기에서 제외 됨.)
                        ManagerPremiumPass.OpenPopupGetReward("n_s_71");
                        return;
                    }
                    

                    var uiitemPremiumPass = scrollView.GetComponentsInChildren<UIItemPremiumPass>();

                    for (int i = 0; i < uiitemPremiumPass.Length; i++)
                    {
                        uiitemPremiumPass[i].SetRewardState(i);
                    }

                    //그로시
                    {

                        for (int i = 0; i < nMissionIndex.Count; i++)
                        {
                            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                            (
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PREMIUM_PASS_REWARD,
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_PREMIUM_PASS_REWARD,
                                $"PREMIUM_PASS_REWARD_{nMissionIndex[i]}_NORMAL_{ManagerPremiumPass.GetPassType()}",
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                            );
                            var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
                        }
                        
                        for (int i = 0; i < nRewards.Count; i++)
                        {
                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                nRewards[i].type,
                                nRewards[i].value,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PREMIUM_PASS_REWARD,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PREMIUM_PASS_REWARD,
                                $"G_PREMIUM_PASS_REWARD_{ManagerPremiumPass.GetPassType()}_{resp.userPremiumPass.vsn}_NORMAL"
                            );
                        }

                        for (int i = 0; i < pMissionIndex.Count; i++)
                        {
                            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                            (
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PREMIUM_PASS_REWARD,
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_PREMIUM_PASS_REWARD,
                                $"PREMIUM_PASS_REWARD_{pMissionIndex[i]}_PREMIUM_{ManagerPremiumPass.GetPassType()}",
                                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                            );
                            var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
                        }

                        for (int i = 0; i < pRewards.Count; i++)
                        {

                            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                pRewards[i].type,
                                pRewards[i].value,
                                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PREMIUM_PASS_REWARD,
                                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PREMIUM_PASS_REWARD,
                                $"G_PREMIUM_PASS_REWARD_{ManagerPremiumPass.GetPassType()}_{resp.userPremiumPass.vsn}_PREMIUM"
                            );
                        }
                    }

                    Global.star = (int)GameData.User.Star;
                    Global.clover = (int)(GameData.User.AllClover);
                    Global.coin = (int)(GameData.User.AllCoin);
                    Global.jewel = (int)(GameData.User.AllJewel);
                    Global.wing = (int)(GameData.User.AllWing);
                    Global.exp = (int)(GameData.User.expBall);

                    if (ManagerUI._instance != null)
                        ManagerUI._instance.UpdateUI();
                }
            });
    }

    void RebootSetting()
    {
        ManagerPremiumPass.passState = ManagerPremiumPass.PassBuyState.PASS_DISABLED;

        //패스 구매 설정
        objPremiumPassBuyBtn.SetActive(false);
        objPremiumPassCheck.SetActive(true);

        //그로시
        {
            var prmiumPass = ManagerPremiumPass.GetPremiumPassData();
            var userPremiumPass = ServerRepos.UserPremiumPass;

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE_MISSION,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_PREMIUM_PASS,
                $"PREMIUM_PASS_{ManagerPremiumPass.GetPassType()}_{prmiumPass.vsn}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
        }

        SetRebootPopup();
    }

    void SetRebootPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_21"), true);
        popup.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
        popup.SetButtonText(1, Global._instance.GetString("btn_54"));
        popup.FunctionSetting(2, "SetPremiumPassActiveText", this.gameObject);
        popup.SetButtonText(2, Global._instance.GetString("btn_55"));
        popup.SortOrderSetting();
        popup._callbackEnd += () => SetPremiumPassActiveText();
    }

    void SetPremiumPassActiveText()
    {
        labelPassInfo.SetText(Global._instance.GetString($"pr_b_1"));
    }

    void OnApplicationPauseReboot()
    {
        if (UIPopupSystem._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
        Global.ReBoot();
    }

    void SetPremiumActive(bool isActive)
    {
        for (int i = 0; i < premiumPassItem.Count; i++)
        {
            premiumPassItem[i].SetPremiumRewardBlock(isActive);
        }
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
}