using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PokoAddressable;

public class UIPopupBuyWingUp : UIPopupBase
{
    public static UIPopupBuyWingUp _instance = null;

    [SerializeField] private UITexture texWing;
    [SerializeField] private UILabel wingPrice_Exp;
    [SerializeField] private UILabel wingPrice_Exp_Shadow;
    

    private int WingPrice_Exp
    {
        set
        {
            wingPrice_Exp.text = value.ToString();
            wingPrice_Exp_Shadow.text = value.ToString();
        }
    }

    private Global.UseMoneyData currentUesJewelData;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            bShowTopUI = true;
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);

        WingPrice_Exp = ServerRepos.LoginCdn.wingExtPrice;
        this.gameObject.AddressableAssetLoad<Texture>("local_ui/shop_icon_wing_up", (x) => texWing.mainTexture = x);
    }

    void OnClickBtnOk()
    {
        string text = Global._instance.GetString("p_wi_up_2");
        MakePurchasePopUP(text);
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        base.ClosePopUp(_mainTime, callback);

        if (_instance == this)
            _instance = null;
    }

    void MakePurchasePopUP(string text)
    {
        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
        popupSystem.SetCallbackSetting(1, OnClickBtnBuyWingConfirm, true);
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, price_type: PackagePriceType.Jewel, price_value: ServerRepos.LoginCdn.wingExtPrice.ToString());
        popupSystem.SetResourceImage("local_ui/shop_icon_wing_up");
        popupSystem.ShowBuyInfo("buyinfo_wup_1");
    }

    void OnClickBtnBuyWingConfirm()
    {
        int price = ServerRepos.LoginCdn.wingExtPrice;

        if (CheckCanPurchase(price) == true)
        {
            currentUesJewelData = Global._instance.UseJewel(price);

            ServerAPI.BuyWingExtend(
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    ManagerSound.AudioPlay(AudioInGame.PRAISE1);
                    PopUpPurchaseSucceed();

                    //UI업데이트.
                    Global.clover = (int)(ServerRepos.User.AllClover);
                    Global.jewel = (int)(ServerRepos.User.AllJewel);
                    Global.coin = (int)(ServerRepos.User.AllCoin);
                    Global.wing = (int)(ServerRepos.User.AllWing);
                    Global.exp = (int)GameData.User.expBall;
                    ManagerUI._instance.UpdateUI();

                    ClosePopUp();

                    SendGrowthyLog_Item_WingPlus2();
                }
                else
                {
                    ClosePopUp();
                    ManagerUI._instance.LackDiamondsPopUp();
                }
            }
            );
        }
        else
        {
            ClosePopUp();
            ManagerUI._instance.LackDiamondsPopUp();
        }
    }

    bool CheckCanPurchase(int price)
    {
        int assetValue = Global.jewel;
        if (assetValue < price)
        {
            return false;
        }
        return true;
    }

    void PopUpPurchaseSucceed()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        string text = Global._instance.GetString("n_b_13");
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
        popupSystem.SetResourceImage("Message/ok");
        UIItemShopWing._instance?.SetWingUpgradeObject(false);
    }

    private void SendGrowthyLog_Item_WingPlus2()
    {
        string itemCode = ServerRepos.LoginCdn.WingExtSale != 0 ? "adv_wing_plus2_s" : "adv_wing_plus2";

        var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
        (
            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_WING,
            currentUesJewelData.usePMoney * -1,
            currentUesJewelData.useFMoney * -1,
            (int)(ServerRepos.User.jewel),
            (int)(ServerRepos.User.fjewel),
            itemCode
        );
        var docMoney = JsonConvert.SerializeObject(growthyMoney);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);

        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
        (
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
            "WING_PLUS2",
            itemCode,
            1,
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
        );
        var doc = JsonConvert.SerializeObject(useReadyItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

        var useWingItem = new ServiceSDK.GrowthyCustomLog_ITEM
              (
                 ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                  "WING_PLUS2",
                  itemCode,
                  -1,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
              );
        var wingDoc = JsonConvert.SerializeObject(useWingItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", wingDoc);
    }
}
