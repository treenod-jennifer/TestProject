using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopupPackage : UIPopupBase, IImageRequestable
{
    public static UIPopupPackage _instance = null;

    public UITexture texture;
    public UILabel loadingText;
    public UILabel[] buttonText;

    //Notice _data = null;
    private string productCode = string.Empty;
    private int packageId = 0;
    void Awake()
    {
        _instance = this;
    }/*
    public void InitPopUp(CdnShopPackage in_data, Method.FunctionVoid func = null)
    {
        this._callbackEnd += func;
        this.packageId = in_data.idx;

        if( in_data.type == 2 )
        {
            string prefKey = string.Format("spotDia_{0}", in_data.idx);
            PlayerPrefs.SetInt(prefKey, 1);
        }
        
#if UNITY_IPHONE
        this.productCode = in_data.sku_i;
#elif UNITY_ANDROID
        this.productCode = in_data.sku_a;
#endif

        string baseDisplayPrice = Global._systemLanguage == CurrLang.eJap ? "￥" + in_data.prices[1] : "＄" + in_data.prices[0];
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

        UIImageLoader.Instance.Load(Global.gameImageDirectory, "Shop/", in_data.image, this);

    }*/
    public void OnLoadComplete(ImageRequestableResult r)
    {
        texture.mainTexture = r.texture;
        loadingText.gameObject.SetActive(false);
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }

    void Update()
    {
        loadingText.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }

    void OnClickButton()
    {
        if (this.bCanTouch == false)
            return;

        if (string.IsNullOrEmpty(this.productCode))
        {
            return;
        }

        bCanTouch = true;
        string name = string.Format("pack_{0}", packageId);
        string message = Global._instance.GetString("n_b_9").Replace("[1]", Global._instance.GetString(name));
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmPurchaseDia", gameObject);
        Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, texture);
    }
    /*
    private void ConfirmPurchaseDia()
    {
        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

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
    */
    private void CheckBillingState()
    {
       // ServiceSDK.ServiceSDKManager.instance.GetNotice(OnCheckBillingState);

    }


    private void RebootApplication()
    {
        Global.ReBoot();
    }


    private void RequestPurchase()
    {
       // ServiceSDK.ServiceSDKManager.instance.PurchasePackage(this.packageId, this.productCode, OnPurchase);
    }

    /// <summary>
    /// 구입 콜백
    /// </summary>
    /// <param name="isSuccess"></param>
    private void OnPurchase(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
           // Extension.PokoLog.Log("============Billing error");
            this.ShowFailedPopup();
            this.bCanTouch = true;
            return;
        }

        //ServerAPI.BuyShopPackage(this.packageId, this.productCode, orderId, OnPostPurchase);

        /*
#if !UNITY_EDITOR
        //MAT로그 전송
        ServiceSDK.BillingProductInfo billingProductInfo = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this.productCode];

        if (billingProductInfo == null)
        {
            return;
        }
        
        ServiceSDK.MATManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency);
        ServiceSDK.MATManager.instance.OnPayUser();

        ServiceSDK.FacebookSDKManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency, this.productCode, ServiceSDK.ServiceSDKManager.instance.GetPurchaseContentTypeForFacebook(this.productCode));
#endif*/
    }

    /// <summary>
    /// 게임서버에서 결과 받기
    /// </summary>
    /// <param name="resp"></param>
    /*
    private void OnPostPurchase(Protocol.BaseResp resp)
    {
        this.bCanTouch = true;

        if (resp.IsSuccess == false)
        {
          //  Extension.PokoLog.Log("============PostPurchase error");
            ShowFailedPopup();
            return;
        }

     

        //획득한 상품 ui에 갱신
        Global.jewel = GameData.User.AllJewel;
        Global.clover = GameData.User.AllClover;
        Global.coin = GameData.User.AllCoin;
        Global.star = GameData.User.Star;
        ManagerUI._instance.UpdateUI();

        string name = "メッセージ";
        string message = "全部購入したぞ！";
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.FunctionSetting(1, "OnClickBtnClose", gameObject, true);
        Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
        popup.SortOrderSetting();
        popup.InitSystemPopUp(name, message, false, texture);  
    }
    */
    /// <summary>
    /// 구입 실패시 알림 팝업 열기
    /// </summary>
    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false, texture);
    }

    private void GoToAppMarketAndReboot()
    {
       // Application.OpenURL(ServiceSDK.ServiceSDKManager.instance.GetMarketAppLink());
        Global.ReBoot();
    }

    protected override void OnClickBtnClose()
    {/*
        // 스팟다이아 패키지인 경우
        if( ServerContents.Packages.ContainsKey(this.packageId) &&
            ServerContents.Packages[packageId].type == 2 )
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popup.FunctionSetting(1, "AbandonPackage", gameObject, true);
            Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
            popup.SortOrderSetting();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_12"), false, texture);
        }
        else
        {
            base.OnClickBtnClose();
        }*/
    }

    void AbandonPackage()
    {
        base.OnClickBtnClose();
    }

}
