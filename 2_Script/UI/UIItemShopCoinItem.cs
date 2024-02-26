using UnityEngine;
using Newtonsoft.Json;

public class UIItemShopCoinItem : MonoBehaviour
{
    public UIUrlTexture coinIcon;
    public UILabel originCount;
    public UILabel bonusCount;
    public UILabel bonusCountSale;
    public UILabel[] allCount;
    public UILabel[] allCountSale;
    public UILabel[] coinPrice;
    public GameObject saleIcon;
    [SerializeField] private UISprite sprite_PriceItem;

    private CdnShopCoin.NormalItem _data = new CdnShopCoin.NormalItem();
    private string coinText = "";
    int purchasePrice = 0;

    public void InitItemCoin(CdnShopCoin.NormalItem data)
    {
        _data = data;
        purchasePrice = data.price;

        int origin = data.pCoin;
        int bonus = data.fCoin;

        originCount.text = origin.ToString();
        coinPrice[0].text = purchasePrice.ToString();
        coinPrice[1].text = purchasePrice.ToString();
        sprite_PriceItem.spriteName = $"icon_gem";;
        sprite_PriceItem.MakePixelPerfect();
        sprite_PriceItem.transform.localPosition = new Vector2(coinPrice[0].transform.localPosition.x - 38f - (coinPrice[0].text.Length * 10), sprite_PriceItem.transform.localPosition.y);
        coinIcon.LoadCDN(Global.gameImageDirectory, "IconShopItem/coin", $"shop_icon_coin_{(data.idx):D2}");
        coinIcon.MakePixelPerfect();
        coinIcon.transform.localScale = new Vector3(1.12f, 1.12f, 1.12f);

        if (data.saleLevel == 0)
        {
            SettingDefualt(origin, bonus);
        }
        else
        {
            SettingSale(origin, bonus);
        }
    }

    void SettingDefualt(int origin, int bonus)
    {
        allCount[0].text = (origin + bonus).ToString();
        allCount[1].text = (origin + bonus).ToString();
        bonusCount.text = bonus.ToString();
        saleIcon.SetActive(false);
    }

    void SettingSale(int origin, int bonus)
    {
        allCount[0].gameObject.SetActive(false);
        bonusCount.gameObject.SetActive(false);
        allCountSale[0].gameObject.SetActive(true);
        bonusCountSale.gameObject.SetActive(true);
        allCountSale[0].text = (origin + bonus).ToString();
        allCountSale[1].text = (origin + bonus).ToString();
        bonusCountSale.text = bonus.ToString();
        saleIcon.SetActive(true);
    }

    void OnClickBtnPurchase()
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;

        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, OnClickBtnConfirm, true);
        GetCoinText();
        string text = Global._instance.GetString("n_b_1").Replace("[1]", coinText);
        text = text.Replace("[n]", GetCoinCount());
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: PackagePriceType.Jewel, price_value: purchasePrice.ToString());
        popupSystem.SetResourceImage("Message/coin");
        popupSystem.ShowBuyInfo("buyinfo_cs_1");
    }

    string GetCoinCount()
    {
        if (allCount[0].gameObject.activeInHierarchy == true)
        {
            return allCount[0].text;
        }
        else
        {
            return allCountSale[0].text;
        }
    }

    void OnClickBtnConfirm()
    {
        if (UIPopupShop._instance.bCanTouch == false)
            return;
        UIPopupShop._instance.bCanTouch = false;

        //살 수 있는 돈이 되는지 검사.
        if (CheckCanPurchase() == true)
        {
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

            ServerAPI.ExchangeCoin(_data.idx, PurchaseCoin);
        }
        else
        {
            //터치 가능.
            OnTouch();
            ManagerUI._instance.LackDiamondsPopUp();
        }
    }

    bool CheckCanPurchase()
    {
        if (Global.jewel < purchasePrice)
        {
            return false;
        }
        return true;
    }

    int useFree = 0;
    int usePay = 0;
    void PurchaseCoin(Protocol.BaseResp resp)
    {
        //터치 가능.
        OnTouch();

        if (resp.IsSuccess)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            GetCoinText();
            string text = Global._instance.GetString("n_b_3").Replace("[1]", coinText);
            text = text.Replace("[n]", GetCoinCount());
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
            popupSystem.SetResourceImage("Message/ok");

            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            ManagerUI._instance.UpdateUI();
            
            //그로씨            
            {
                string productCode = "g_coin_";
                int IsSale = _data.saleLevel == 1 ? 2 : 1;
                productCode += IsSale.ToString();
                productCode += "_" + _data.idx;

                int origin = _data.pCoin;
                int bonus = _data.fCoin;

                int payClover = _data.pCoin;
                int freeClover = _data.fCoin;

                if (usePay > 0 && useFree == 0)
                {
                    payClover = _data.pCoin;
                    freeClover = _data.fCoin;
                }
                else if (usePay == 0 && useFree > 0)
                {
                    payClover = 0;
                    freeClover = _data.pCoin + _data.fCoin;
                }
                else if (usePay > 0 && useFree > 0)
                {
                    float payRatio = ((float)usePay) / ((float)useFree + (float)usePay);

                    payClover = usePay * 100;
                    freeClover = useFree * 100 + _data.fCoin;
                }

                var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_BY_1ST,
                    payClover,
                    freeClover,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    productCode
                    );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);

                var useJewel = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_GOLD,
                        -usePay,
                        -useFree,
                        (int)(ServerRepos.User.jewel),
                        (int)(ServerRepos.User.fjewel),
                        productCode
                        );
                var docJewel = JsonConvert.SerializeObject(useJewel);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docJewel);
            }
        }
        else
        {
            ShowFailedPopup();
        }
    }
    
    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false);
        popup.SetResourceImage("Message/tired");
    }

    private void GetCoinText()
    {
        if (coinText == "")
        {
            coinText = Global._instance.GetString("item_4");
        }
    }

    void OnTouch()
    {
        //터치 가능.
        UIPopupShop._instance.bCanTouch = true;
    }
}
