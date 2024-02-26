using System.Collections.Generic;
using UnityEngine;

public class UIPopUpAdventurePass : UIPopupBase
{
    public static UIPopUpAdventurePass _instance = null;

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
    [SerializeField] private GameObject objAdventurePassBuyBtn;
    [SerializeField] private GameObject objAdventurePassCheck;

    [Header("ScrollView")]
    [SerializeField] private GameObject _objAdventurePassItem;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private UIPanel blockPanel;

    [Header("LanPageInfo")]
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    [SerializeField] private UIItemLanpageButton lanpageButton;


    private List<UIItemAdventurePass> adventurePassItem = new List<UIItemAdventurePass>();
    private PackageBuyRoutine buyRoutine = null;

    #region Get/Set

    private bool AdventurePassBuy
    {
        set
        {
            if (value)
            {
                objAdventurePassBuyBtn.SetActive(false);
                objAdventurePassCheck.SetActive(true);
                SetAdventureActive(false);
            }
            else
            {
                objAdventurePassBuyBtn.SetActive(true);
                objAdventurePassCheck.SetActive(false);
                SetAdventureActive(true);
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

    private int AdventurePassIndex
    {
        get { return ManagerAdventurePass.GetAdventurePassData().packageIndex; }
    }

    #endregion

    #region 팝업 기본 함수

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
            texMainTitle.mainTexture = null;
            texMissionIcon.mainTexture = null;
        }
    }

    public override void OpenPopUp(int depth)
    {
        scrollView.depth = depth + 1;
        blockPanel.depth = depth + 2;

        base.OpenPopUp(depth);
    }

    #endregion

    #region UI 세팅

    public void InitData()
    {
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        //상단 UI 세팅
        SetTopUI();

        //ScrollView 세팅
        SetScrollView();

        //패스 구매 및 패키지 설정
        SetBuySetting();
    }

    private void SetTopUI()
    {
        texMainTitle.SettingTextureScale(800, 364);
        texMainTitle.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/",
            $"pass_adventure_{ManagerAdventurePass.GetAdventurePassData().resourceIndex}");

        tmsTitle.SetStringKey($"prpass_t_3");

        labelSubTitle.text = Global._instance.GetString($"prpass_st_3");

        texMissionIcon.LoadCDN(Global.gameImageDirectory, "IconPremiumPass/",
            $"icon_collection_adventure_{ManagerAdventurePass.GetAdventurePassData().resourceIndex}");
        texMissionIcon.MakePixelPerfect();

        labelMissionCount.text = $"{ServerRepos.UserAdventurePass.targetCount}";

        EndTsTimer.Run(target: labelEndTs, endTs: ManagerAdventurePass.GetAdventurePassData().endTs,
            timeOutAction: () => { labelEndTs.text = "00:00:00"; });

        labelPassInfo.SetText(Global._instance.GetString($"pr_b_{ServerRepos.UserAdventurePass.premiumState}"));
    }

    private void SetScrollView()
    {
        UIGrid uiGrid = scrollView.GetComponentInChildren<UIGrid>();
        int passMissionCount = ManagerAdventurePass.GetAdventurePassData().targetCount.Count;

        for (int i = 0; i < passMissionCount; i++)
        {
            UIItemAdventurePass _adventurePassItem = NGUITools.AddChild(uiGrid.transform, _objAdventurePassItem).GetComponent<UIItemAdventurePass>();
            _adventurePassItem.InitData(i);
            adventurePassItem.Add(_adventurePassItem);
        }

        scrollView.GetComponent<UIScrollView>().ResetPosition();
        scrollView.GetComponentInChildren<UIGrid>().Reposition();
        UIScrollView tempScrollView = scrollView.GetComponent<UIScrollView>();
        tempScrollView.ResetPosition();
        uiGrid.Reposition();
        tempScrollView.UpdateScrollbars();

        //스크롤 범위 최대, 최소 값 맞춰서 스크롤 될 위치 인덱스 구함
        int positionIndex = (ManagerAdventurePass.GetMissionProgress() - 3 < 0 ? 0 : ManagerAdventurePass.GetMissionProgress() - 3);
        tempScrollView.verticalScrollBar.value = ((float) uiGrid.cellHeight * positionIndex) / ((float) uiGrid.cellHeight * passMissionCount - scrollView.height);
    }

    private void SetBuySetting()
    {
        if (ManagerAdventurePass.passState == ManagerAdventurePass.PassBuyState.PASS_DISABLED)
        {
            objAdventurePassBuyBtn.SetActive(false);
            objAdventurePassCheck.SetActive(true);
            SetAdventureActive(true);
            labelPassInfo.SetText(Global._instance.GetString($"pr_b_1"));
        }
        else
        {
            AdventurePassBuy = ServerRepos.UserAdventurePass.premiumState == (int)ManagerAdventurePass.PassBuyState.PASS_ACTIVE;
            labelPassPrice.SetText(ServiceSDK.ServiceSDKManager.instance.GetPassPrice_String(AdventurePassIndex));
        }

        lanpageButton.On("LGPKV_pass_expedition", Global._instance.GetString("prpass_4"));

        if (ServerContents.Packages.ContainsKey(AdventurePassIndex))
        {
            var packageData = ServerContents.Packages[AdventurePassIndex];

            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.init(this, packageData);
        }
    }

    #endregion
    
    #region 클릭 이벤트
    
    public void OnClickMissionInfo()
    {
        ManagerUI._instance.OpenPopup<UIPopUpAdventurePassMission>();
    }

    public void OnClickBuyAdventurePass()
    {
        if (LanguageUtility.IsShowBuyInfo)
        {
            ManagerUI._instance.OpenMinorCheckPopup(
                ServiceSDK.ServiceSDKManager.instance.GetPassPrice_Double(AdventurePassIndex),
                ServiceSDK.ServiceSDKManager.instance.GetPassCurrency(AdventurePassIndex), OpenPurchaseConfirmPopup);
        }
        else
        {
            BuyAdventurePass();
        }
    }

    private void OpenPurchaseConfirmPopup()
    {
        //상품판매 법률 개정 표기
        int priceType = (int) RewardType.none; //1차 재화
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, BuyAdventurePass, true);
        string text = Global._instance.GetString("n_b_22");
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false,
            price_type: (PackagePriceType) priceType, price_value: labelPassPrice[0].text);
        popupSystem.ShowBuyInfo("buyinfo_prpass_1");
    }

    private void BuyAdventurePass()
    {
        if (ManagerAdventurePass.CheckStartable() == false)
        {
            OpenPopupNotBuyAdventurePass();
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

    public void OpenPopupNotBuyAdventurePass()
    {
        UIPopupSystem systemPopup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        systemPopup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_17"), false);
        systemPopup.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
        systemPopup.SortOrderSetting();
        systemPopup._callbackEnd += () => OnApplicationPauseReboot();
    }

    public void OnClickGetAllReward()
    {
        var nRewardState = ServerRepos.UserAdventurePass.rewardState;
        var pRewardState = ServerRepos.UserAdventurePass.premiumRewardState;

        bool isAllRewardGet = true;

        var nReward = ManagerAdventurePass.GetAdventurePassData().reward;
        var pReward = ManagerAdventurePass.GetAdventurePassData().premiumReward;

        for (int i = 0; i < nReward.Count; i++)
        {
            if (UIItemAdventurePass.RewardState.OPEN_REWARD == ManagerAdventurePass.GetRewardState(i, false))
            {
                isAllRewardGet = false;
                break;
            }

            if (ServerRepos.UserAdventurePass.premiumState > 1)
            {
                if (UIItemAdventurePass.RewardState.OPEN_REWARD == ManagerAdventurePass.GetRewardState(i, true))
                {
                    isAllRewardGet = false;
                    break;
                }
            }
        }

        if (isAllRewardGet)
        {
            ManagerAdventurePass.OpenPopupGetReward("n_s_71");
            return;
        }

        ServerAPI.AdventurePassReward(0, 0, (resp) =>
        {
            if (resp.IsSuccess)
            {
                List<Reward> nRewards = new List<Reward>();
                List<Reward> pRewards = new List<Reward>();

                List<int> nMissionIndex = new List<int>();
                List<int> pMissionIndex = new List<int>();

                for (int i = 0; i < nRewardState.Count; i++)
                {
                    if (nRewardState[i] != resp.userAdventurePass.rewardState[i])
                    {
                        nRewards.Add(ManagerAdventurePass.GetAdventurePassData().reward[i]);
                        nMissionIndex.Add(i + 1);
                    }

                    if (pRewardState[i] != resp.userAdventurePass.premiumRewardState[i])
                    {
                        for (int j = 0; j < ManagerAdventurePass.GetAdventurePassData().premiumReward[i].Count; j++)
                        {
                            pRewards.Add(ManagerAdventurePass.GetAdventurePassData().premiumReward[i][j]);
                        }

                        pMissionIndex.Add(i + 1);
                    }
                }

                if (nRewards.Count > 0 || pRewards.Count > 0)
                {
                    //받을 수 있는 보상이 있을 경우.(시간제 아이템의 경우 모두 받기에서 제외 됨.)
                    ManagerAdventurePass.OpenPopupGetReward("n_s_70");
                }
                else
                {
                    //받을 수 있는 보상이 없을 경우.(시간제 아이템의 경우 모두 받기에서 제외 됨.)
                    ManagerAdventurePass.OpenPopupGetReward("n_s_71");
                    return;
                }


                var uiitemAdventurePass = scrollView.GetComponentsInChildren<UIItemAdventurePass>();

                for (int i = 0; i < uiitemAdventurePass.Length; i++)
                {
                    uiitemAdventurePass[i].SetRewardState(i);
                }

                SendRewardGrowthy(nRewards, nMissionIndex, "NORMAL");
                SendRewardGrowthy(pRewards, pMissionIndex, "PREMIUM");

                Global.star = (int) GameData.User.Star;
                Global.clover = (int) (GameData.User.AllClover);
                Global.coin = (int) (GameData.User.AllCoin);
                Global.jewel = (int) (GameData.User.AllJewel);
                Global.wing = (int) (GameData.User.AllWing);
                Global.exp = (int) (GameData.User.expBall);

                if (ManagerUI._instance != null)
                    ManagerUI._instance.UpdateUI();
            }
        });
    }

    private void SendRewardGrowthy(List<Reward> rewardList, List<int> indexList, string rewardState)
    {
        for (int i = 0; i < indexList.Count; i++)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.ADVENTURE_PASS_REWARD,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_ADVENTURE_PASS_REWARD,
                $"ADVENTURE_PASS_REWARD_{indexList[i]}_{rewardState}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
        }

        for (int i = 0; i < rewardList.Count; i++)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                rewardList[i].type,
                rewardList[i].value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ADVENTURE_PASS_REWARD,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ADVENTURE_PASS_REWARD,
                $"G_ADVENTURE_PASS_REWARD_{ManagerAdventurePass.GetAdventurePassData().vsn}_{rewardState}"
            );
        }
    }

    private void RebootSetting()
    {
        ManagerAdventurePass.passState = ManagerAdventurePass.PassBuyState.PASS_DISABLED;

        //패스 구매 설정
        objAdventurePassBuyBtn.SetActive(false);
        objAdventurePassCheck.SetActive(true);

        //그로시
        {
            var prmiumPass = ManagerAdventurePass.GetAdventurePassData();
            var userAdventurePass = ServerRepos.UserAdventurePass;

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE_MISSION,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.G_ADVENTURE_PASS,
                $"ADVENTURE_PASS_{prmiumPass.vsn}_{ManagerAdventurePass.GetMissionProgress()}",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var docAchieve = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docAchieve);
        }

        SetRebootPopup();
    }

    private void SetRebootPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopup<UIPopupSystem>();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_21"), true);
        popup.FunctionSetting(1, "OnApplicationPauseReboot", this.gameObject);
        popup.SetButtonText(1, Global._instance.GetString("btn_54"));
        popup.FunctionSetting(2, "SetAdventurePassActiveText", this.gameObject);
        popup.SetButtonText(2, Global._instance.GetString("btn_55"));
        popup.SortOrderSetting();
        popup._callbackEnd += () => SetAdventurePassActiveText();
    }

    private void SetAdventurePassActiveText()
    {
        labelPassInfo.SetText(Global._instance.GetString($"pr_b_1"));
    }

    private void OnApplicationPauseReboot()
    {
        if (UIPopupSystem._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }

        Global.ReBoot();
    }

    private void SetAdventureActive(bool isActive)
    {
        for (int i = 0; i < adventurePassItem.Count; i++)
        {
            adventurePassItem[i].SetPremiumRewardBlock(isActive);
        }
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms,
            "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms,
            "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
    
    #endregion
}