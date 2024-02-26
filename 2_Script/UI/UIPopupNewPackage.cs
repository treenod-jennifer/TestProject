using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UIPopupNewPackage : UIPopupBase
{
    [SerializeField] private UILabel [] priceLabel;
    [SerializeField] private GameObject priceDiaObj;
    [SerializeField] private UITexture textureBg;
    [SerializeField] private UILabel loadingText;
    [SerializeField] private UILabel[] buyLimitText;
    [SerializeField] private GameObject buyLimitRootObject;
    [SerializeField] private GameObject allSpriteRoot;
    [SerializeField] private UILabel labelBuyInfo;
    
    //특정 상거래법 버튼.
    [SerializeField] private UISprite spritePackageBack;
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    //자금결제법 버튼.
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    
    public static UIPopupNewPackage instance = null;
    private CdnShopPackage packageData = null;
    private PackageBuyRoutine buyRoutine = null;
    private string productCode = string.Empty;
    
    public bool bAutoOpen = false;

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
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    protected override void OnDestroy()
    {
        if (instance == this)
            instance = null;
        
        base.OnDestroy();
    }

    private void Update()
    {
        loadingText.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }

    public void InitPopup(CdnShopPackage packageData)
    {
        this.packageData = packageData;
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Shop", $"{packageData.image}_popup", OnLoadComplete);

        var shopPackage = ServerRepos.UserShopPackages.Find(x => x.idx == packageData.idx);
        int buyCount = shopPackage?.buyCount ?? 0;
        this.buyLimitText.SetText( $"{Global._instance.GetString("p_pack_3")}  ([FEF402]{packageData.buyLimit - buyCount}[-]/{packageData.buyLimit})" );
        this.buyLimitRootObject.SetActive(packageData.buyLimit > 1);

        SetPriceLabel();

        if (!LanguageUtility.IsShowBuyInfo)
        {
            RemoveTermsOfUseRoot();
            labelBuyInfo.gameObject.SetActive(false);
        }
        else
        {
            switch (packageData.type)
            {
                // cbu package
                case 9:
                    labelBuyInfo.text = Global._instance.GetString("buyinfo_cbpk_1");
                    break;
                // npu spot package
                case 10:
                    labelBuyInfo.text = Global._instance.GetString("buyinfo_nonpupk_1");
                    break;
                default:
                    labelBuyInfo.text = Global._instance.GetString("buyinfo_cbpk_1");
                    break;
            }
        }
    }
    public void OnLoadComplete(Texture2D r)
    {
        textureBg.mainTexture = r;
        loadingText.gameObject.SetActive(false);
    }
    
    private void SetPriceLabel()
    {
        if (packageData.payType == 0)
        {
            string baseDisplayPrice = LanguageUtility.GetPrices(packageData.prices);
            priceLabel[0].transform.localPosition = new Vector3(-6, 3, 0);
            priceLabel[0].text = baseDisplayPrice;
            priceLabel[1].text = baseDisplayPrice;
            priceDiaObj.SetActive(false);

#if !UNITY_EDITOR
#if UNITY_IPHONE
            productCode = packageData.sku_i;
#elif UNITY_ANDROID
            productCode = packageData.sku_a;
#endif
            string displayPrice = string.Empty;

            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
            {
                displayPrice = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[productCode].displayPrice;
                priceLabel[0].text = displayPrice;
                priceLabel[1].text = displayPrice;
            }
#endif
        }
        else
        {
            string baseDisplayPrice = packageData.prices[0];
            priceLabel[0].transform.localPosition = new Vector3(18, 3, 0);
            priceLabel[0].text = baseDisplayPrice;
            priceLabel[1].text = baseDisplayPrice;
            priceDiaObj.SetActive(true);
        }
    }

    private void RemoveTermsOfUseRoot()
    {
        spritePackageBack.gameObject.SetActive(false);

        int height = 60;
        mainSprite.transform.localPosition -= new Vector3(0f, height * 0.5f, 0);
        mainSprite.height -= height;
        allSpriteRoot.transform.localPosition -= new Vector3(0f, height * 0.5f, 0);
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
    
    /// <summary>
    /// 구매 버튼 클릭.
    /// </summary>
    private void OnClickPurchaseBtnClick()
    {
        if (!bCanTouch || packageData == null) return;
        
        if (buyRoutine == null)
        {
            buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
            buyRoutine.PostPurchaseCompleteEvent += OnPurchaseComplete;
        }
        
        string name = string.Format("pack_{0}", packageData.title_msg == 0 ? packageData.idx : packageData.title_msg);
        string message = Global._instance.GetString("n_b_9").Replace("[1]", name);
        
        buyRoutine.init(this, packageData, message, autoOpen: bAutoOpen);
        
        //패키지 구매.
        buyRoutine.OnClickButton();
    }
    
    private void OnPurchaseComplete()
    {
        //cbu팩의 경우 유저 카운트와 상관없이 유저 세그먼트에 따라 변경되므로 이번에 buylimit을 넘었을 때 expireTs를 0으로 설정하여 로비 갱신 시 배너가 뜨지 않도록 함
        if (packageData.type == 9)
            ServerRepos.UserCbuPackage.expiredAt = 0;
        ManagerUI._instance.PackageUpdate();
        
        string name = Global._instance.GetString("p_t_4");
        string message = Global._instance.GetString("n_b_14");
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.FunctionSetting(1, "OnClickBtnClose_Base", gameObject, true);
        popup.FunctionSetting(3, "OnClickBtnClose_Base", gameObject, true);
        popup.FunctionSetting(4, "OnClickBtnClose_Base", gameObject, true);
        popup.SortOrderSetting();
        popup.InitSystemPopUp(name, message, false);
        popup.SetResourceImage("Message/icon_buyPackage");
    }

    protected override void OnClickBtnClose()
    {
        if (!bCanTouch) return;
        
        // NPU 스팟성 패키지인 경우 종료 전에 시스템 팝업 1회 더 출력
        if( ServerContents.Packages.ContainsKey(packageData.idx) && ServerContents.Packages[packageData.idx].type == 10 )
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popup.SortOrderSetting();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_24"), true);
            popup.SetButtonText(1, Global._instance.GetString("btn_88"));
            popup.SetButtonText(2, Global._instance.GetString("btn_89"));
            popup.FunctionSetting(1, "OnClickBtnClose_Base", gameObject, true);
            popup.FunctionSetting(3, "OnClickBtnClose_Base", gameObject, true);
            popup.SetResourceImage("Message/icon_dontbuyPackage");
        }
        else
        {
            base.OnClickBtnClose();
        }
    }
    
    void OnClickBtnClose_Base()
    {
        if (!bCanTouch) return;
        
        base.OnClickBtnClose();
    }
}
