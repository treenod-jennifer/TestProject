using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class UIPopupCloverShop : UIPopupBase
{
    public UILabel[] cloverPrice;
    public UILabel[] freeTimeCloverPrice;
    public GameObject clover5Sale;
    public GameObject clover60TimeSale;

    //ios관련.
    public GameObject[] buttonList;
    public UITexture cloverBoni;
    public UITexture inviteIcon;
    public UITexture requestIcon;
    public UITexture clover5Icon;
    public UITexture cloverTimeIcon;

    public int[] purchasePrice = new int[2] { 10, 30 };

    private int itemIndex = -1;
    private string cloverText = "";
    
    public override void OpenPopUp(int _depth)
    {/*
        base.OpenPopUp(_depth);

        popupType = PopupType.cloverShop;

        cloverBoni.mainTexture = Resources.Load("UI/clover_boni") as Texture2D;
        inviteIcon.mainTexture = Resources.Load("UI/icon_invite") as Texture2D;
        requestIcon.mainTexture = Resources.Load("UI/icon_send") as Texture2D;
        Texture2D clover = Resources.Load("UI/shop_icon_clover") as Texture2D;
        clover5Icon.mainTexture = clover;
        cloverTimeIcon.mainTexture = clover;

        purchasePrice[0] = ServerRepos.LoginCdn.cloverPrice;
        purchasePrice[1] = ServerRepos.LoginCdn.cloverFreeTimePrice;

        cloverPrice[0].text = purchasePrice[0].ToString();
        cloverPrice[1].text = purchasePrice[0].ToString();
        freeTimeCloverPrice[0].text = purchasePrice[1].ToString();
        freeTimeCloverPrice[1].text = purchasePrice[1].ToString();

        if (ServerRepos.LoginCdn.clover5Sale == 1)
        {
            clover5Sale.SetActive(true);
        }
        else
        {
            clover5Sale.SetActive(false);
        }

        if (ServerRepos.LoginCdn.cloverFreeTimeSale == 1)
        {
            clover60TimeSale.SetActive(true);
        }
        else
        {
            clover60TimeSale.SetActive(false);
        }

#if UNITY_IOS
        if(ServerRepos.LoginCdn.EnableInvite == 0)
            SetButtonList_Ios();
#endif
*/
    }

    void OnClickBtnIviteFriend()
    {
        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

        /*if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            //터치 가능.
            OnTouch();
            ManagerUI._instance.GuestLoginSignInCheck(true, uiPanel.sortingOrder + 1);
        }
        else*/
        {
            bShopBehind = true;
            _callbackClose = ManagerUI._instance.OpenPopupInvite;
            //터치 가능.
            _callbackEnd += OnTouch;
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnRequestClover()
    {
        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;

        /*if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            //터치 가능.
            OnTouch();
            ManagerUI._instance.GuestLoginSignInCheck(true, uiPanel.sortingOrder + 1);
        }
        else*/
        {
            bShopBehind = true;
            _callbackClose = ManagerUI._instance.OpenPopupRequestClover;
            //터치 가능.
            _callbackEnd += OnTouch;
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnBuy5Clover()
    {
        if (this.bCanTouch == false)
            return;
        itemIndex = 0;
        GetCloverString();
        string text = Global._instance.GetString("n_b_1").Replace("[1]", cloverText);
        text = text.Replace("[n]", "5");
        MakePurchasePopUP(text);
    }

    void OnClickBtnBuyFreeTimeClover()
    {
        if (this.bCanTouch == false)
            return;
        itemIndex = 1;
        string text = Global._instance.GetString("n_b_2");
        MakePurchasePopUP(text);
    }

    void MakePurchasePopUP(string text)
    {
        if (this.bCanTouch == false)
            return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        // "YES" 버튼 함수 설정, 창 닫힌 후 함수 호출.
        popupSystem.FunctionSetting(1, "OnClickBtnConfirm", gameObject, true);
        Texture2D texture = Resources.Load("Message/clover") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, texture);
    }

    void OnClickBtnConfirm()
    {
        if (itemIndex < 0)
        { 
            //터치 가능.
            OnTouch();
            return;
        }

        if (this.bCanTouch == false)
            return;
        this.bCanTouch = false;
        /*
        //살 수 있는 돈이 되는지 검사.
        if (CheckCanPurchase(purchasePrice[itemIndex]) == true)
        {
            useIndex = itemIndex;
            if (useIndex >= 0)
            {
                if ((int)ServerRepos.User.jewel > purchasePrice[useIndex]) usePJewel = purchasePrice[useIndex];
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = purchasePrice[useIndex] - (int)ServerRepos.User.jewel;
                }
                else useFJewel = purchasePrice[useIndex];
            }
            ServerAPI.BuyClover(itemIndex, PurchaseClover);
        }
        else
        {
            ManagerUI._instance.LackDiamondsPopUp();
            //터치 가능.
            OnTouch();
            itemIndex = -1;
        }*/
    }

    int useFJewel = 0;
    int usePJewel = 0;


    bool CheckCanPurchase(int price)
    {
        if (Global.jewel < price)
        {   
            return false;
        }
        return true;
    }

    int useIndex = -1;
    /*
    void PurchaseClover(Protocol.BaseResp resp)
    {
        if (resp.IsSuccess)
        {
            PurchasePopUP();

            //UI업데이트.
            Global.clover = (int)(ServerRepos.User.AllClover);
            Global.jewel = (int)(ServerRepos.User.AllJewel);
            ManagerUI._instance.UpdateUI();


            //그로씨            
            {
                string productCode = "CLOVER";

                if (useIndex == 0) productCode += "_5";
                else if (useIndex == 1)   productCode += "_60m";  
                if(useIndex == 0 && ServerRepos.LoginCdn.clover5Sale == 1)   productCode += "_s";                
                else if (useIndex == 1 && ServerRepos.LoginCdn.cloverFreeTimeSale == 1)     productCode += "_s";
                
                if(useIndex == 0)
                {
                    var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BUY_BY_1ST,
                        0,
                        5,
                        0,//(int)(ServerRepos.User.clover),
                        (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover),
                        productCode
                        );
                    var doc = JsonConvert.SerializeObject(playEnd);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);
                }
                else
                {
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                              productCode,
                              "CLOVER_60m",
                              1,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM
                          );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

                    var useCloverItem = new ServiceSDK.GrowthyCustomLog_ITEM
                          (
                             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                              productCode,
                              "CLOVER_60m",
                              -1,
                              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM
                          );
                    var cloverDoc = JsonConvert.SerializeObject(useCloverItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", cloverDoc);

                }

                if (useIndex >= 0)
                {
                    var useJewelPurchaseClover = new ServiceSDK.GrowthyCustomLog_Money
                           (
                           ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                           ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_AP_CLOVAR,
                           -usePJewel,
                           -useFJewel,
                          (int)(ServerRepos.User.jewel),
                          (int)(ServerRepos.User.fjewel),
                           productCode
                           );
                    var docJewel = JsonConvert.SerializeObject(useJewelPurchaseClover);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docJewel);
                }
                
            }
        }
        else
        {
            ShowFailedPopup();
        }
        itemIndex = -1;
    }
    */
    void PurchasePopUP()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        string text = "";
        //클로버 5개 구매 시. 
        if (itemIndex == 0)
        {
            GetCloverString();
            text = Global._instance.GetString("n_b_3").Replace("[1]", cloverText);
            text = text.Replace("[n]", "5");
        }
        //FreeTime Clover 구매 시.
        else
        {
            text = Global._instance.GetString("n_b_8");
        }
        Texture2D texture = Resources.Load("Message/OK") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, texture, OnTouch);
    }

    void SetButtonList_Ios()
    {
        cloverBoni.transform.localPosition = new Vector3(0f, 190f, 0f);
        buttonList[0].SetActive(false);
        for (int i = 1; i < buttonList.Length; i++)
        {
            float yPos = 87f - (160f * i);
            buttonList[i].transform.localPosition = new Vector3(0f, yPos, 0f);
        }
    }

    void GetCloverString()
    {
        if (cloverText == "")
        {
            cloverText = Global._instance.GetString("item_1");
        }
    }

    void OnTouch()
    {
        //터치 가능.
        this.bCanTouch = true;
    }

    private void ShowFailedPopup()
    {
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/clover") as Texture2D;
        popup.SortOrderSetting();
        popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_4"), false, texture, OnTouch);
    }
}
