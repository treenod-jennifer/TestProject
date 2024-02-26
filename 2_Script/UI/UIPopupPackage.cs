using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PokoAddressable;
using ServiceSDK;
using Spine.Unity;

public class UIPopupPackage : UIPopupBase
{
    public static UIPopupPackage _instance = null;

    public UITexture  texture;
    public UITexture  textureReconfirm; //팝업 내 재확인 이미지
    public UILabel    loadingText;
    public UILabel[]  buttonText;
    public GameObject diaObject;

    public UILabel[] buyLimitText;
    public GameObject buyLimitRootObject;

    //Notice _data = null;
    private string productCode = string.Empty;
    private int packageId = 0;
    CdnShopPackage packageData = null;
    private string messagePackageIconPath = "Message/icon_buyPackage";
    private string messageFailIconPath = "Message/tired";
    private SkeletonAnimation _spineObject = null;

    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    [SerializeField] private UILabel labelTimer;
    [SerializeField] private UIPanel _panelSpineUpper;

    [SerializeField] private GameObject objPackageBack;
    
    public bool bAutoOpen        = false; //배너 리스트에서 상품 노출도 개선 건으로 추가된 자동 오픈 여부
    public bool bUseBuyCount     = false; //구매 횟수 버블 사용 여부
    public bool bUseConfirmPopup = false; //구매 확인 팝업 사용 여부
    
    public  bool bUseReconfirm = false; //팝업 내에서 변경하는 재확인 이미지 사용 여부
    private bool bCanReconfirm = true;  //재확인 이미지 출력 여부

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

    void Awake()
    {
        _instance = this;
    }

    protected override void OnDestroy()
    {
        texture.mainTexture = null;
        if (bUseReconfirm)
        {
            textureReconfirm.mainTexture = null;
        }
        base.OnDestroy();
    }

    bool CheckPackageType_UseTimer()
    {
        bool IsTimerActive = false;

        var packageType = ServerContents.Packages[packageId].type;
        
        if (packageType == 1 || packageType == 2 || packageType == 3 || packageType == (int)PackageType.SpotCloverPack)
            IsTimerActive = false;
        else
            IsTimerActive = true;
        
        return IsTimerActive;
    }
    
    public void InitPopUp(CdnShopPackage in_data, Method.FunctionVoid func = null, bool autoOpen = false)
    {
        packageData = in_data;
        this._callbackEnd += func;
        this.packageId = in_data.idx;
        this.bAutoOpen = autoOpen;
        
        if(LanguageUtility.IsShowBuyInfo && CheckPackageType_UseTimer())
        {
            labelTimer.gameObject.SetActive(true);
            EndTsTimer.Run(labelTimer, packageData.expireTs);
        }
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        if (string.IsNullOrEmpty(labelTermsOfUse.text) && string.IsNullOrEmpty(labelPrecautions.text))
        {
            objPackageBack.SetActive(false);
            mainSprite.height = 1016;
        }
        else
        {
            objPackageBack.SetActive(true);
            mainSprite.height = 1065;
        }

#if UNITY_IPHONE
        this.productCode = in_data.sku_i;
#elif UNITY_ANDROID
            this.productCode = in_data.sku_a;
#endif

        diaObject.SetActive(in_data.payType != 0);

        if (in_data.payType == 0)
        {
            string baseDisplayPrice = LanguageUtility.GetPrices(in_data.prices);
            buttonText[0].transform.localPosition = new Vector3(-6, 3, 0);
            buttonText[0].text = baseDisplayPrice;
            buttonText[1].text = baseDisplayPrice;

#if !UNITY_EDITOR
            string displayPrice = string.Empty;

            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(this.productCode))
            {
                displayPrice = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this.productCode].displayPrice;
                buttonText[0].text = displayPrice;
                buttonText[1].text = displayPrice;
            }
#endif
        }
        else
        {
            string baseDisplayPrice = in_data.prices[0];
            buttonText[0].transform.localPosition = new Vector3(18, 3, 0);
            buttonText[0].text = baseDisplayPrice;
            buttonText[1].text = baseDisplayPrice;
        }

        SetBuyCount();

        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Shop",
            (CheckPackageType_UseTimer()) ? $"{in_data.image}_popup" : in_data.image, OnLoadComplete);
        
        if (bUseReconfirm)
        {
            if (packageData.type == (int) PackageType.SpotCloverPack)
            {
                Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Shop",$"{in_data.image}_pre_reconfirm"
                    , (complete) =>
                    {
                        textureReconfirm.mainTexture = complete;
                        textureReconfirm.gameObject.SetActive(true);
                        textureReconfirm.MakePixelPerfect();
                    });
            }
        }
    }
    public void InitPopup_SpotDia(int layer, CdnSpotDiaPackage spotDiaData)
    {
        InitSpine(layer, ResourceType.SPOT_DIA_SPINE);
        
        if (_spineObject == null)
        {
            return;
        }
        
        // (5번째)마지막 패키지라면 커진 노이 애니메이션 연출로 재생
        var skinIndex    = spotDiaData.segMin + 1;
        var noySkinIndex = skinIndex == 5 ? 2 : 1;

        _spineObject.AnimationName = $"appear_{noySkinIndex}";
        _spineObject.initialSkinName = skinIndex.ToString();
        _spineObject.Initialize(true);
        _spineObject.AnimationState.Complete += delegate
        {
            _spineObject.loop          = true;
            _spineObject.AnimationName = $"idle_{noySkinIndex}";
        };
        _spineObject.transform.localPosition = new Vector3(0, -140f, 0);
    }

    /// <summary>
    /// 패키지 팝업 내 스파인 생성 함수
    /// </summary>
    private void InitSpine(int layer, ResourceType resourceType)
    {
        GameObject spineObject = null;
        ManagerResourceLoader.instance.resourceDic.TryGetValue(resourceType, out spineObject);
        var startLayer = layer >= 10 ? layer : 10;
        if (spineObject != null)
        {
            _spineObject                         = Instantiate(spineObject, mainSprite.transform).GetComponent<SkeletonAnimation>();
            _spineObject.transform.localScale    = Vector3.one * 100f;
            _spineObject.transform.localPosition = Vector3.zero;
            _spineObject.GetComponent<MeshRenderer>().sortingOrder = startLayer + 1;
        }
        
        // 스파인 레이어 세팅
        _panelSpineUpper.sortingOrder = startLayer    + 2;
        _panelSpineUpper.depth        = uiPanel.depth + 1;
        
        // 팝업 레이어 정렬값 세팅
        sortOrderCount += 2;
        panelCount     += 2;
    }

    private void SetBuyCount()
    {
        if (bUseBuyCount == false)
            return;
        var shopPackage = ServerRepos.UserShopPackages.Find(x => x.idx == packageData.idx);
        int buyCount    = shopPackage?.buyCount ?? 0;
        //this.buyLimitText.SetText(buyLimitText[0].text.Replace("[n]", $"[FFFF00]{buyCount}[-]/{packageData.buyLimit}"));
        this.buyLimitText.SetText( $"{Global._instance.GetString("p_pack_3")}  ([FEF402]{packageData.buyLimit - buyCount}[-]/{packageData.buyLimit})" );

        this.buyLimitRootObject.SetActive(this.packageData.buyLimit > 1);
    }
    
    public void OnLoadComplete(Texture2D r)
    {
        texture.mainTexture = r;
        loadingText.gameObject.SetActive(false);
    }

    void Update()
    {
        loadingText.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }

    void OnClickButton()
    {
        if (this.bCanTouch == false)
            return;

        if (this.packageData.payType == 0 && string.IsNullOrEmpty(this.productCode))
        {
            return;
        }

        if (ManagerAdventure.CheckStartable() == false && packageData.HaveAdventureItem())
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_1"), false);
            popup.SetResourceImage("Message/tired");
            popup.SortOrderSetting();
            return;
        }
        
        List<int> gachaIds = packageData.GetGachaId();
        if (gachaIds.Count > 0)
        {
            StartCoroutine(PackageBuyRoutine.CanGachaCheck(gachaIds, this,
               () => {
                   OpenPopupConfirmPurchase();
               },
               () => {
                   UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                   popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_2"), false);
                   popup.SetResourceImage("Message/tired");
                   popup.SortOrderSetting();
               }));
        }
        else
        {
            OpenPopupConfirmPurchase();
        }

        //그로씨
        if (PackageBuyRoutine.packageSuggestedAtLogin)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                null,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
    }

    void OpenPopupConfirmPurchase()
    {
        bCanTouch = true;
        string name = string.Format("pack_{0}", packageData.title_msg == 0 ? packageId : packageData.title_msg);
        string message = Global._instance.GetString("n_b_9").Replace("[1]", Global._instance.GetString(name));
        
        string priceText = "";
        if (packageData.payType == 0)
        {
            List<string> priceList = new List<string>();
            foreach (var prices in packageData.prices)
                priceList.Add(prices.ToString());
            priceText = LanguageUtility.GetPrices(priceList);
            double price = LanguageUtility.GetPrice(priceList);
            string currency = string.Empty;
#if !UNITY_EDITOR
#if UNITY_IPHONE
            string _productCode = packageData.sku_i;
#elif UNITY_ANDROID
            string _productCode = packageData.sku_a;
#endif
            string displayPrice = string.Empty;

            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(_productCode))
            {
                displayPrice = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].displayPrice;
                priceText = displayPrice;
                price = double.Parse(ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].price, System.Globalization.CultureInfo.InvariantCulture);
                currency = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[_productCode].currency;
            }
#endif
            ManagerUI._instance.OpenMinorCheckPopup(price, currency, () => { OpenPurchaseConfirmPopup(message, priceText); });
        }
        else
        {
            priceText = packageData.prices[0];
            OpenPurchaseConfirmPopup(message, priceText);
        }
        
    }

    private void OpenPurchaseConfirmPopup(string message, string priceText)
    {
        if (bUseConfirmPopup == false)
        {
            ConfirmPurchaseDia();
            return;
        }
        
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, ConfirmPurchaseDia, true);

        if (ServerContents.Packages[packageId].type != 2) //2 : 스팟다이아
        {
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, price_type: (PackagePriceType)packageData.payType, price_value: priceText);
            popupSystem.ShowBuyInfo(packageData.payType != 0 ? "buyinfo_dpk_1" : "buyinfo_pk_1");
        }
        else
        {
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        }
        popupSystem.SetResourceImage(messagePackageIconPath);
    }

    int usePay = 0;
    int useFree = 0;
    private void ConfirmPurchaseDia()
    {
        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

        if(this.packageData.payType == 1)
        {
            int purchasePrice = (int)(System.Convert.ToDouble(packageData.prices[0]));

            if (Global.jewel < (int)(System.Convert.ToDouble(packageData.prices[0])))
            {
                this.bCanTouch = true;
                ManagerUI._instance.LackDiamondsPopUp();
                return;
            }

            if ((int)ServerRepos.User.jewel >= purchasePrice)
            {
                usePay = purchasePrice;
            }
            else if ((int)ServerRepos.User.jewel > 0)
            {
                usePay = (int)ServerRepos.User.jewel;
                useFree = purchasePrice - (int)ServerRepos.User.jewel;
            }
            else
            {
                useFree = purchasePrice;
            }

            ServerAPI.BuyPackageByJewel(packageData.idx, this.productCode, OnPostPurchase);
            return;
        }

#if UNITY_EDITOR
        ServerUserShopPackage pack = new ServerUserShopPackage();
        pack.idx = 1;
        pack.vsn = 1;
        pack.sku = "a_pkv_0_1";
        ServerRepos.UserShopPackages.Add(pack);
        ManagerUI._instance.UpdateUI();
#endif

        CheckBillingState();
    }

    private void CheckBillingState()
    {
        ManagerNotice.instance.ShowMajorNotice(RequestPurchase, OnCheckBillingStateFail);
    }

    private void OnCheckBillingStateFail(Trident.Error error)
    {
        // 터치 가능.
        this.bCanTouch = true;
        Extension.PokoLog.Log("============GetNotice error");
        this.ShowFailedPopup();
    }

    HashSet<long> prevInbox = new HashSet<long>();

    private void RequestPurchase()
    {
        prevInbox.Clear();
        if( packageData.HaveGacha() )
        {
            ServerAPI.Inbox(
                (resp) => {
                    
                    if( resp.IsSuccess )
                    {
                        for(int i = 0; i < resp.inbox.Count; ++i)
                        {
                            prevInbox.Add(resp.inbox[i].index);
                        }

                        ServiceSDK.ServiceSDKManager.instance.PurchasePackage(this.packageId, this.productCode, OnPurchase);
                    }
                    else
                    {
                        this.bCanTouch = true;
                        ShowFailedPopup();
                    }
                });

        }
        else
        {
            ServiceSDK.ServiceSDKManager.instance.PurchasePackage(this.packageId, this.productCode, OnPurchase);
        }
    }

    /// <summary>
    /// 구입 콜백
    /// </summary>
    /// <param name="isSuccess"></param>
    private void OnPurchase(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
            Extension.PokoLog.Log("============Billing error");
            this.ShowFailedPopup();
            this.bCanTouch = true;
            return;
        }

        ServerAPI.BuyShopPackage(this.packageId, this.productCode, orderId, OnPostPurchase);


#if !UNITY_EDITOR
        //MAT로그 전송
        ServiceSDK.BillingProductInfo billingProductInfo = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this.productCode];

        if (billingProductInfo == null)
        {
            return;
        }
        
        ServiceSDK.AdjustManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency);
#endif
    }

    /// <summary>
    /// 게임서버에서 결과 받기
    /// </summary>
    /// <param name="resp"></param>
    private void OnPostPurchase(Protocol.BaseResp resp)
    {
        this.bCanTouch = true;

        if (resp.IsSuccess == false)
        {
            Extension.PokoLog.Log("============PostPurchase error");
            ShowFailedPopup();
            return;
        }

        if (this.packageData.payType == 1)
        {
            var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                   (
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_PACKAGE,
                   -usePay,
                   -useFree,
                   (int)(ServerRepos.User.jewel),
                   (int)(ServerRepos.User.fjewel),
                   this.productCode
                   );
            var docDia = JsonConvert.SerializeObject(GetDia);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
        }

        if (this.packageData.paidJewel > 0 || this.packageData.freeJewel > 0)
        {
            var GetDia = new ServiceSDK.GrowthyCustomLog_Money
                   (
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                   ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                   this.packageData.paidJewel,
                   this.packageData.freeJewel,
                   (int)(ServerRepos.User.jewel),
                   (int)(ServerRepos.User.fjewel),
                   this.productCode
                   );
            var docDia = JsonConvert.SerializeObject(GetDia);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
        }

        foreach (var item in this.packageData.items)
        {
            if (item.type == (int)RewardType.coin)
            {
                var GetDia = new ServiceSDK.GrowthyCustomLog_Money(
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                    item.value,
                    0,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    this.productCode
                    );
                var docDia = JsonConvert.SerializeObject(GetDia);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);

            }
            else if (item.type == (int)RewardType.FreeCoin)
            {
                var GetDia = new ServiceSDK.GrowthyCustomLog_Money (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                    0,
                    item.value,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    this.productCode
                    );
                var docDia = JsonConvert.SerializeObject(GetDia);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docDia);
            }
            else
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)item.type,
                    item.value,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                    this.productCode
               );
            }
        }
        
        if(bAutoOpen)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                this.productCode,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
        
        if(packageData.type == (int)PackageType.SpotCloverPack)
        {
            var    mode  = Global.GameInstance.GetGrowthyGameMode();
            string stage = mode == ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL ? Global.stageIndex.ToString() : Global.GameInstance.GetGrowthyStageIndex();
      
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPOT_CLOVER_PACKAGE,
                "SPOT_CLOVER_PACKAGE_PURCHASE",
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                str1:$"{mode}_{Global.eventIndex}_{stage}"
            );
            var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }

        //획득한 상품 ui에 갱신
        Global.jewel = GameData.User.AllJewel;
        Global.clover = GameData.User.AllClover;
        Global.coin = GameData.User.AllCoin;
        Global.star = GameData.User.Star;
        ManagerUI._instance.UpdateUI();
        ManagerUI._instance.PackageUpdate();
        
        // 2019.05.24 : 패키지 구매 후 오토가챠 삭제
        //string completeCallbackString = packageData.GetGachaId() != 0 ? "PostProcess_AutoGacha" : "OnClickBtnClose_Base";
        
        var    shopPackage            = ServerRepos.UserShopPackages.Find(x => x.idx == packageData.idx);
        int    buyCount               = shopPackage?.buyCount ?? 0;
        string completeCallbackString = buyCount < packageData.buyLimit ? "SetBuyCount" : "OnClickBtnClose_Base";

        string name = Global._instance.GetString("p_t_4");
        string message = Global._instance.GetString("n_b_14");
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.FunctionSetting(1, completeCallbackString, gameObject, true);
        popup.FunctionSetting(3, completeCallbackString, gameObject, true);
        popup.FunctionSetting(4, completeCallbackString, gameObject, true);
        popup.SortOrderSetting();
        popup.InitSystemPopUp(name, message, false);
        popup.SetResourceImage(messagePackageIconPath);
    }

    /// <summary>
    /// 구입 실패시 알림 팝업 열기
    /// </summary>
    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        popup.SetResourceImage(messageFailIconPath);
    }

    protected override void OnClickBtnClose()
    {
        if (bUseReconfirm && bCanReconfirm)
        {
            // 스팟다이아 재확인 이미지 변경 (스파인)
            if (packageData.type == (int)PackageType.SpotDiaPack)
            {
                if (_spineObject != null && !_spineObject.AnimationName.Contains("idle"))
                {
                    return;
                }
                bCanReconfirm = false;
                textureReconfirm.gameObject.AddressableAssetLoad<Texture>($"local_ui/package_reconfirm_bubble", (x) =>
                {
                    textureReconfirm.gameObject.SetActive(true);
                    textureReconfirm.mainTexture = x;
                });

                if (_spineObject != null)
                {
                    var closeAnim = _spineObject.AnimationName.Contains("2") ? "close_2" : "close_1";
                    _spineObject.AnimationName = closeAnim;
                    _spineObject.AnimationState.Complete += delegate
                    {
                        _spineObject.AnimationName = closeAnim;
                    };
                }
            }

            // 스팟클로버 재확인 이미지 변경 (이미지)
            if (packageData.type == (int) PackageType.SpotCloverPack)
            {
                bCanReconfirm = false;
                Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Shop", $"{packageData.image}_reconfirm"
                    , (complete) =>
                    {
                        textureReconfirm.mainTexture = complete;
                        textureReconfirm.MakePixelPerfect();
                    });
            }
            return;
        }
        if (bUseReconfirm && bCanReconfirm == false)
        {
            if (packageData.type == (int)PackageType.SpotDiaPack)
            {
                OnClickBtnClose_Base();
                ManagerResourceLoader.instance.UnLoadResource(ResourceType.SPOT_DIA_SPINE, true);
            }
            if (packageData.type == (int)PackageType.SpotCloverPack)
            {
                _callbackClose = () =>
                {
                    ManagerUI._instance.OpenPopup<UIPopupShop>((popup) => popup.Init(UIPopupShop.ShopType.Clover));
                };
            }
        }
            
        //그로씨
        if(ManagerLobby._instance.IsLobbyComplete == false)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                    null,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FAIL
                );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
        
        base.OnClickBtnClose();
        
    }

    protected void PostProcess_AutoGacha()
    {
        ServerAPI.Inbox(
            (resp) => {
                if (resp.IsSuccess)
                {
                    for(int i = 0; i < resp.inbox.Count; ++i)
                    {
                        // 시작할 때 확인했던 우편함에서 못봤던 가챠티켓이 보이면, 냅다 까고보기
                        // 일단 이 코드대로라면 1개 이상의 가챠티켓은 못까는게 정상
                        if( !prevInbox.Contains(resp.inbox[i].index) )
                        {
                            if( resp.inbox[i].type == (int)RewardType.gachaTicket )
                            {
                                ServerAPI.ReceiveMail((int)resp.inbox[i].index, 
                                    (recvResp) => {
                                        if( recvResp.IsSuccess )
                                        {
                                            this._callbackClose = delegate
                                            {
                                                ManagerAdventure.OnInit((bool b) =>
                                                {
                                                    ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                                                    {
                                                        animalIdx = recvResp.userAdvAnimal.animalId,
                                                        exp = recvResp.userAdvAnimal.exp,
                                                        gettime = 0,
                                                        grade = recvResp.userAdvAnimal.grade,
                                                        level = recvResp.userAdvAnimal.level,
                                                        overlap = recvResp.userAdvAnimal.Overlap
                                                    };

                                                    var newAnimalInstance = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                                                    ManagerAdventure.User.SyncFromServer_Animal();
                                                    ManagerAIAnimal.Sync();
                                                    ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimalInstance, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                                                });
                                            };
                                        }

                                        OnClickBtnClose_Base();
                                    });
                                break;
                            }
                        }
                    }
                }
            });
    }

    void OnClickBtnClose_Base()
    {
        base.OnClickBtnClose();
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
