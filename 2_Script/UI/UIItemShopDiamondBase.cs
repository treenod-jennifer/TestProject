using System;
using System.Linq;
using UnityEngine;
using ServiceSDK;
using Newtonsoft.Json;

public abstract class UIItemShopDiamondBase : MonoBehaviour
{
    protected Action onCompletePurchase;
    
    [SerializeField] protected UIUrlTexture _diaIcon;      // 다이아 아이콘
    [SerializeField] protected UILabel      _originCount;  // 기본 지급 다이아
    [SerializeField] protected UILabel      _bonusCount;   // 보너스 지급 다이아
    [SerializeField] protected UILabel[]    _allCount;     // 총 지급 다이아
    [SerializeField] protected UILabel[]    _displayPrice; // 가격

    protected string productCode = string.Empty;
    protected int    itemIndex;

    private CdnShopJewel.JewelItemBase _jewelData;

    private CdnShopJewel.JewelItemBase JewelData
    {
        get
        {
            if (_jewelData == null)
            {
                _jewelData = GetJewelData(itemIndex);
            }

            return _jewelData;
        }
    }

    private string _diaText;

    private string DiaText
    {
        get
        {
            if (_diaText == null)
            {
                _diaText = Global._instance.GetString("item_2");
            }

            return _diaText;
        }
    }

    public abstract    void   InitItemDia(int index);
    protected abstract string GetAllCount();
    protected abstract void   RequestPurchase(Action<bool, string> onComplete);
    protected abstract void   RequestBuyDiaShopItem();

    protected abstract GrowthyCustomLog_Money.Code_L_MRSN GetGrowthy_MSRN();

    /// <summary>
    /// 상품 데이터 리턴
    /// </summary>
    protected abstract CdnShopJewel.JewelItemBase GetJewelData(int index);

    /// <summary>
    /// 상품 id 리턴
    /// </summary>
    protected string GetProductCode(int index)
    {
#if UNITY_IPHONE
        return JewelData.sku_i;
#elif UNITY_ANDROID
        return JewelData.sku_a;
#else
        return JewelData.sku_a;
#endif
    }

    /// <summary>
    /// 기본 지급 다이아 갯수 리턴
    /// </summary>
    protected int GetOriginDiaCount() => JewelData.pJewel;

    /// <summary>
    /// 보너스 다이아 갯수 리턴
    /// </summary>
    protected int GetBonusDiaCount() => JewelData.fJewel;

    /// <summary>
    /// 가격 리턴
    /// </summary>
    private string GetDisplayPrice()
    {
        var displayPrice = LanguageUtility.GetPrices(JewelData.prices.ToList());

#if !UNITY_EDITOR
        displayPrice = string.Empty;
        if (ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
        {
            displayPrice = ServiceSDKManager.instance.billingProductInfoDic[productCode].displayPrice;
        }
#endif
        return displayPrice;
    }

    #region UI
    protected void SetDiaIconTexture(int index)
    {
        _diaIcon.SettingTextureScale(_diaIcon.width, _diaIcon.height);
        _diaIcon.LoadCDN(Global.gameImageDirectory, "IconShopItem/diamond", $"shop_icon_dia_{(index):D2}");
    }

    protected void SetDiaOriginCountLabel() => _originCount.text = GetOriginDiaCount().ToString();
    protected void SetDiaBonusCountLabel()  => _bonusCount.text = GetBonusDiaCount().ToString();
    protected void SetDisplayPriceLabel()   => _displayPrice.SetText(GetDisplayPrice());
    #endregion

    #region 구매 관련 처리
    /// <summary>
    /// 구매 버튼 클릭
    /// </summary>
    protected void OnClickPurchaseButton()
    {
        if (UIPopupShop._instance.bCanTouch == false)
        {
            return;
        }

        var price    = LanguageUtility.GetPrice(JewelData.prices.ToList());
        var currency = string.Empty;

#if !UNITY_EDITOR
        if (ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(productCode))
        {
            price    = double.Parse(ServiceSDKManager.instance.billingProductInfoDic[productCode].price, System.Globalization.CultureInfo.InvariantCulture);
            currency = ServiceSDKManager.instance.billingProductInfoDic[productCode].currency;
        }
#endif

        // 미성년자 보호 팝업 출력
        ManagerUI._instance.OpenMinorCheckPopup(price, currency, OpenPurchaseConfirmPopup);
    }

    /// <summary>
    /// 구매 의사 재확인 팝업 출력
    /// </summary>
    private void OpenPurchaseConfirmPopup()
    {
        var message = Global._instance.GetString("n_b_1").Replace("[1]", DiaText);
        message = message.Replace("[n]", GetAllCount());

        var popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmPurchaseDia", gameObject);
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popupSystem.SetResourceImage("Message/jewel");
    }

    /// <summary>
    /// 강제/점검 확인 후 서버에 OrderID 요청 후 스토어 구매 진행
    /// </summary>
    protected void ConfirmPurchaseDia()
    {
        if (UIPopupShop._instance.bCanTouch == false)
        {
            return;
        }

        UIPopupShop._instance.bCanTouch = false;

        // 구매 전 강제 업데이트, 점검이 걸려 있는지 확인
        ManagerNotice.instance.ShowMajorNotice(OnCheckBillingStateComplete, OnCheckBillingStateFail);
        
        void OnCheckBillingStateComplete() => RequestPurchase(CompletePurchaseInApp);

        void OnCheckBillingStateFail(Trident.Error error)
        {
            UIPopupShop._instance.bCanTouch = true;
            Extension.PokoLog.Log("============GetNotice error");
            ShowFailedPopup();
        }
    }

    /// <summary>
    /// 스토어 구매 성공 후 보상 갱신을 위해 서버 API 호출
    /// </summary>
    private void CompletePurchaseInApp(bool isSuccess, string orderId)
    {
        if (isSuccess == false)
        {
            //터치 가능.
            UIPopupShop._instance.bCanTouch = true;
            Extension.PokoLog.Log("============Billing error");
            ShowFailedPopup();
            return;
        }

        RequestBuyDiaShopItem();

#if !UNITY_EDITOR
        //MAT로그 전송
        var billingProductInfo = ServiceSDKManager.instance.billingProductInfoDic[productCode];

        if (billingProductInfo == null)
        {
            return;
        }

        AdjustManager.instance.OnPurchase(billingProductInfo.price, billingProductInfo.currency);
#endif
    }

    /// <summary>
    /// 서버 API 호출 후 구매 완료 처리
    /// </summary>
    protected void PurchaseComplete(Protocol.BaseResp resp)
    {
        UIPopupShop._instance.bCanTouch = true;

        if (resp.IsSuccess == false)
        {
            Extension.PokoLog.Log("============PostPurchase error");
            ShowFailedPopup();
            return;
        }

        Global.jewel = (int)(GameData.User.AllJewel);
        ManagerUI._instance.UpdateUI();

        var message = Global._instance.GetString("n_b_3").Replace("[1]", DiaText);
        message = message.Replace("[n]", _allCount[0].text);

        var popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popup.SetResourceImage("Message/jewel");

        // 구매 완료 후 Growthy MONEY Log 전송
        {
            var playEnd = new GrowthyCustomLog_Money
            (
                GrowthyCustomLog_Money.Code_L_TAG.FC,
                GetGrowthy_MSRN(),
                JewelData.pJewel,
                JewelData.fJewel,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                productCode
            );
            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
        }
        
        onCompletePurchase?.Invoke();
    }

    /// <summary>
    /// 구매 실패 팝업 출력
    /// </summary>
    private void ShowFailedPopup()
    {
        var popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        popup.SetResourceImage("Message/jewel");
    }
    #endregion
}