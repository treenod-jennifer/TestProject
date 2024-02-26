using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PokoAddressable;
using ServiceSDK;

public class UIItemSpecialDiaShop : MonoBehaviour
{
    [SerializeField] private UIUrlTexture texDiaItem;

    [SerializeField] private UILabel labelOriginCount;
    [SerializeField] private UILabel labelBonusCount;
    [SerializeField] private UILabel labelAllCount;
    [SerializeField] private UILabel[] labelDiaPrice;

    [SerializeField] private GameObject objBuyBtn;
    [SerializeField] private GameObject objBuyCompletionBtn;
    [SerializeField] private GameObject objItemLine;

    private string _productCode = string.Empty;

    CdnMailShopGoods shopGoods = new CdnMailShopGoods();

    bool IsBuyButton
    {
        set
        {
            objBuyBtn.SetActive(value);
            objBuyCompletionBtn.SetActive(!value);
        }
    }

    public void InitData(CdnMailShopGoods mailShopGood)
    {
        shopGoods = mailShopGood;

        this.gameObject.AddressableAssetLoad<Texture>($"local_ui/special_diamond_shop_dia{mailShopGood.idx}",
            (x) => texDiaItem.mainTexture = x);
        
        _productCode = this.GetProductCode(mailShopGood);

        IsBuyButton = ServerRepos.UserMailShop.purchaseStatus[mailShopGood.idx - 1] == 0;

        labelOriginCount.text = $"{mailShopGood.p_jewel}";
        labelBonusCount.text = $"{mailShopGood.f_jewel}";
        labelAllCount.text = $"x{mailShopGood.p_jewel + mailShopGood.f_jewel}";
        labelDiaPrice.SetText(GetDiaPrice());
    }

    public void CoverLastLine()
    {
        objItemLine.SetActive(false);
    }

    string GetDiaPrice()
    {
        string productCode = String.Empty;
        
#if UNITY_IPHONE
        productCode = shopGoods.sku_i;
#elif UNITY_ANDROID
        productCode = shopGoods.sku_a;
#else
        productCode = shopGoods.sku_a;
#endif
        
#if !UNITY_EDITOR
        if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
            return ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[productCode].displayPrice;
#endif
        return LanguageUtility.GetPrices(shopGoods.price);
    }
    
    private double GetPrice()
    {
        string productCode = String.Empty;
        
#if UNITY_IPHONE
        productCode = shopGoods.sku_i;
#elif UNITY_ANDROID
        productCode = shopGoods.sku_a;
#else
        productCode = shopGoods.sku_a;
#endif
        
#if !UNITY_EDITOR
        if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
            return double.Parse(ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[productCode].price, System.Globalization.CultureInfo.InvariantCulture);
#endif
        return LanguageUtility.GetPrice(shopGoods.price);
    }
    
    private string GetCurrency()
    {
        string productCode = String.Empty;
        
#if UNITY_IPHONE
        productCode = shopGoods.sku_i;
#elif UNITY_ANDROID
        productCode = shopGoods.sku_a;
#else
        productCode = shopGoods.sku_a;
#endif
        
#if !UNITY_EDITOR
        if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
            return ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[productCode].currency;
#endif
        return string.Empty;
    }


    void OnClickBtnPurchase()
    {
        if (UIPopupSpecialDiaShop._instance.bCanTouch == false)
            return;
        
        ManagerUI._instance.OpenMinorCheckPopup(GetPrice(), GetCurrency(), OpenPurchaseConfirmPopup);
    }

    private void OpenPurchaseConfirmPopup()
    {
        string message = Global._instance.GetString("n_b_1").Replace("[1]", Global._instance.GetString("item_2"));
        message = message.Replace("[n]", labelAllCount.text);
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmPurchaseDia", gameObject);
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popupSystem.SetResourceImage("Message/jewel");
    }

    private void ConfirmPurchaseDia()
    {
        if (UIPopupSpecialDiaShop._instance.bCanTouch == false)
            return;
        UIPopupSpecialDiaShop._instance.bCanTouch = false;

        CheckBillingState();
    }

    private void CheckBillingState()
    {
        ManagerNotice.instance.ShowMajorNotice(RequestPurchase, OnCheckBillingStateFail);
    }

    private void OnCheckBillingStateFail(Trident.Error error)
    {
        //터치 가능.
        UIPopupSpecialDiaShop._instance.bCanTouch = true;
        Extension.PokoLog.Log("============GetNotice error");
        this.ShowFailedPopup();
    }

    private void RequestPurchase()
    {
        ServiceSDK.ServiceSDKManager.instance.Purchase(this._productCode, OnPurchase);
    }

    private void OnPurchase(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
            //터치 가능.
            UIPopupSpecialDiaShop._instance.bCanTouch = true;
            Extension.PokoLog.Log("============Billing error");
            this.ShowFailedPopup();
            return;
        }

        ServerAPI.PostPurchase(this._productCode, OnPostPurchase);


#if !UNITY_EDITOR
        //MAT로그 전송
        ServiceSDK.BillingProductInfo billingProductInfo = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this._productCode];

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
        //터치 가능.
        UIPopupSpecialDiaShop._instance.bCanTouch = true;
        if (resp.IsSuccess == false)
        {
            Extension.PokoLog.Log("============PostPurchase error");
            ShowFailedPopup();
            return;
        }

        //전체 상품 판매 시 메일 샵 카운트 감소
        if (ServerRepos.UserMailShop.purchaseStatus.FindIndex(x => x == 0) == -1)
            ServerRepos.MailCnt--;

        //스페셜 다이아 샵 버튼 초기화
        IsBuyButton = ServerRepos.UserMailShop.purchaseStatus[shopGoods.idx - 1] == 0;
        
        Global.jewel = (int)(GameData.User.AllJewel);
        ManagerUI._instance.UpdateUI();

        //메일 함 초기화
        UIPopupMailBox._instance.SetMailBox();

        string message = Global._instance.GetString("n_b_3").Replace("[1]", Global._instance.GetString("item_2"));
        message = message.Replace("[n]", labelAllCount.text);

        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popup.SetResourceImage("Message/jewel");

        //그로씨
        {
            var jewelData = shopGoods;
            int origin = jewelData.p_jewel;
            int bonus = jewelData.f_jewel;

            var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_AT_SHOP,
                    origin,
                    bonus,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                _productCode
                );
            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }

    }

    /// <summary>
    /// 상품 id 가져오기
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetProductCode(CdnMailShopGoods cdnMailShop)
    {
#if UNITY_IPHONE
        return cdnMailShop.sku_i;
#elif UNITY_ANDROID
        return cdnMailShop.sku_a;
#else
        return cdnMailShop.sku_a;
#endif
    }

    /// <summary>
    /// 구입 실패시 알림 팝업 열기
    /// </summary>
    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        popup.SetResourceImage("Message/jewel");
    }
}