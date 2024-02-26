using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPopUpDiaStashEvent : UIPopupBase
{
    [Header("TopUI")]
    [SerializeField] private TextMeshGlobalString title;
    [SerializeField] private UISprite mainTitle;
        
    [SerializeField] private UILabel labelTs;
    
    [SerializeField] private UILabel[] labelAllDia;
    [SerializeField] private UILabel[] labelDefaultDia;
    [SerializeField] private UILabel[] labelAddDia;
    [SerializeField] private GameObject objFull;

    [Header("BottomUI")] 
    [SerializeField] private UILabel labelBonusInfo;
    
    [SerializeField] private UISprite sprBeforeDia;

    [SerializeField] private UILabel[] labelBeforeAllDia;
    [SerializeField] private UILabel labelBeforeDefaultDia;
    [SerializeField] private UILabel labelBeforeAddDia;
    
    [SerializeField] private UISprite sprAfterDia;
    
    [SerializeField] private UILabel[] labelAfterAllDia;
    [SerializeField] private UILabel labelAfterDefaultDia;
    [SerializeField] private UILabel labelAfterAddDia;

    [SerializeField] private UISlider progressBar;
    [SerializeField] private GameObject objDiaIcon;
    [SerializeField] private UILabel labelBuyCount;

    [SerializeField] private UILabel[] labelPackagePrice;

    [SerializeField] private GameObject objFullDiaEffect;

    //거래법 링크 관련
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    [SerializeField] private UISprite sprBottomBG;
    [SerializeField] private GameObject objHelpBtn; 

    [SerializeField] private List<UISprite> listDiaStashSprite;

    #region 결제 관련
    
    private int               packageId   = 0;
    private CdnShopPackage    packageData = null;
    private string            productCode = string.Empty;
    private PackageBuyRoutine buyRoutine  = null;
    #endregion

    private int packageGrade   = 0;

    #region 그로시에서 사용될 데이터
    private int  prevGrade           = 0;
    private int  prevSegment         = 0;
    private int  prevBuyCount        = 0;
    private int  prevAddDia          = 0;
    private int  prevStageClearCount = 0;
    public  bool bAutoOpen           = false;
    #endregion
    
    
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

    private void Awake()
    {
        packageGrade = ManagerDiaStash.instance.GetPackageGrade();
    }
    
    protected override void OnDestroy()
    {
        if (buyRoutine != null)
        {
            buyRoutine.PostPurchaseCompleteEvent -= OnPurchaseComplete;
        }
        
        base.OnDestroy();
    }
    
    public void InitData()
    {
        SetAtlas();
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        //패키지 데이터 세팅
        CdnShopPackage in_data = ManagerDiaStash.instance.GetCurrentShopPackageData();
        packageData = in_data;
        
        //패키지 ID 설정
        this.packageId = packageData.idx;
        
        //OS 에 따른 상품 ID(productID) 설정
#if UNITY_IPHONE
        this.productCode = in_data.sku_i;
#elif UNITY_ANDROID
        this.productCode = packageData.sku_a;
#endif
        
        //구매 가격 설정
        labelPackagePrice.SetText(ManagerDiaStash.instance.GetPackagePrice(packageData));
        
        //구매 카운트 설정
        labelBuyCount.text = $"{ManagerDiaStash.instance.GetBuyCount()}/{ManagerDiaStash.instance.GetPackageBuyLimit()}";

        
        //타이틀 세팅
        SetTitle();

        //받아온 결제 정보로 말풍선 다이아 데이터 세팅
        SetBubbleDia();

        //보너스 정보 텍스트 세팅
        string bonusInfoText = ManagerDiaStash.instance.IsFullDia() ? Global._instance.GetString("p_dp_2") :
            Global._instance.GetString("p_dp_1").Replace("[n]", $"{ManagerDiaStash.instance.GetBonusDia()}");
        labelBonusInfo.text = bonusInfoText;
        
        //다이아 세팅
        SettingDia();
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text  = TermsOfUse;
        labelPrecautions.text = Precautions;

        if (string.IsNullOrEmpty(TermsOfUse) || string.IsNullOrEmpty(Precautions))
        {
            sprBottomBG.height += -50;
            objHelpBtn.transform.localPosition += new Vector3(0f, 50f, 0f);
        }
    }
    
    private void SetAtlas()
    {
        for (int i = 0; i < listDiaStashSprite.Count; i++)
        {
            listDiaStashSprite[i].atlas = ManagerDiaStash.instance.diaStashResource.GetDiaStashPack().AtlasUI;
        }
    }

    void SetTitle()
    {
        //타이틀
        title.SetPath($"Prefab/Title/TMP_DiaStashEvent_{packageGrade}_Title");
        title.SetStringKey($"title_dp_{packageGrade}");
        
        //타이틀 이미지
        mainTitle.spriteName = $"mayuji_pack_bg_0{packageGrade}";
        
        //남은시간
        EndTsTimer.Run(labelTs, ManagerDiaStash.instance.GetEndTs());
    }

    void SetBubbleDia()
    {
        string allDia     = ManagerDiaStash.instance.GetCurrentDia().ToString();
        string defaultDia = ManagerDiaStash.instance.GetDefaultDia().ToString();
        string addDia     = ManagerDiaStash.instance.GetAddDia().ToString();
        
        foreach (var item in labelAllDia)
            item.text = allDia;
        foreach (var item in labelDefaultDia)
            item.text = defaultDia;
        foreach (var item in labelAddDia)
            item.text = addDia;
        
        objFull.SetActive(ManagerDiaStash.instance.IsFullDia());
    }

    void SettingDia()
    {
        string defaultDia = ManagerDiaStash.instance.GetDefaultDia().ToString();
        string fullDia      = ManagerDiaStash.instance.GetFullDia().ToString();
        foreach (var item in labelBeforeAllDia)
            item.text = defaultDia;
        foreach (var item in labelAfterAllDia)
            item.text = fullDia;
        
        labelBeforeDefaultDia.text = defaultDia;
        labelAfterDefaultDia.text  = defaultDia;
        
        labelBeforeAddDia.text = $"{ManagerDiaStash.instance.GetMinBonusDia()}";
        labelAfterAddDia.text = $"{ManagerDiaStash.instance.GetFullBonusDia()}";

        sprBeforeDia.spriteName = $"mayuji_pack_dia{packageGrade}";
        sprAfterDia.spriteName = $"mayuji_pack_dia{packageGrade + 1}";

        progressBar.value = ManagerDiaStash.instance.GetCurrentStageValue();
        
        objFullDiaEffect.SetActive(ManagerDiaStash.instance.IsFullDia());
        sprAfterDia.GetComponent<TweenScale>().enabled = ManagerDiaStash.instance.IsFullDia();

        if (IsDiaIconActive())
        {
            objDiaIcon.SetActive(true);
            objDiaIcon.transform.localPosition = new Vector3(500f * ManagerDiaStash.instance.GetCurrentStageValue(), 0f, 0f);
        }
        else
        {
            objDiaIcon.SetActive(false);
        }
    }

    bool IsDiaIconActive()
    {
        // 유저 현재 스테이지 클리어 횟수 0 or 클리어 횟수 Full로 채웠을 때 false.
        float currentStageValue = ManagerDiaStash.instance.GetCurrentStageValue(); 
        if (currentStageValue == 0 || currentStageValue == 1)
            return false;
        else
            return true;
    }

    void OnClickHelp()
    {
        if (this.bCanTouch == false)
            return;
        
        ManagerUI._instance.OpenPopup<UIPopupDiaStashEventInfo>();
    }

    private void OnClickBtnPurchase()
    {
        if (this.bCanTouch == false)
            return;

        //결제 이후, 그로시에서 사용될 데이터 초기화
        SetGrowthyData();

#if UNITY_EDITOR
        PurchaseTest_Editor();
        return;
#endif
        //결제 관련 정보가 잘못되어 있다면 구매 하지 않음
        if (string.IsNullOrEmpty(this.productCode))
            return;
        
        if (buyRoutine == null)
        {   //결제 완료 시 호출될 콜백 등록
            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.PostPurchaseCompleteEvent += OnPurchaseComplete;
        }
        
        //결제 완료 메세지 설정
        string message = Global._instance.GetString("n_b_9").Replace("[1]", name);
        buyRoutine.init(this, packageData, message, autoOpen:bAutoOpen);
        
        //패키지 구매.
        buyRoutine.OnClickButton();
    }

    /// <summary>
    /// 에디터 상에서 동작하는 가상 결제 테스트 함수
    /// </summary>
    private void PurchaseTest_Editor()
    {
        bCanTouch = false;
        
        //가상으로 유저 패키지 구매 데이터 갱신
        int fIdx = ServerRepos.UserShopPackages.FindIndex(x => x.idx == packageId);
        if (fIdx == -1)
        {
            ServerUserShopPackage pack = new ServerUserShopPackage();
            pack.idx = packageId;
            pack.vsn = 1;
            pack.sku = productCode;
            ServerRepos.UserShopPackages.Add(pack);
            pack.buyCount++;
        }
        else
        {
            ServerRepos.UserShopPackages[fIdx].buyCount++;
        }
        
        //최대 구매 횟수에 도달했으면 다음 단계의 상품으로 변경
        if (ManagerDiaStash.instance.GetBuyCount() >= ManagerDiaStash.instance.GetPackageBuyLimit())
        {
            ServerRepos.UserDiaStash.grade++;
            ServerRepos.UserDiaStash.stageClearCount = 0;
            
            int currentSegment = ServerRepos.UserDiaStash.segment;
            //더 이상 현재 세그먼트에서 구매 가능한 패키지가 없을 때 처리
            if (ServerContents.DiaStashEvent.packages[currentSegment].Count < ManagerDiaStash.instance.GetPackageGrade())
            {   //구매할 수 있는 패키지가 남아있다면 세그먼트 변경
                if (ServerContents.DiaStashEvent.packages.Count > currentSegment)
                {
                    ServerRepos.UserDiaStash.segment++;
                }
            }
        }

        //구매 완료 후 호출되는 콜백 함수 호출
        OnPurchaseComplete();
    }

    private void OnPurchaseComplete()
    {
        bCanTouch = false;
        
        ServerAPI.BuyDiaStash(ManagerDiaStash.instance.GetCurrentPackageIdx(), (resp) =>
        {
            if(resp.IsSuccess)
            {
                //획득한 상품 ui에 갱신
                Global.jewel = GameData.User.AllJewel;
                Global.clover = GameData.User.AllClover;
                Global.coin = GameData.User.AllCoin;
                Global.star = GameData.User.Star;
                ManagerUI._instance.UpdateUI();
                ManagerUI._instance.PackageUpdate();

                //구매 시, 구매 정보에 대한 그로시 로그 전송
                SendGrowthy();
                
                //구매 시, 패키지 데이터 
                ManagerDiaStash.instance.SyncDiaStashPackageData();
                ManagerDiaStash.instance.SyncUserDiaCount();
                
                //현재 구매한 패키지의 최대 구매 횟수를 갱신했을 때 or 모든 패키지를 구매 했을 때
                //현재 grade에서 패키지를 최대 구매 했을 때 BuyCount가 0으로 초기화가 됨.
                if(ManagerDiaStash.instance.GetBuyCount() == 0 || ManagerDiaStash.IsAllBuyPackage())
                {
                    //패키지를 구입 했으므로 StageClearCount를 0으로 초기화
                    ManagerDiaStash.instance.SyncPrevStageClearCount(0);
                    ManagerDiaStash.instance.SyncPrevBonusDiaCount();
                }

                //구매 성공 팝업 호출
                string name = Global._instance.GetString("p_t_4");
                string message = Global._instance.GetString("n_b_14");
                UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

                popup.FunctionSetting(1, "PurchaseCompleteAction", gameObject, true);
                popup.FunctionSetting(3, "PurchaseCompleteAction", gameObject, true);
                popup.FunctionSetting(4, "PurchaseCompleteAction", gameObject, true);
                popup.SortOrderSetting();
                popup.InitSystemPopUp(name, message, false);
                popup.SetResourceImage("Message/icon_buyPackage");
            }
            else
            {
                bCanTouch = true;
            }
        });
    }

    /// <summary>
    /// 구매 성공 팝업 이후 동작에 대한 함수
    /// </summary>
    private void PurchaseCompleteAction()
    {
        //현재 구매한 패키지의 최대 구매 횟수를 갱신했을 때 or 모든 패키지를 구매 했을 때.
        //현재 grade에서 패키지를 최대 구매 했을 때 BuyCount가 0으로 초기화가 됨.
        if (ManagerDiaStash.instance.GetBuyCount() == 0 || ManagerDiaStash.IsAllBuyPackage())
        {
            //추가로 구매 가능한 패키지가 남아있다면 팝업 재갱신
            if(ManagerDiaStash.CheckStartable() && ManagerDiaStash.IsAllBuyPackage() == false)
            {
                _callbackEnd = () => ManagerUI._instance.OpenPopup<UIPopUpDiaStashEvent>((popup) =>
                {
                    popup.InitData();
                    popup.bAutoOpen = this.bAutoOpen;
                    popup.SetPurchaseCompleteEvent(purchaseCompleteEvent);
                });
            }
            
            //등록된 이벤트가 있을 때 만 실행.
            purchaseCompleteEvent?.Invoke();

            ClickBlocker.Make(1.0f);

            ManagerUI._instance.ClosePopUpUI();
        }
        else
        { 
            //구매 가능 횟수가 남아있다면 구매 카운트만 갱신
            labelBuyCount.text = $"{ManagerDiaStash.instance.GetBuyCount()}/{ManagerDiaStash.instance.GetPackageBuyLimit()}";
            
            bCanTouch = true;
        }
    }

    private System.Action purchaseCompleteEvent = null;
    
    /// <summary>
    /// 패키지 구매 후 실행되는 이벤트를 추가하는 함수.
    /// </summary>
    /// <param name="action"></param>
    public void SetPurchaseCompleteEvent(System.Action action)
    {
        purchaseCompleteEvent = action;
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        if (this.bCanTouch == false)
            return;
        
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        if (this.bCanTouch == false)
            return;
        
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
    
    #region 그로시 처리
    private void SetGrowthyData()
    {
        //그로시에서 사용될 데이터를 현재 상태로 캐싱해둠
        prevGrade           = ManagerDiaStash.instance.GetPackageGrade();
        prevSegment         = ServerRepos.UserDiaStash.segment;
        prevStageClearCount = ServerRepos.UserDiaStash.stageClearCount;
        prevBuyCount        = ManagerDiaStash.instance.GetBuyCount();
        prevAddDia          = ManagerDiaStash.instance.GetAddDia();
    }
    
    private void SendGrowthy()
    {
        //그로시 추가
        string segment = prevSegment == 1 ? "NPU" : "PU";
        if (prevSegment > 2)
            segment = prevSegment.ToString();
            
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.DIA_STASH,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.DIA_STASH_BUY,
            $"{segment}_{prevGrade}_{prevStageClearCount}_{ServerContents.DiaStashEvent.eventIndex}_{GetBuyGameTypeGrowthy()}_{(prevBuyCount + 1)}",
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
        );
        
        //그로시 로그 디버깅
        var achievementDoc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", achievementDoc);
        
        var getDia = new ServiceSDK.GrowthyCustomLog_Money(
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_DIA_STASH,
            ManagerDiaStash.instance.GetDefaultDia(),
            prevAddDia,
            (int)(ServerRepos.User.jewel),
            (int)(ServerRepos.User.fjewel),
            this.productCode
        );

        var docDIA = JsonConvert.SerializeObject(getDia);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDIA);
    }

    private string GetBuyGameTypeGrowthy()
    {
        string gameType = SceneManager.GetActiveScene().name == "Lobby" ? "LOBBY" : $"{Global.GameType}";

        return gameType;
    }
    #endregion
}
